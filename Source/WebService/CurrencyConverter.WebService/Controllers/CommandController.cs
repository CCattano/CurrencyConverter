using Microsoft.AspNetCore.Mvc;
using Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;

namespace Torty.Web.Apps.CurrencyConverter.WebService.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CommandController : ControllerBase
{
    private readonly ICommandsAdapter _adapter;

    public CommandController(ICommandsAdapter adapter) => _adapter = adapter;

    [HttpGet]
    public async Task<ActionResult> Parse()
    {
        await _adapter.ParseCommand();
        return Ok();
    }
}