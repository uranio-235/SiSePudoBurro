
using Application.Balances.Queries;
using Application.Payments.Commands;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace View.Api.V1;

[Authorize]
[ApiController]
[Route("v1/balance")]
public class BalanceController : BaseController
{
    private readonly IMediator _mediator;

    public BalanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{UserId}")]
    public async Task<IActionResult> GetByUserId(Guid UserId)
        => Ok(await _mediator.Send(new GetUserBalance.Query(UserId)));

    [HttpPost("{UserId}")]
    public async Task<IActionResult> RequestPaymentByUserId(
        [FromBody] RequestPayment.Command command,
        Guid UserId)
        => Ok(await _mediator.Send(command with { UserId = UserId }));

}
