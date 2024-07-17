namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public record OrderItemDto
{
    public string ProductName { get; set; }
    public int Units { get; set; }
    public decimal UnitPrice { get; set; }
    public string PictureUrl { get; set; }
}

public record CardTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
