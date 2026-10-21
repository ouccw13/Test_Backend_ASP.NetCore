using backendShoppingcart.DTOs;
using backendShoppingcart.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
// using backendShoppingcart.Services; 

namespace backendShoppingcart.Services;

public class CartService : ICartService
{
    private readonly IMongoCollection<Cart> _cartsCollection;
    private readonly IMongoCollection<Product> _productsCollection;

    public CartService(IOptions<MongoDbSettings> settings, IMongoDatabase database)
    {
        _cartsCollection = database.GetCollection<Cart>(settings.Value.CartsCollectionName);
        _productsCollection = database.GetCollection<Product>(settings.Value.ProductsCollectionName);
    }

    public async Task<Cart?> GetCartAsync(string? userId)

    {
        Cart cart = null;
        if(!string.IsNullOrEmpty(userId))
         cart = await _cartsCollection.Find(c => c.Id == userId).FirstOrDefaultAsync();

    

        if (cart == null)

        {

            return null; // หรือจะคืนค่า CartDto เปล่าๆ ก็ได้ตามดีไซน์ของคุณ

        }


        return cart;

    }

    public async Task<Cart> GetCartByUserIdAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))

    {

        // โค้ดดักกรณีไม่มี userId (เช่น คืนค่าเป็น null หรือสร้างตะกร้าชั่วคราว)

        return null; 

    }
        var cart = await _cartsCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>(),
                TotalItems = 0,
                TotalPrice = 0m,
                UpdatedAt = DateTime.UtcNow
            };
            await _cartsCollection.InsertOneAsync(cart);
        }
        return cart;
    }

    private async Task<CartDto> AddItemAsync(string cartId, string productId, int quantity)
{
    // 1. ตรวจสอบสินค้าในคลัง (MongoDB) ว่ามีเพียงพอไหม และตัดสต็อก
    var productFilter = Builders<Product>.Filter.And(
        Builders<Product>.Filter.Eq(p => p.Id, productId),
        Builders<Product>.Filter.Gte(p => p.StockQuantity, quantity)
    );

    var productUpdate = Builders<Product>.Update.Inc(p => p.StockQuantity, -quantity);

    var product = await _productsCollection.FindOneAndUpdateAsync(
        productFilter,
        productUpdate,
        new FindOneAndUpdateOptions<Product> { ReturnDocument = ReturnDocument.After }
    );

    if (product == null)
    {
        throw new InvalidOperationException("สินค้าหมด หรือจำนวนสินค้าในคลังไม่เพียงพอ");
    }

    // 2. ดึงข้อมูลตะกร้าปัจจุบัน (หรือถ้ายังไม่มีในขั้นตอนนี้ ให้สร้างใหม่)
    var cart = await _cartsCollection.Find(c => c.Id == cartId).FirstOrDefaultAsync();
    if (cart == null)
    {
        cart = new Cart { Id = cartId, Items = new List<CartItem>() };
    }

    // 3. เช็คว่ามีสินค้านี้อยู่ในตะกร้าหรือยัง ถ้ามีแล้วให้บวกจำนวนเพิ่ม
    var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
    if (existingItem != null)
    {
        existingItem.Quantity += quantity;
    }
    else
    {
        // ถ้ายังไม่มี ให้เพิ่มรายการใหม่เข้าไป
        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = product.Id,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            UnitPrice = product.Price,
            Quantity = quantity,
            Stock = product.StockQuantity
        });
    }

    // 4. คำนวณราคารวมทั้งหมดในตะกร้า
    CalculateCartTotals(cart);

    // 5. บันทึกข้อมูลตะกร้าที่อัปเดตแล้วกลับลง MongoDB (Upsert)
    var cartFilter = Builders<Cart>.Filter.Eq(c => c.Id, cartId);
    var options = new ReplaceOptions { IsUpsert = true };
    await _cartsCollection.ReplaceOneAsync(cartFilter, cart, options);

    // 6. แปลงเป็น DTO ส่งกลับไป
    return MapToDto(cart);
}

private CartDto MapToDto(Cart cart)
{
    if (cart == null) return null;

    return new CartDto
    {
        Id = cart.Id,
        Subtotal = cart.TotalPrice,
        // Discount = cart.Discount,
        // ShippingFee = cart.ShippingFee,
        Total = cart.TotalItems,
        Items = cart.Items?.Select(item => new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Name = item.Name,
            ImageUrl = item.ImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            Stock = item.Stock,
            TotalPrice  = item.UnitPrice * item.Quantity // คำนวณราคารวมต่อรายการ
        }).ToList() ?? new List<CartItemDto>()
    };
}

