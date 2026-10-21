using System.ComponentModel.DataAnnotations;
using backendShoppingcart.Models;

namespace backendShoppingcart.DTOs;

public class AddToCartDto
{
    [Required(ErrorMessage = "กรุณาระบุ ProductId")]
     public string? CartId { get; set; }
     
    public string ProductId { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "จำนวนสินค้าต้องอยู่ระหว่าง 1 ถึง 1000")]
    public int Quantity { get; set; } = 1;

    public string? UserId { get; set; }
}

public class UpdateCartItemDto
{
    [Range(0, 1000, ErrorMessage = "จำนวนสินค้าต้องอยู่ระหว่าง 0 ถึง 1000 (หากใส่ 0 จะลบสินค้าออกจากตระกร้า)")]
    public int Quantity { get; set; }

    public string? UserId { get; set; }
}

public class CheckoutResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CheckoutAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Completed";
    public string Message { get; set; } = "ทำการสั่งซื้อและตัดสต็อกสินค้าเรียบร้อยแล้ว";
}
