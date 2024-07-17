using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

public class Handler : IRequestHandler<Request, IEnumerable<Model>>
{
    private readonly OrderingContext _orderingContext;

    public Handler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }

    public async Task<IEnumerable<Model>> Handle(Request request, CancellationToken cancellationToken)
    {
        var orders = await _orderingContext.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .Where(o => o.Buyer.IdentityGuid == request.UserId)
                .ToListAsync();

        var orderSummary = orders
            .Select(o => new Features.Orders.GetOrders.Model
            {
                OrderNumber = o.Id,
                Date = o.OrderDate,
                Status = o.OrderStatus.ToString(),
                Total = OrderManager.GetTotal(o)
            });

        return orderSummary;
    }

}
