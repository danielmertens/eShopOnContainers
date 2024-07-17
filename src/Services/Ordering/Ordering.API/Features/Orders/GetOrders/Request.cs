using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

public record Request: IRequest<IEnumerable<Model>>
{
    public string UserId { get; set; }
}
