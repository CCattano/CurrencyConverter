using Microsoft.Extensions.DependencyInjection;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

public static class CurrencyConverterExtensions
{
    public static void AddCurrencyConverter(this IServiceCollection services)
    {
        services.AddHttpClient<ICurrencyConverter, CurrencyConverter>();
    }
}