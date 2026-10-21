using backendShoppingcart.DTOs;
using backendShoppingcart.Models;
using backendShoppingcart.Services;
using Microsoft.AspNetCore.Mvc;

namespace backendShoppingcart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// ดึงรายการสินค้าทั้งหมด (สามารถกรองตามหมวดหมู่ได้)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Product>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var products = await _productService.GetAllAsync(category);
        return Ok(ApiResponse<List<Product>>.Ok(products, "ดึงรายการสินค้าสำเร็จ"));
    }

    /// <summary>
    /// ดึงข้อมูลสินค้าตาม ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse<Product>.Fail($"ไม่พบสินค้า ID: {id}"));
        }
        return Ok(ApiResponse<Product>.Ok(product, "ดึงข้อมูลสินค้าสำเร็จ"));
    }

    /// <summary>
    /// เพิ่มสินค้าใหม่
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<Product>.Fail("ข้อมูลไม่ถูกต้อง", errors));
        }

        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Product>.Ok(created, "เพิ่มสินค้าใหม่สำเร็จ"));
    }

    /// <summary>
    /// แก้ไขข้อมูลสินค้า
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<Product>.Fail("ข้อมูลไม่ถูกต้อง", errors));
        }

        var updated = await _productService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFound(ApiResponse<Product>.Fail($"ไม่พบสินค้า ID: {id}"));
        }
        return Ok(ApiResponse<Product>.Ok(updated, "อัปเดตข้อมูลสินค้าสำเร็จ"));
    }

    /// <summary>
    /// ลบสินค้า
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<bool>.Fail($"ไม่พบสินค้า ID: {id}"));
        }
        return Ok(ApiResponse<bool>.Ok(true, "ลบสินค้าสำเร็จ"));
    }

    /// <summary>
    /// สุ่ม/เพิ่มข้อมูลสินค้าตัวอย่าง (Sample Products) สำหรับทดสอบระบบ
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(ApiResponse<List<Product>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed()
    {
        var products = await _productService.SeedSampleProductsAsync();
        return Ok(ApiResponse<List<Product>>.Ok(products, "เตรียมข้อมูลสินค้าตัวอย่างเรียบร้อยแล้ว"));
    }
}
