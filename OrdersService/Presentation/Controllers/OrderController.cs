using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Interfaces;
using OrdersService.Domain.Entities;
using OrdersService.Presentation.DTO;

namespace OrdersService.Presentation.Controllers;


[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;


    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderCreationResponce), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateOrder(
        [FromQuery] Guid userId,
        [FromQuery] decimal amount,
        [FromQuery] string description = "")
    {
        if (amount < 0)
        {
            return BadRequest("Amount should not be negative");
        }
        try
        {
            Order newReq = new Order(
                userId, amount, description
            );
            var t = await _orderService.AddOrderAsync(userId, amount, description);
            return Ok(new OrderCreationResponce(
                Id: t,
                UserId: newReq.UserId,
                Amount: newReq.Amount,
                Description: newReq.Description));
        }
        catch
        {
            return BadRequest("LOLLLL");
        }
    }

    [HttpGet]
    [Route("/GetAllOrders")]
    [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAllOrders()
    {
        try
        {
            var resp = await _orderService.GetAllOrdersAsync();
            if (resp == null)
            {
                return BadRequest("No orders");
            }

            return Ok(resp);
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("/GetOrderStatus")]
    [ProducesResponseType(typeof(StatusType), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetOrderStatus(Guid id)
    {
        try
        {
            var resp = await _orderService.GetOrderAsync(id);
            return Ok(resp.Status.ToString());
        }
        catch
        {
            return BadRequest();
        }
    }
    
}