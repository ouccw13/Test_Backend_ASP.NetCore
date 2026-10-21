namespace backendShoppingcart.Models;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ProductsCollectionName { get; set; } = "Products";
    public string CartsCollectionName { get; set; } = "Carts";
}