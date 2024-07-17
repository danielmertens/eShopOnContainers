using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.Ship;

public class Request : IRequest<bool>
{
    public int OrderNumber { get; set; }
}
