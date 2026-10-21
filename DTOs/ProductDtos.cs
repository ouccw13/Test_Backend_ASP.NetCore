using System.ComponentModel.DataAnnotations;

namespace backendShoppingcart.DTOs;

public class CreateProductDto
{
    public string? ProductId { get; set; }

    [Required(ErrorMessage = "กรุณาระบุชื่อสินค้า")]
    [StringLength(100, ErrorMessage = "ชื่อสินค้าต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000000, ErrorMessage = "ราคาสินค้าต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "จำนวนสต็อกต้องไม่ติดลบ")]
    public int StockQuantity { get; set; }

    public string Category { get; set; } = "ทั่วไป";

    public string ImageUrl { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    public string? ProductId { get; set; }

    [Required(ErrorMessage = "กรุณาระบุชื่อสินค้า")]
    [StringLength(100, ErrorMessage = "ชื่อสินค้าต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000000, ErrorMessage = "ราคาสินค้าต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "จำนวนสต็อกต้องไม่ติดลบ")]
    public int StockQuantity { get; set; }

    public string Category { get; set; } = "ทั่วไป";

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
