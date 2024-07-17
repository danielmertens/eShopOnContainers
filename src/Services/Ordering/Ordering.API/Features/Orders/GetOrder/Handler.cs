using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder
{
    public class Handler(OrderingContext _orderingContext) : IRequestHandler<Request, Model>
    {
        public async Task<Model> Handle(Request request, CancellationToken cancellationToken)
        {
            var order = await _orderingContext.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .Where(o => o.Id == request.OrderId)
                .SingleOrDefaultAsync();

            return new Model
            {
                OrderNumber = order.Id,
                Description = order.Description,
                Street = order.Address.Street,
                City = order.Address.City,
                ZipCode = order.Address.ZipCode,
                Country = order.Address.Country,
                Date = order.OrderDate,
                Status = order.OrderStatus.ToString(),
                Total = OrderManager.GetTotal(order),
                OrderItems = order.OrderItems.Select(orderItem => new OrderItemDto
                {
                    ProductName = orderItem.ProductName,
                    PictureUrl = orderItem.PictureUrl,
                    UnitPrice = orderItem.UnitPrice,
                    Units = orderItem.Units
                }).ToList()
            };
        }
    }
}
