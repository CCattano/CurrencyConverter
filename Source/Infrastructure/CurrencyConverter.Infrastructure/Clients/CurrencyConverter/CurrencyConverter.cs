using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter.Types;
using SC = Torty.Web.Apps.CurrencyConverter.Infrastructure.SystemConstants;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

public interface ICurrencyConverter
{
    /// <summary>
    /// Given to currency codes returns the ratio to convert once currency to the other
    /// </summary>
    /// <param name="fromCurrencyCode"></param>
    /// <param name="toCurrencyCode"></param>
    /// <returns></returns>
    Task<decimal?> GetConversionRatio(string fromCurrencyCode, string toCurrencyCode);

    /// <summary>
    /// Returns all country data managed by the Currency Converter API
    /// </summary>
    /// <returns></returns>
    Task<List<CountryDetails>> GetAllCountryData();
}

public class CurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;

    private readonly string _apiKey =
        Environment.GetEnvironmentVariable(SystemConstants.EnvVars.CurrencyConverterApiKey);

    public CurrencyConverter(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<decimal?> GetConversionRatio(string fromCurrencyCode, string toCurrencyCode)
    {
        Dictionary<string, string> queryParams = new()
        {
            { "q", $"{fromCurrencyCode}_{toCurrencyCode}".ToUpper() },
            { "compact", "ultra" },
            { "apiKey", _apiKey }
        };
        string queryStr = string.Join('&', queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        string request = $"{EndpointPaths.GetConversionRatio}?{queryStr}";

        HttpResponseMessage response = await _httpClient.GetAsync(request);
        
        if (!response.IsSuccessStatusCode) return null;
        
        // Example value: "{"USD_EUR":0.944598}"
        string responseStr = await response.Content.ReadAsStringAsync();

        if (!responseStr.Contains(':')) return null;
        
        string ratioStr = responseStr.Split(':')[1][..^1];
        
        decimal? result = decimal.TryParse(ratioStr, out decimal ratio) ? ratio : null;
        return result;
    }

    public async Task<List<CountryDetails>> GetAllCountryData()
    {
        string request = $"{EndpointPaths.GetCountryData}?apiKey={_apiKey}";

        HttpResponseMessage response = await _httpClient.GetAsync(request);

        if (!response.IsSuccessStatusCode)
            return new List<CountryDetails>();

        Stream responseStream = await response.Content.ReadAsStreamAsync();
        JsonNode responseData = JsonNode.Parse(responseStream);
        string jsonToParse =
            "[" +
            string.Join(',', responseData!["results"]!.AsObject().Select(objProp => objProp.Value!.ToString())) +
            "]";

        List<CountryDetails> countryDetails = JsonNode.Parse(jsonToParse).Deserialize<List<CountryDetails>>();

        return countryDetails;
    }
    
    private struct EndpointPaths
    {
        public const string GetConversionRatio = "api/v7/convert";
        public const string GetCountryData = "api/v7/countries";
    }
}