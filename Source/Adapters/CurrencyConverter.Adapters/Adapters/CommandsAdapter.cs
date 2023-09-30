using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

namespace Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;

public interface ICommandsAdapter
{
    Task ParseCommand();
}

public class CommandsAdapter : ICommandsAdapter
{
    private readonly ICurrencyConverter _client;

    public CommandsAdapter(ICurrencyConverter client) => _client = client;

    public async Task ParseCommand()
    {
        await _client.Temp();
    }
}