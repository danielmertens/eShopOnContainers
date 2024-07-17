using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.Ship
{
    public class Handler : IRequestHandler<ShipModel, bool>
    {
        private readonly OrderingContext _orderingContext;

        public Handler(OrderingContext orderingContext)
        {
            _orderingContext = orderingContext;
        }

        public async Task<bool> Handle(ShipModel model, CancellationToken token)
        {
            var orderToUpdate = await _orderingContext.Orders.FindAsync(model.OrderNumber);
            if (orderToUpdate == null)
            {
                return false;
            }

            orderToUpdate.OrderStatusId = OrderStatus.Shipped.Id;
            orderToUpdate.Description = "The order was shipped.";

            await _orderingContext.SaveChangesAsync();

            return true;
        }

    }
}
