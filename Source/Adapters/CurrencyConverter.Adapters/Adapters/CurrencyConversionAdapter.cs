using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

namespace Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;

public interface ICurrencyConversionAdapter
{
    Task<string> ConvertCurrency(string amount, string from, string to);
}

public class CurrencyConversionAdapter : ICurrencyConversionAdapter
{
    private readonly ICurrencyConverter _client;
    private readonly ICountryDetailsAdapter _countryDetailsAdapter;

    public CurrencyConversionAdapter(ICurrencyConverter client, ICountryDetailsAdapter countryDetailsAdapter)
    {
        _client = client;
        _countryDetailsAdapter = countryDetailsAdapter;
    }

    public async Task<string> ConvertCurrency(string amount, string from, string to)
    {
        if (!decimal.TryParse(amount, out decimal amountNum))
            return $"{amount} doesn't seem to be a real number. If you feel you provided" +
                   " a real number, send the command text you used to Torty to look into.";
        
        decimal? conversionRatio = await _client.GetConversionRatio(from, to);
            
        if (conversionRatio == null)
            return "Could not determine a conversion ratio. You may have provided an incorrect 3-character" +
                   " currency code or the online conversion service may be down temporarily. Try again in" +
                   " a few minutes. If the issue persists and you feel you used the !converter" +
                   " command correctly, send the command text you used to Torty to look into.";

        decimal conversionResult =
            decimal.Round(amountNum * conversionRatio.Value, 2, MidpointRounding.AwayFromZero);

        string fromSymbol = await _countryDetailsAdapter.GetCurrencySymbolByCurrencyCodeOrDefault(from) ?? string.Empty;
        string toSymbol = await _countryDetailsAdapter.GetCurrencySymbolByCurrencyCodeOrDefault(to) ?? string.Empty;

        string response = $"{fromSymbol}{amount} {from} is {toSymbol}{conversionResult} {to}";

        return response;
    }
}