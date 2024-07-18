using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;

public class CancelOrderRequest : IRequest<bool>
{
    public int OrderNumber { get; set; }
}

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(req => req.OrderNumber).GreaterThan(0);
    }
}
