using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder;

public class GetOrderRequest : IRequest<OrderDto>
{
    public int OrderId { get; set; }
}

public class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(req => req.OrderId).GreaterThan(0);
    }
}