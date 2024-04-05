using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

public class CreateOrderDraftHandler : IRequestHandler<CreateOrderDraftRequest, OrderDraftModel>
{
    public Task<OrderDraftModel> Handle(CreateOrderDraftRequest request, CancellationToken cancellationToken)
    {
        var order = Order.NewDraft();
        var orderItems = request.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = CreateOrderDraftRequest.FromOrder(order);

        return Task.FromResult(result);
    }
}
