using Application.Customers.Commands;
using Application.Customers.Queries;

using Domain.SharedKernel;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace View.Api.V1;


[Authorize]
[ApiController]
[Route("v1/customer")]
public class CustomerController : BaseController
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ListAll()
        => Ok(await _mediator.Send(new ListAllCustomers.Query()));

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(Guid userId)
        => Ok(await _mediator.Send(new GetCustomer.Query(userId)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomer.Command command)
        => Ok(await _mediator.Send(command));

    [HttpGet("select/payment-providers")]
    public IActionResult SelectPaymentProviders() =>
        ResponseWithEnum(PaymentProvider.QvaPay);

    [HttpGet("select/payment-status")]
    public IActionResult SelectPaymentStatuses() =>
    ResponseWithEnum(PaymentStatus.Completed);
}
