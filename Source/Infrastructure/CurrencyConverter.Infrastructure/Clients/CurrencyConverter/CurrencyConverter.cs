using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

public interface ICurrencyConverter
{
    Task Temp();
}

public class CurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;

    public CurrencyConverter(HttpClient httpClient) => _httpClient = httpClient;

    public Task Temp()
    {
        Console.WriteLine("e2e works");
        return Task.CompletedTask;
    }
}