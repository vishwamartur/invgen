using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvGen.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string searchTerm)
        {
            IEnumerable<Customer> customers;
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = await _customerService.SearchCustomersAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                customers = await _customerService.GetAllCustomersAsync();
            }

            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                // Validate GST number format if provided
                if (!string.IsNullOrEmpty(customer.GstNumber))
                {
                    if (!IsValidGstNumber(customer.GstNumber))
                    {
                        ModelState.AddModelError("GstNumber", "Please enter a valid GST number (e.g., 22AAAAA0000A1Z5).");
                    }
                }

                // Check for duplicate email
                if (await _customerService.EmailExistsAsync(customer.Email))
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                }

                // Check for duplicate company name if provided
                if (!string.IsNullOrEmpty(customer.CompanyName))
                {
                    var existingCustomers = await _customerService.SearchCustomersAsync(customer.CompanyName);
                    if (existingCustomers.Any(c => c.CompanyName?.Equals(customer.CompanyName, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        ModelState.AddModelError("CompanyName", "A customer with this company name already exists.");
                    }
                }

                if (ModelState.IsValid)
                {
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
                    if (!string.IsNullOrEmpty(customer.State))
                    {
                        customer.State = CapitalizeName(customer.State);
                    }

                    // Format GST number
                    if (!string.IsNullOrEmpty(customer.GstNumber))
                    {
                        customer.GstNumber = customer.GstNumber.ToUpper().Replace(" ", "");
                    }

                    customer.CreatedDate = DateTime.UtcNow;
                    await _customerService.CreateCustomerAsync(customer);

                    TempData["SuccessMessage"] = $"Customer '{customer.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = customer.Id });
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the customer. Please try again.";
            }

            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if email already exists for another customer
                if (await _customerService.EmailExistsAsync(customer.Email, customer.Id))
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    return View(customer);
                }

                await _customerService.UpdateCustomerAsync(customer);
                TempData["SuccessMessage"] = "Customer updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // API endpoint for customer search (for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> SearchApi(string term)
        {
            var customers = await _customerService.SearchCustomersAsync(term);
            var result = customers.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                email = c.Email,
                company = c.CompanyName,
                phone = c.Phone
            });
            return Json(result);
        }

        // Helper method to validate GST number format
        private static bool IsValidGstNumber(string gstNumber)
        {
            if (string.IsNullOrEmpty(gstNumber))
                return false;

            // Remove spaces and convert to uppercase
            gstNumber = gstNumber.Replace(" ", "").ToUpper();

            // GST number format: 2 digits + 10 alphanumeric + 1 letter + 1 digit + 1 letter + 1 alphanumeric
            // Example: 22AAAAA0000A1Z5
            if (gstNumber.Length != 15)
                return false;

            // Check pattern: 2 digits + 10 chars + 1 letter + 1 digit + 1 letter + 1 char
            return System.Text.RegularExpressions.Regex.IsMatch(gstNumber, @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}[Z]{1}[0-9A-Z]{1}$");
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