private void CalculateCartTotals(Cart cart)
{
    if (cart.Items == null || !cart.Items.Any())
    {
        cart.TotalItems = 0;
        // cart.Discount = 0;
        // cart.ShippingFee = 0;
        cart.TotalPrice = 0;
        return;
    }

    // 1. คำนวณราคารวมสินค้าทั้งหมด (Subtotal)
    // decimal priceDecimal = cart.Items.Sum(item => item.UnitPrice * item.Quantity);
    // int priceInt = (int)Math.Round(priceDecimal); 
    cart.Total = cart.Items.Sum(item => item.UnitPrice * item.Quantity);

    // 2. กำหนดค่าจัดส่ง (ตัวอย่าง: ถ้าซื้อครบ 1,000 บาท ส่งฟรี หรือตั้งค่าคงที่ เช่น 50 บาท)
    // cart.ShippingFee = cart.Subtotal >= 1000 ? 0 : 50;

    // 3. หายอดสุทธิ (Total) = ราคารวมสินค้า - ส่วนลด + ค่าจัดส่ง
    // cart.TotalItems = cart.to - cart.Discount + cart.ShippingFee;
    
    // ป้องกันไม่ให้ยอด Total ติดลบ
    if (cart.TotalItems < 0)
    {
        cart.TotalItems = 0;
    }
}

    public async Task<CartDto> AddItemToCartAsync(AddToCartDto dto, string userId)
    {
        // var product = await _productsCollection.Find(p => p.Id == dto.ProductId && p.IsActive).FirstOrDefaultAsync();
        // if (product == null)
        // {
        //     throw new KeyNotFoundException("ไม่พบสินค้าที่ต้องการ หรือสินค้านี้ไม่พร้อมจำหน่าย");
        // }

        // var cart = await GetCartByUserIdAsync(userId);
        // var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

        // var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;
        // if (product.StockQuantity < requestedTotalQuantity)
        // {
        //     throw new InvalidOperationException($"สินค้า '{product.Name}' คงเหลือในสต็อกไม่เพียงพอ (มี {product.StockQuantity} ชิ้น)");
        // }

        // if (existingItem != null)
        // {
        //     existingItem.Quantity = requestedTotalQuantity;
        //     existingItem.UnitPrice = product.Price;
        //     existingItem.SubTotal = existingItem.Quantity * existingItem.UnitPrice;
        // }
        // else
        // {
        //     cart.Items.Add(new CartItem
        //     {
        //         ProductId = product.Id!,
        //         ProductName = product.Name,
        //         UnitPrice = product.Price,
        //         Quantity = dto.Quantity,
        //         SubTotal = dto.Quantity * product.Price,
        //         ImageUrl = product.ImageUrl
        //     });
        // }

        // cart.RecalculateTotals();
        // await _cartsCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart, new ReplaceOptions { IsUpsert = true });
        // return cart;
        Cart cart = null;
        if (!string.IsNullOrEmpty(userId))
        {
             cart = await _cartsCollection.Find(c => c.Id == userId).FirstOrDefaultAsync();
        }
    
        if (cart == null)

        {
            cart = new Cart
            {
                Id = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId, 

                Items = new List<CartItem>() 
            };
            await _cartsCollection.InsertOneAsync(cart);
            // return null; // หรือจะคืนค่า CartDto เปล่าๆ ก็ได้ตามดีไซน์ของคุณ

        }

         string activeCartId = cart.Id;
         return await AddItemAsync(activeCartId, dto.ProductId, dto.Quantity);
    }

    public async Task<Cart> UpdateItemQuantityAsync(string productId, int quantity, string userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem == null)
        {
            throw new KeyNotFoundException("ไม่พบสินค้านี้ในตระกร้า");
        }

        if (quantity <= 0)
        {
            cart.Items.Remove(existingItem);
        }
        else
        {
            var product = await _productsCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new KeyNotFoundException("ไม่พบข้อมูลสินค้า");
            }

            if (product.StockQuantity < quantity)
            {
                throw new InvalidOperationException($"สินค้า '{product.Name}' มีในสต็อกเพียง {product.StockQuantity} ชิ้น");
            }

            existingItem.Quantity = quantity;
            existingItem.UnitPrice = product.Price;
            existingItem.SubTotal = existingItem.Quantity * existingItem.UnitPrice;
        }

        cart.RecalculateTotals();
        await _cartsCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return cart;
    }

    public async Task<Cart> RemoveItemFromCartAsync(string userId, string? productId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (itemToRemove != null)
        {
            cart.Items.Remove(itemToRemove);
            cart.RecalculateTotals();
            await _cartsCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        return cart;
    }

    public async Task<Cart> ClearCartAsync(string? userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        cart.Items.Clear();
        cart.RecalculateTotals();
        await _cartsCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return cart;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(string? userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart.Items == null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("ไม่สามารถเช็คเอาท์ได้เนื่องจากไม่มีสินค้าในตระกร้า");
        }

        // Validate stock for all items
        foreach (var item in cart.Items)
        {
            var product = await _productsCollection.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new KeyNotFoundException($"ไม่พบสินค้ารหัส {item.ProductId} ในระบบ");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"สินค้า '{product.Name}' คงเหลือไม่เพียงพอ (ต้องการ {item.Quantity} ชิ้น แต่มีในสต็อก {product.StockQuantity} ชิ้น)");
            }
        }

        // Deduct stock
        foreach (var item in cart.Items)
        {
            var update = Builders<Product>.Update
                .Inc(p => p.StockQuantity, -item.Quantity)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _productsCollection.UpdateOneAsync(p => p.Id == item.ProductId, update);
        }

        var orderSummary = new CheckoutResultDto
        {
            OrderId = "ORD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString()[..6].ToUpper(),
            UserId = userId,
            Items = new List<CartItem>(cart.Items),
            TotalItems = cart.TotalItems,
            TotalPrice = cart.TotalPrice,
            CheckoutAt = DateTime.UtcNow,
            Status = "Completed",
            Message = "ทำการสั่งซื้อและตัดสต็อกสินค้าเรียบร้อยแล้ว"
        };

        // Clear cart after checkout
        cart.Items.Clear();
        cart.RecalculateTotals();
        await _cartsCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart);

        return orderSummary;
    }
}
