using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.Interfaces;
using PaymentsService.Controllers.DTO;

namespace PaymentsService.Controllers;


[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }


    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponce), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateUserAccount([FromQuery] decimal amount = 0)
    {
        if (amount < 0)
        {
            return BadRequest();
        }

        var id = Guid.NewGuid();
        await _paymentService.CreateUserBalance(id, amount);
        return Ok(new PaymentResponce(
            UserId: id,
            Amount: amount));
    }


    [HttpGet]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetUserBalance([FromQuery] Guid userId)
    {
        try
        {
            var balance = await _paymentService.GetUserBalance(userId);
            return Ok(balance);
        }
        catch
        {
            return BadRequest();
        }

    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> TopUpUserBalance([FromQuery] Guid userId, [FromQuery] decimal amount)
    {
        try
        {
            var newBalance = await _paymentService.TopUpUserBalance(userId, amount);
            return Ok(newBalance);
        }
        catch
        {
            return BadRequest();
        }
    }
}