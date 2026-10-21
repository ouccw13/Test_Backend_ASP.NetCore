using backendShoppingcart.DTOs;
using backendShoppingcart.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backendShoppingcart.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _productsCollection;

    public ProductService(IOptions<MongoDbSettings> settings, IMongoDatabase database)
    {
        _productsCollection = database.GetCollection<Product>(settings.Value.ProductsCollectionName);
    }

    public async Task<List<Product>> GetAllAsync(string? category = null)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filter = filterBuilder.Eq(p => p.IsActive, true);

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter &= filterBuilder.Eq(p => p.Category, category.Trim());
        }

        return await _productsCollection.Find(filter).ToListAsync();
    }

    private static FilterDefinition<Product> BuildProductFilter(string idOrProductId)
    {
        var builder = Builders<Product>.Filter;
        if (MongoDB.Bson.ObjectId.TryParse(idOrProductId, out _))
        {
            return builder.Or(builder.Eq(p => p.Id, idOrProductId), builder.Eq(p => p.ProductId, idOrProductId));
        }

        return builder.Eq(p => p.ProductId, idOrProductId);
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _productsCollection.Find(BuildProductFilter(id)).FirstOrDefaultAsync();
    }

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var productId = !string.IsNullOrWhiteSpace(dto.ProductId)
            ? dto.ProductId.Trim()
            : $"PROD-{(await _productsCollection.CountDocumentsAsync(_ => true) + 1):D3}";

        var product = new Product
        {
            ProductId = productId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "ทั่วไป" : dto.Category,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _productsCollection.InsertOneAsync(product);
        return product;
    }

    public async Task<Product?> UpdateAsync(string id, UpdateProductDto dto)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.ProductId))
        {
            existing.ProductId = dto.ProductId.Trim();
        }
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.StockQuantity = dto.StockQuantity;
        existing.Category = dto.Category;
        existing.ImageUrl = dto.ImageUrl;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _productsCollection.ReplaceOneAsync(p => p.Id == existing.Id, existing);
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        var result = await _productsCollection.DeleteOneAsync(p => p.Id == existing.Id);
        return result.DeletedCount > 0;
    }

    public async Task<List<Product>> SeedSampleProductsAsync()
    {
        var count = await _productsCollection.CountDocumentsAsync(_ => true);
        if (count > 0)
        {
            await EnsureProductIdsAssignedAsync();
            return await _productsCollection.Find(_ => true).ToListAsync();
        }

        var samples = new List<Product>
        {
            new Product
            {
                ProductId = "PROD-001",
                Name = "MacBook Pro 14 นิ้ว (M3)",
                Description = "ชิป Apple M3, RAM 16GB, SSD 512GB, จอภาพ Liquid Retina XDR",
                Price = 59900m,
                StockQuantity = 10,
                Category = "Electronics",
                ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500"
            },
            new Product
            {
                ProductId = "PROD-002",
                Name = "iPhone 16 Pro Max 256GB",
                Description = "สมาร์ทโฟนไทเทเนียม ดีไซน์พรีเมียม กล้อง 48MP ชิป A18 Pro",
                Price = 48900m,
                StockQuantity = 15,
                Category = "Electronics",
                ImageUrl = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=500"
            },
            new Product
            {
                ProductId = "PROD-003",
                Name = "หูฟัง Sony WH-1000XM5",
                Description = "หูฟังตัดเสียงรบกวนไร้สายระดับพรีเมียม แบตเตอรี่ใช้งานได้ 30 ชม.",
                Price = 11990m,
                StockQuantity = 20,
                Category = "Accessories",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500"
            },
            new Product
            {
                ProductId = "PROD-004",
                Name = "คีย์บอร์ดไร้สาย Mechanical Keyboard RGB",
                Description = "Custom Mechanical Keyboard สวิตช์ Hot-swappable เชื่อมต่อได้ 3 โหมด",
                Price = 3290m,
                StockQuantity = 25,
                Category = "Accessories",
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=500"
            },
            new Product
            {
                ProductId = "PROD-005",
                Name = "แก้วกาแฟสแตนเลสเก็บอุณหภูมิ 500ml",
                Description = "เก็บความร้อน-เย็นได้นาน 12 ชั่วโมง ผลิตจาก Stainless Steel 304",
                Price = 490m,
                StockQuantity = 50,
                Category = "Lifestyle",
                ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=500"
            }
        };

        await _productsCollection.InsertManyAsync(samples);
        return samples;
    }

    public async Task EnsureProductIdsAssignedAsync()
    {
        var products = await _productsCollection.Find(_ => true).ToListAsync();
        var index = 1;
        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.ProductId))
            {
                var assignedId = $"PROD-{index:D3}";
                var update = Builders<Product>.Update.Set(p => p.ProductId, assignedId);
                await _productsCollection.UpdateOneAsync(p => p.Id == product.Id, update);
            }
            index++;
        }
    }
}
