using backendShoppingcart.DTOs;
using backendShoppingcart.Models;

namespace backendShoppingcart.Services;

public interface ICartService
{
    Task<Cart> GetCartAsync(string? userId = null);
    Task<Cart> GetCartByUserIdAsync(string? userId = null);
    Task<CartDto> AddItemToCartAsync(AddToCartDto dto, string userId);
    Task<Cart> UpdateItemQuantityAsync(string productId, int quantity, string userId);
    Task<Cart> RemoveItemFromCartAsync(string productId, string userId);
    Task<Cart> ClearCartAsync(string userId);
    Task<CheckoutResultDto> CheckoutAsync(string? userId = null);
}
