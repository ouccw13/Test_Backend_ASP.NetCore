using backendShoppingcart.DTOs;
using backendShoppingcart.Models;
using backendShoppingcart.Services;
using Microsoft.AspNetCore.Mvc;

namespace backendShoppingcart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("[controller]")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string? ResolveUserId(string? routeUserId, string? dtoUserId = null)
    {
        if (!string.IsNullOrWhiteSpace(routeUserId))
            return routeUserId;

        if (!string.IsNullOrWhiteSpace(dtoUserId))
            return dtoUserId;

        if (HttpContext.Request.Query.TryGetValue("userId", out var queryVal) && !string.IsNullOrWhiteSpace(queryVal))
            return queryVal.ToString();

        return null;
    }

    /// <summary>
    /// ดึงข้อมูลตระกร้าสินค้า (หากยังไม่มีจะสร้างตระกร้าเปล่าให้โดยอัตโนมัติ ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpGet]
    [HttpGet("{userId?}")]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart(string? userId = null)
    {
        var resolvedUserId = ResolveUserId(userId);
        var cart = await _cartService.GetCartAsync(resolvedUserId);
        return Ok(ApiResponse<Cart>.Ok(cart, "ดึงข้อมูลตระกร้าสินค้าสำเร็จ"));
    }

    /// <summary>
    /// เพิ่มสินค้าลงในตระกร้า (หากมีอยู่แล้วจะบวกจำนวนเพิ่ม และตรวจสอบสต็อก ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpPost("items")]
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto, string? userId = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<Cart>.Fail("ข้อมูลไม่ถูกต้อง", errors));
        }

        try
        {
            var targetUserId = ResolveUserId(userId, dto.UserId);
            var cart = await _cartService.AddItemToCartAsync(dto, targetUserId);
            return Ok(ApiResponse<CartDto>.Ok(cart, "เพิ่มสินค้าลงในตระกร้าสำเร็จ"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<Cart>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<Cart>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Cart>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
        }
    }

    /// <summary>
    /// ปรับปรุงจำนวนสินค้าในตระกร้า (หากระบุจำนวนเป็น 0 จะลบสินค้าออกจากตระกร้า ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpPut("items/{productId}")]
    [HttpPatch("items/{productId}")]
    [HttpPut("{userId}/items/{productId}")]
    [HttpPatch("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity(string productId, [FromBody] UpdateCartItemDto dto, string? userId = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<Cart>.Fail("ข้อมูลไม่ถูกต้อง", errors));
        }

        try
        {
            var targetUserId = ResolveUserId(userId, dto.UserId);
            var cart = await _cartService.UpdateItemQuantityAsync(productId, dto.Quantity, targetUserId);
            return Ok(ApiResponse<Cart>.Ok(cart, "อัปเดตจำนวนสินค้าในตระกร้าสำเร็จ"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<Cart>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<Cart>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Cart>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
        }
    }

    /// <summary>
    /// ลบสินค้าออกจากตระกร้า (ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpDelete("items/{productId}")]
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem(string productId, string? userId = null)
    {
        try
        {
            var targetUserId = ResolveUserId(userId);
            var cart = await _cartService.RemoveItemFromCartAsync(productId, targetUserId);
            return Ok(ApiResponse<Cart>.Ok(cart, "ลบสินค้าออกจากตระกร้าสำเร็จ"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Cart>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
        }
    }

    /// <summary>
    /// ล้างสินค้าทั้งหมดออกจากตระกร้า (ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpDelete]
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(ApiResponse<Cart>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart(string? userId = null)
    {
        try
        {
            var targetUserId = ResolveUserId(userId);
            var cart = await _cartService.ClearCartAsync(targetUserId);
            return Ok(ApiResponse<Cart>.Ok(cart, "ล้างตระกร้าสินค้าเรียบร้อยแล้ว"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Cart>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
        }
    }

    /// <summary>
    /// เช็คเอาท์และสั่งซื้อสินค้า (ตรวจสอบสต็อก, ตัดสต็อกสินค้าจริง และล้างตระกร้า ไม่จำเป็นต้องส่ง userId)
    /// </summary>
    [HttpPost("checkout")]
    [HttpPost("{userId}/checkout")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Checkout(string? userId = null)
    {
        try
        {
            var targetUserId = ResolveUserId(userId);
            var result = await _cartService.CheckoutAsync(targetUserId);
            return Ok(ApiResponse<CheckoutResultDto>.Ok(result, "การเช็คเอาท์สำเร็จ"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CheckoutResultDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CheckoutResultDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CheckoutResultDto>.Fail($"เกิดข้อผิดพลาดในการเช็คเอาท์: {ex.Message}"));
        }
    }
}
