using InvGen.Web.Models;

namespace InvGen.Web.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsByServiceTypeAsync(ServiceType serviceType);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, ServiceType? serviceType = null);
        Task<bool> ProductExistsAsync(int id);
        Task<bool> ProductCodeExistsAsync(string code, int? excludeId = null);
        
        // Category methods
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<ProductCategory>> GetCategoriesByServiceTypeAsync(ServiceType serviceType);
        Task<ProductCategory?> GetCategoryByIdAsync(int id);
        Task<ProductCategory> CreateCategoryAsync(ProductCategory category);
        Task<ProductCategory> UpdateCategoryAsync(ProductCategory category);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
