
using Application.Balances.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace View.Api.V1;

[ApiController]
[Route("v1/balance")]
public class BalanceController : Controller
{
    private readonly IMediator _mediator;

    public BalanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("{UserId}")]
    public async Task<IActionResult> GetUserBalace(Guid UserId)
    {
        return Ok(await _mediator.Send(new GetUserBalance.Query(UserId)));
    }
}
