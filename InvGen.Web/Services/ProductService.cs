using InvGen.Web.Data;
using InvGen.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InvGen.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly InvGenDbContext _context;

        public ProductService(InvGenDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByServiceTypeAsync(ServiceType serviceType)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && 
                    (p.Category.ServiceType == serviceType || p.Category.ServiceType == ServiceType.Both))
                .OrderBy(p => p.Category.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.CreatedDate = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            product.UpdatedDate = DateTime.UtcNow;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            product.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, ServiceType? serviceType = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return serviceType.HasValue 
                    ? await GetProductsByServiceTypeAsync(serviceType.Value)
                    : await GetAllProductsAsync();

            searchTerm = searchTerm.ToLower();
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && 
                    (p.Name.ToLower().Contains(searchTerm) ||
                     p.Code.ToLower().Contains(searchTerm) ||
                     (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                     (p.Brand != null && p.Brand.ToLower().Contains(searchTerm))));

            if (serviceType.HasValue)
            {
                query = query.Where(p => p.Category.ServiceType == serviceType.Value || 
                                        p.Category.ServiceType == ServiceType.Both);
            }

            return await query
                .OrderBy(p => p.Category.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<bool> ProductCodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Products.Where(p => p.Code == code && p.IsActive);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        // Category methods
        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync()
        {
            return await _context.ProductCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.ServiceType)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesByServiceTypeAsync(ServiceType serviceType)
        {
            return await _context.ProductCategories
                .Where(c => c.IsActive && 
                    (c.ServiceType == serviceType || c.ServiceType == ServiceType.Both))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<ProductCategory> CreateCategoryAsync(ProductCategory category)
        {
            category.CreatedDate = DateTime.UtcNow;
            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<ProductCategory> UpdateCategoryAsync(ProductCategory category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.ProductCategories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
