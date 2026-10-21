using System.Collections.Generic;

namespace backendShoppingcart.Models // เปลี่ยน Namespace ให้ตรงกับโปรเจกต์ของคุณ
{
    public class CartDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }

    public class CartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // สำหรับรองรับข้อมูลตอนยิง POST เพิ่มสินค้าเข้ามา
    // public class AddToCartDto
    // {
    //     public string? CartId { get; set; }
    //     public string ProductId { get; set; } = string.Empty;
    //     public int Quantity { get; set; }
    // }
}