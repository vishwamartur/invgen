using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InvGen.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchTerm, int? categoryId, ServiceType? serviceType)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = await _productService.SearchProductsAsync(searchTerm, serviceType);
                ViewBag.SearchTerm = searchTerm;
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }
            else if (serviceType.HasValue)
            {
                products = await _productService.GetProductsByServiceTypeAsync(serviceType.Value);
                ViewBag.ServiceType = serviceType;
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            // Populate dropdowns
            var categories = await _productService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.ServiceTypes = new SelectList(Enum.GetValues<ServiceType>());

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Check if product code already exists
                if (await _productService.ProductCodeExistsAsync(product.Code))
                {
                    ModelState.AddModelError("Code", "A product with this code already exists.");
                    await PopulateDropdowns();
                    return View(product);
                }

                await _productService.CreateProductAsync(product);
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await PopulateDropdowns();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if product code already exists for another product
                if (await _productService.ProductCodeExistsAsync(product.Code, product.Id))
                {
                    ModelState.AddModelError("Code", "A product with this code already exists.");
                    await PopulateDropdowns();
                    return View(product);
                }

                await _productService.UpdateProductAsync(product);
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // API endpoint for product search (for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> SearchApi(string term, ServiceType? serviceType)
        {
            var products = await _productService.SearchProductsAsync(term, serviceType);
            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                code = p.Code,
                unitPrice = p.UnitPrice,
                unit = p.Unit,
                category = p.Category.Name,
                isService = p.IsService
            });
            return Json(result);
        }

        // GET: Products/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            return View(categories);
        }

        private async Task PopulateDropdowns()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
        }
    }
}
