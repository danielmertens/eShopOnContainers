using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersRequest, IEnumerable<OrderSummaryDto>>
{
    private readonly IIdentityService _identityService;
    private readonly OrderingContext _orderingContext;

    public GetOrdersHandler(IIdentityService identityService, OrderingContext orderingContext)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _orderingContext = orderingContext;
    }
    
    public async Task<IEnumerable<OrderSummaryDto>> Handle(GetOrdersRequest request, CancellationToken cancellationToken)
    {
        var userid = _identityService.GetUserIdentity();
        var orders = await _orderingContext.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => o.Buyer.IdentityGuid == userid)
            .ToListAsync();

        var orderSummary = orders
            .Select(o => new OrderSummaryDto
            {
                OrderNumber = o.Id,
                Date = o.OrderDate,
                Status = o.OrderStatus.ToString(),
                Total = o.GetTotal()
            })
            .ToList();
        
        return orderSummary;
    }
}