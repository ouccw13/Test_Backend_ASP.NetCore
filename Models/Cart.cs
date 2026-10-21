using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backendShoppingcart.Models;

public class Cart
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = "default-user";

    [BsonElement("items")]
    public List<CartItem> Items { get; set; } = new();

    [BsonElement("totalItems")]
    public int TotalItems { get; set; }

    [BsonElement("totalPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalPrice { get; set; }

    [BsonElement("total")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Total { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void RecalculateTotals()
    {
        TotalItems = Items.Sum(i => i.Quantity);
        TotalPrice = Items.Sum(i => i.SubTotal);
        UpdatedAt = DateTime.UtcNow;
    }
}
