namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

using ApiDto;
using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.Ship;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly OrderingContext _orderingContext;
    private readonly ISender _mediator;
    
    public OrdersController(IIdentityService identityService, OrderingContext orderingContext, ISender mediator)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _orderingContext = orderingContext;
        _mediator = mediator;
    }

    [Route("new")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> NewOrderAsync([FromBody] NewOrderModel model, CancellationToken token)
    {
        // Get the user info
        var userId = HttpContext.User.FindFirst("sub").Value;
        var userName = HttpContext.User.FindFirst("name").Value;
        
        var request = new Features.Orders.NewOrder.Request
        {
            Buyer = model.Buyer,
            CardExpiration = model.CardExpiration,
            CardHolderName = model.CardHolderName,
            CardNumber = model.CardNumber,
            CardSecurityNumber = model.CardSecurityNumber,
            CardTypeId = model.CardTypeId,
            City = model.City,
            Country = model.Country,
            OrderItems = model.OrderItems.Select(oi => new Features.Orders.NewOrder.Request.OrderItem
            {
                Discount = oi.Discount,
                PictureUrl = oi.PictureUrl,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice,
                Units = oi.Units
            }).ToList(),
            RequestId = model.RequestId,
            State = model.State,
            Street = model.Street,
            UserId = userId,
            UserName = userName,
            ZipCode = model.ZipCode,
        };

        var orderId = _mediator.Send(request, token);

        return CreatedAtAction("GetOrder", new { orderId }, null);
    }

    [Route("cancel")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrderAsync([FromBody] Features.Orders.CancelOrder.Request request, CancellationToken token)
    {
        var result = await _mediator.Send(request, token);

        return result ? Ok() : BadRequest();
    }

    [Route("ship")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ShipOrderAsync([FromBody] ShipOrderModel model, CancellationToken token)
    {
        var result = await _mediator.Send(new Features.Orders.Ship.Request { OrderNumber = model.OrderNumber });

        if (result)
            return Ok();
        
        return BadRequest();
    }

    [Route("{orderId:int}")]
    [HttpGet]
    [ProducesResponseType(typeof(Features.Orders.GetOrder.Model), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> GetOrderAsync(int orderId)
    {
        try
        {
            var order = _mediator.Send(new Features.Orders.GetOrder.Request { OrderId = orderId });

            if (order == null)
            {
                return NotFound();
            }
            
            return Ok(order);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Features.Orders.GetOrders.Model>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<Features.Orders.GetOrders.Model>>> GetOrdersAsync(CancellationToken token)
    {
        var userid = _identityService.GetUserIdentity();

        var request = new Features.Orders.GetOrders.Request
        {
            UserId = userid
        };

        var orderSummary = await _mediator.Send(request, token);

        return Ok(orderSummary);
    }

    [Route("cardtypes")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CardTypeDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CardTypeDto>>> GetCardTypesAsync()
    {
        var cardTypes = await _orderingContext.CardTypes.ToListAsync();

        var result = cardTypes
            .Select(c => new CardTypeDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        return Ok(result);
    }

    [Route("draft")]
    [HttpPost]
    public ActionResult<OrderDraftModel> CreateOrderDraftFromBasketDataAsync([FromBody] CreateOrderDraftModel createOrderDraftModel)
    {
        var order = new Order();
        var orderItems = createOrderDraftModel.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = OrderDraftModel.FromOrder(order);

        return result;
    }

}
