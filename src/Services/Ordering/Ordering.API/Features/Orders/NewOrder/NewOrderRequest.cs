using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;

public record NewOrderRequest : IRequest<int>
{        
    public string City { get; set; }
        
    public string Street { get; set; }
        
    public string State { get; set; }

    public string Country { get; set; }

    public string ZipCode { get; set; }

    public string CardNumber { get; set; }

    public string CardHolderName { get; set; }

    public DateTime CardExpiration { get; set; }

    public string CardSecurityNumber { get; set; }

    public int CardTypeId { get; set; }

    public string Buyer { get; set; }

    public Guid RequestId { get; set; }
    
    public List<OrderItem> OrderItems { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public record OrderItem
    {
        public int ProductId { get; init; }

        public string ProductName { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal Discount { get; init; }

        public int Units { get; init; }

        public string PictureUrl { get; init; }
    }
}

public class NewOrderRequestValidator : AbstractValidator<NewOrderRequest>
{
    public NewOrderRequestValidator()
    {
        // Validation for address
        RuleFor(req => req.Street).NotEmpty();
        RuleFor(req => req.City).NotEmpty();
        RuleFor(req => req.State).NotEmpty();
        RuleFor(req => req.Country).NotEmpty();
        RuleFor(req => req.ZipCode).NotEmpty();

        RuleFor(req => req.OrderItems).NotEmpty();

        // Validation for Buyer
        RuleFor(req => req.UserId).NotEmpty();
        RuleFor(req => req.UserName).NotEmpty();

        // Validation Payment info
        RuleFor(req => req.CardExpiration).GreaterThanOrEqualTo(DateTime.UtcNow);
        RuleFor(req => req.CardHolderName).NotEmpty();
        RuleFor(req => req.CardNumber).NotEmpty().CreditCard();
        RuleFor(req => req.CardSecurityNumber).NotEmpty();
    }
}