using InvGen.Web.Data;
using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InvGen.Web.Controllers
{
    public class QuotationsController : Controller
    {
        private readonly IQuotationService _quotationService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;

        public QuotationsController(
            IQuotationService quotationService,
            ICustomerService customerService,
            IProductService productService,
            IPdfService pdfService,
            IConfiguration configuration)
        {
            _quotationService = quotationService;
            _customerService = customerService;
            _productService = productService;
            _pdfService = pdfService;
            _configuration = configuration;
        }

        // GET: Quotations
        public async Task<IActionResult> Index(string searchTerm, QuotationStatus? status)
        {
            IEnumerable<Quotation> quotations;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                quotations = await _quotationService.SearchQuotationsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else if (status.HasValue)
            {
                quotations = await _quotationService.GetQuotationsByStatusAsync(status.Value);
                ViewBag.Status = status;
            }
            else
            {
                quotations = await _quotationService.GetAllQuotationsAsync();
            }

            ViewBag.StatusList = new SelectList(Enum.GetValues<QuotationStatus>());
            return View(quotations);
        }

        // GET: Quotations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        // GET: Quotations/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();

            var quotation = new Quotation
            {
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                TaxRate = decimal.Parse(_configuration["TaxSettings:DefaultGstRate"] ?? "18"),
                IsTaxInclusive = bool.Parse(_configuration["TaxSettings:TaxInclusiveByDefault"] ?? "false"),
                Status = QuotationStatus.Draft,
                TermsAndConditions = "1. Prices are valid for 30 days from quotation date.\n2. Payment terms: 50% advance, 50% on completion.\n3. GST extra as applicable.\n4. Material delivery charges extra if applicable.",
                LineItems = new List<QuotationLineItem>()
            };

            return View(quotation);
        }

        // POST: Quotations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quotation quotation, List<QuotationLineItem> lineItems)
        {
            try
            {
                // Remove empty line items
                lineItems = lineItems?.Where(li => li.ProductId > 0 && li.Quantity > 0).ToList() ?? new List<QuotationLineItem>();

                if (lineItems.Count == 0)
                {
                    ModelState.AddModelError("", "Please add at least one line item to the quotation.");
                }

                // Additional validation
                if (quotation.CustomerId <= 0)
                {
                    ModelState.AddModelError("CustomerId", "Please select a customer.");
                }

                if (quotation.QuotationDate == default(DateTime))
                {
                    ModelState.AddModelError("QuotationDate", "Please enter a valid quotation date.");
                }

                if (ModelState.IsValid)
                {
                    // Set line items
                    quotation.LineItems = lineItems;

                    // Set sort order for line items
                    var lineItemsList = quotation.LineItems.ToList();
                    for (int i = 0; i < lineItemsList.Count; i++)
                    {
                        lineItemsList[i].SortOrder = i + 1;
                    }
                    quotation.LineItems = lineItemsList;

                    // Create quotation
                    await _quotationService.CreateQuotationAsync(quotation);

                    TempData["SuccessMessage"] = $"Quotation {quotation.QuotationNumber} created successfully.";
                    return RedirectToAction(nameof(Details), new { id = quotation.Id });
                }
            }
            catch (Exception ex)
            {
                // Log the detailed error for debugging
                Console.WriteLine($"Error creating quotation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"An error occurred while creating the quotation: {ex.Message}";

                // Add model state error for debugging
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            // If we got this far, something failed, redisplay form
            await PopulateDropdowns();
            quotation.LineItems = lineItems ?? new List<QuotationLineItem>();
            return View(quotation);
        }

        // GET: Quotations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            await PopulateDropdowns();
            return View(quotation);
        }

        // POST: Quotations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quotation quotation)
        {
            if (id != quotation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _quotationService.UpdateQuotationAsync(quotation);
                await _quotationService.UpdateQuotationTotalsAsync(quotation.Id);
                TempData["SuccessMessage"] = "Quotation updated successfully.";
                return RedirectToAction(nameof(Details), new { id = quotation.Id });
            }

            await PopulateDropdowns();
            return View(quotation);
        }

        // GET: Quotations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        // POST: Quotations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _quotationService.DeleteQuotationAsync(id);
            TempData["SuccessMessage"] = "Quotation deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Quotations/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, QuotationStatus status)
        {
            var success = await _quotationService.UpdateQuotationStatusAsync(id, status);
            if (success)
            {
                TempData["SuccessMessage"] = $"Quotation status updated to {status}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update quotation status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Quotations/Pdf/5
        public async Task<IActionResult> Pdf(int id)
        {
            try
            {
                // First check if quotation exists
                var quotation = await _quotationService.GetQuotationByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound($"Quotation with ID {id} not found.");
                }

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(quotation);
                var fileName = $"Quotation_{quotation.QuotationNumber}_{DateTime.Now:yyyyMMdd}.pdf";

                // Set proper headers for PDF download
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                // Log the error (you might want to add proper logging here)
                Console.WriteLine($"ArgumentException in PDF generation: {ex.Message}");
                return NotFound($"Error generating PDF: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the error (you might want to add proper logging here)
                Console.WriteLine($"Exception in PDF generation: {ex.Message}");
                return StatusCode(500, $"Internal server error while generating PDF: {ex.Message}");
            }
        }



        // POST: Quotations/AddLineItem
        [HttpPost]
        public async Task<IActionResult> AddLineItem(QuotationLineItem lineItem)
        {
            if (ModelState.IsValid)
            {
                await _quotationService.AddLineItemAsync(lineItem);
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // POST: Quotations/UpdateLineItem
        [HttpPost]
        public async Task<IActionResult> UpdateLineItem(QuotationLineItem lineItem)
        {
            if (ModelState.IsValid)
            {
                await _quotationService.UpdateLineItemAsync(lineItem);
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // POST: Quotations/DeleteLineItem/5
        [HttpPost]
        public async Task<IActionResult> DeleteLineItem(int id)
        {
            var success = await _quotationService.DeleteLineItemAsync(id);
            return Json(new { success });
        }

        // API: Get Product Details
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new
            {
                success = true,
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    code = product.Code,
                    unitPrice = product.UnitPrice,
                    unit = product.Unit,
                    description = product.Description,
                    brand = product.Brand,
                    category = product.Category?.Name
                }
            });
        }

        // API: Get Customer Details
        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            return Json(new
            {
                success = true,
                customer = new
                {
                    id = customer.Id,
                    name = customer.Name,
                    companyName = customer.CompanyName,
                    email = customer.Email,
                    phone = customer.Phone,
                    mobile = customer.Mobile,
                    address = customer.Address,
                    city = customer.City,
                    state = customer.State,
                    postalCode = customer.PostalCode,
                    gstNumber = customer.GstNumber
                }
            });
        }

        // API: Get Products by Category
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            var productList = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                code = p.Code,
                unitPrice = p.UnitPrice,
                unit = p.Unit,
                description = p.Description,
                brand = p.Brand
            }).ToList();

            return Json(new { success = true, products = productList });
        }

        // API: Create Customer Inline
        [HttpPost]
        public async Task<IActionResult> CreateCustomerInline([FromBody] Customer customer)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(customer.Name) || string.IsNullOrWhiteSpace(customer.Email))
                {
                    return Json(new { success = false, message = "Name and Email are required." });
                }

                // Check for duplicate email
                if (await _customerService.EmailExistsAsync(customer.Email))
                {
                    return Json(new { success = false, message = "A customer with this email already exists." });
                }

                // Auto-capitalize names
                customer.Name = CapitalizeName(customer.Name);
                if (!string.IsNullOrEmpty(customer.CompanyName))
                {
                    customer.CompanyName = CapitalizeName(customer.CompanyName);
                }
                if (!string.IsNullOrEmpty(customer.City))
                {
                    customer.City = CapitalizeName(customer.City);
                }

                customer.CreatedDate = DateTime.UtcNow;
                customer.IsActive = true;

                var createdCustomer = await _customerService.CreateCustomerAsync(customer);

                return Json(new
                {
                    success = true,
                    message = "Customer created successfully.",
                    customer = new
                    {
                        id = createdCustomer.Id,
                        name = createdCustomer.Name,
                        companyName = createdCustomer.CompanyName,
                        email = createdCustomer.Email,
                        phone = createdCustomer.Phone,
                        address = createdCustomer.Address,
                        city = createdCustomer.City,
                        state = createdCustomer.State,
                        postalCode = createdCustomer.PostalCode
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating the customer." });
            }
        }

        // API: Create Product Inline
        [HttpPost]
        public async Task<IActionResult> CreateProductInline([FromBody] Product product)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.Name) || product.CategoryId <= 0)
                {
                    return Json(new { success = false, message = "Product name and category are required." });
                }

                // Auto-capitalize product name
                product.Name = CapitalizeName(product.Name);
                product.CreatedDate = DateTime.UtcNow;
                product.IsActive = true;

                var createdProduct = await _productService.CreateProductAsync(product);

                return Json(new
                {
                    success = true,
                    message = "Product created successfully.",
                    product = new
                    {
                        id = createdProduct.Id,
                        name = createdProduct.Name,
                        description = createdProduct.Description,
                        unitPrice = createdProduct.UnitPrice,
                        unit = createdProduct.Unit,
                        categoryId = createdProduct.CategoryId,
                        code = createdProduct.Code
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating the product." });
            }
        }

        // API: Create Product Category Inline
        [HttpPost]
        public async Task<IActionResult> CreateCategoryInline([FromBody] ProductCategory category)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return Json(new { success = false, message = "Category name is required." });
                }

                // Auto-capitalize category name
                category.Name = CapitalizeName(category.Name);
                category.CreatedDate = DateTime.UtcNow;
                category.IsActive = true;

                var createdCategory = await _productService.CreateCategoryAsync(category);

                return Json(new
                {
                    success = true,
                    message = "Category created successfully.",
                    category = new
                    {
                        id = createdCategory.Id,
                        name = createdCategory.Name,
                        description = createdCategory.Description,
                        serviceType = createdCategory.ServiceType
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating the category." });
            }
        }

        private async Task PopulateDropdowns()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            ViewBag.CustomerId = new SelectList(customers.Where(c => c.IsActive), "Id", "Name");

            var categories = await _productService.GetAllCategoriesAsync();
            ViewBag.Categories = categories.Where(c => c.IsActive).ToList();

            var products = await _productService.GetAllProductsAsync();
            ViewBag.Products = products.Where(p => p.IsActive).ToList();

            ViewBag.StatusList = new SelectList(Enum.GetValues<QuotationStatus>());
        }

        // Test endpoint to check database and quotations
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var quotations = await _quotationService.GetAllQuotationsAsync();
                var customers = await _customerService.GetAllCustomersAsync();
                var products = await _productService.GetAllProductsAsync();

                return Json(new {
                    success = true,
                    quotationsCount = quotations.Count(),
                    customersCount = customers.Count(),
                    productsCount = products.Count(),
                    quotations = quotations.Take(5).Select(q => new {
                        q.Id,
                        q.QuotationNumber,
                        q.CustomerId,
                        CustomerName = q.Customer?.Name,
                        q.QuotationDate,
                        q.Status,
                        LineItemsCount = q.LineItems?.Count ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Test endpoint to create a simple quotation
        public async Task<IActionResult> TestCreateQuotation()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var products = await _productService.GetAllProductsAsync();

                if (!customers.Any() || !products.Any())
                {
                    return Json(new { success = false, error = "No customers or products available for testing" });
                }

                var customer = customers.First();
                var product = products.First();

                var quotation = new Quotation
                {
                    CustomerId = customer.Id,
                    QuotationDate = DateTime.Now,
                    ProjectName = "Test Project",
                    Description = "Test quotation created via API",
                    Status = QuotationStatus.Draft,
                    LineItems = new List<QuotationLineItem>
                    {
                        new QuotationLineItem
                        {
                            ProductId = product.Id,
                            Quantity = 2,
                            UnitPrice = product.UnitPrice,
                            Unit = product.Unit,
                            Description = product.Description
                        }
                    }
                };

                var createdQuotation = await _quotationService.CreateQuotationAsync(quotation);

                return Json(new {
                    success = true,
                    quotationId = createdQuotation.Id,
                    quotationNumber = createdQuotation.QuotationNumber,
                    message = "Test quotation created successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Debug endpoint to check form data
        [HttpPost]
        public async Task<IActionResult> DebugFormData()
        {
            try
            {
                var formData = new Dictionary<string, object>();

                foreach (var key in Request.Form.Keys)
                {
                    formData[key] = Request.Form[key].ToString();
                }

                return Json(new {
                    success = true,
                    formData = formData,
                    contentType = Request.ContentType,
                    method = Request.Method
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Helper method to capitalize names properly
        private static string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        }
    }
}
