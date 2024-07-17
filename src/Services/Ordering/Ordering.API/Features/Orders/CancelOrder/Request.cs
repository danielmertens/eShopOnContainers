using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;

public record Request : IRequest<bool>
{
    public int OrderNumber { get; set; }
}
