using System.Text.Json.Serialization;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter.Types;

public class CountryDetails
{
    /// <summary>
    /// The 3-character code for the country
    /// </summary>
    [JsonPropertyName("alpha3")]
    public string Alpha3 {get; init;}
    
    /// <summary>
    /// The 3-character code for the country's currency
    /// </summary>
    [JsonPropertyName("currencyId")]
    public string CurrencyId {get; init;}
    
    /// <summary>
    /// The formal name for the country's currency
    /// </summary>
    [JsonPropertyName("currencyName")]
    public string CurrencyName {get; init;}
    
    /// <summary>
    /// The ascii-based symbol for the country's currency
    /// </summary>
    [JsonPropertyName("currencySymbol")]
    public string CurrencySymbol {get; init;}
    
    /// <summary>
    /// The API-defined ID for the country
    /// </summary>
    [JsonPropertyName("id")]
    public string Id {get; init;}
    
    /// <summary>
    /// The formal name of the country
    /// </summary>
    [JsonPropertyName("name")]
    public string Name {get; init;}
}