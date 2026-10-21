using backendShoppingcart.DTOs;
using backendShoppingcart.Models;

namespace backendShoppingcart.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(string? category = null);
    Task<Product?> GetByIdAsync(string id);
    Task<Product> CreateAsync(CreateProductDto dto);
    Task<Product?> UpdateAsync(string id, UpdateProductDto dto);
    Task<bool> DeleteAsync(string id);
    Task<List<Product>> SeedSampleProductsAsync();
    Task EnsureProductIdsAssignedAsync();
}
