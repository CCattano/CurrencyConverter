using Microsoft.Extensions.DependencyInjection;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Caches;

public static class CountryDetailsCacheExtensions
{
    public static void AddCountryDetailsCache(this IServiceCollection services)
    {
        services.AddSingleton<ICountryDetailsCache, CountryDetailsCache>();
    }
}