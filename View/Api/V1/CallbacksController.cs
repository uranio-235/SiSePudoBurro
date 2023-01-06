using Application.Balances.Jobs;
using Application.Payments.Command;

using Domain.Abstract.Services;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace View.Api.V1;

[ApiController]
[Route("v1/callback")]
public class CallbacksController : Controller
{
    private readonly IMediator _mediator;

    public CallbacksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("qvapay")]
    [AllowAnonymous]
    public async Task<ActionResult> QvaPay(string id, string remote_id)
    {
        var result = await _mediator.Send(new ConfirmQvapay.Command(id, remote_id));

        if (result.IsSuccess)
            return Ok();
        else 
            return BadRequest();

    } // QvaPay

}
