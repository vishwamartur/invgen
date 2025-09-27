using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvGen.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsApiController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts(
            string? search, 
            int? categoryId, 
            ServiceType? serviceType)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _productService.SearchProductsAsync(search, serviceType);
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else if (serviceType.HasValue)
            {
                products = await _productService.GetProductsByServiceTypeAsync(serviceType.Value);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.Description,
                p.UnitPrice,
                p.Unit,
                p.Brand,
                p.Model,
                p.IsService,
                Category = new
                {
                    p.Category.Id,
                    p.Category.Name,
                    p.Category.ServiceType
                }
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var result = new
            {
                product.Id,
                product.Name,
                product.Code,
                product.Description,
                product.UnitPrice,
                product.Unit,
                product.Brand,
                product.Model,
                product.Specifications,
                product.IsService,
                Category = new
                {
                    product.Category.Id,
                    product.Category.Name,
                    product.Category.ServiceType,
                    product.Category.Description
                }
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if product code already exists
            if (await _productService.ProductCodeExistsAsync(product.Code))
            {
                return BadRequest(new { error = "A product with this code already exists." });
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, new { id = createdProduct.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if product code already exists for another product
            if (await _productService.ProductCodeExistsAsync(product.Code, product.Id))
            {
                return BadRequest(new { error = "A product with this code already exists." });
            }

            try
            {
                await _productService.UpdateProductAsync(product);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var exists = await _productService.ProductExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchProducts(
            string term, 
            ServiceType? serviceType)
        {
            var products = await _productService.SearchProductsAsync(term, serviceType);
            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.UnitPrice,
                p.Unit,
                p.Brand,
                p.IsService,
                Category = p.Category.Name
            });

            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories(ServiceType? serviceType)
        {
            IEnumerable<ProductCategory> categories;

            if (serviceType.HasValue)
            {
                categories = await _productService.GetCategoriesByServiceTypeAsync(serviceType.Value);
            }
            else
            {
                categories = await _productService.GetAllCategoriesAsync();
            }

            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.ServiceType,
                ProductCount = c.Products.Count(p => p.IsActive)
            });

            return Ok(result);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<object>> GetCategory(int id)
        {
            var category = await _productService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var result = new
            {
                category.Id,
                category.Name,
                category.Description,
                category.ServiceType,
                Products = category.Products.Where(p => p.IsActive).Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.UnitPrice,
                    p.Unit,
                    p.IsService
                })
            };

            return Ok(result);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<object>> CreateCategory(ProductCategory category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCategory = await _productService.CreateCategoryAsync(category);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, new { id = createdCategory.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, ProductCategory category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _productService.UpdateCategoryAsync(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _productService.DeleteCategoryAsync(id);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
