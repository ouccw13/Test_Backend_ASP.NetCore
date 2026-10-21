using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backendShoppingcart.Models;

public class CartItem
{
     [BsonElement("id")]
     public string Id { get; set; } = string.Empty;

      [BsonElement("name")]
     public string Name { get; set; } = string.Empty;

     [BsonElement("stock")]
     public int Stock { get; set; }

    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("unitPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }

     

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("subTotal")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SubTotal { get; set; }

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
}
