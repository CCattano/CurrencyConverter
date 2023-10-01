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
    public async Task<ActionResult> Parse([FromQuery] string command)
    {
        try
        {
            string response = await _adapter.ParseCommand(command);
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine("Caught an unexpected uncaught exception.");
            Console.WriteLine($"The command passed in was: {command}");
            Console.WriteLine("The exception captured was:");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return Ok("Could not process your command as written. Use the \"help\"" +
                      " (!converter help) action to learn how !converter works.");
        }
    }
}