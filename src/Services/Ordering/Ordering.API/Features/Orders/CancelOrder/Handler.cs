using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;

public class Handler(OrderingContext _orderingContext) : IRequestHandler<Request, bool>
{
    public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
    {
        var orderToUpdate = await _orderingContext.Orders.FindAsync(request.OrderNumber);
        if (orderToUpdate == null)
        {
            return false;
        }

        orderToUpdate.OrderStatusId = OrderStatus.Cancelled.Id;
        orderToUpdate.Description = $"The order was cancelled.";

        await _orderingContext.SaveChangesAsync();

        return true;
    }
}
