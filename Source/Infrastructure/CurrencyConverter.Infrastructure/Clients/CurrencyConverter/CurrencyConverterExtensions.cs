using System;
using Microsoft.Extensions.DependencyInjection;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

public static class CurrencyConverterExtensions
{
    /// <summary>
    /// Register an instance of the CurrencyConverter client with the DI Bag
    /// </summary>
    /// <param name="services"></param>
    public static void AddCurrencyConverter(this IServiceCollection services) =>
        services.AddHttpClient<ICurrencyConverter, CurrencyConverter>(httpClient =>
            httpClient.BaseAddress = new Uri("https://free.currconv.com"));
}