using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder;

public class Request : IRequest<Model>
{
    public int OrderId { get; set; }
}
