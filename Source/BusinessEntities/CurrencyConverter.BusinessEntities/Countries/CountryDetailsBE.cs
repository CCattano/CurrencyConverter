namespace Torty.Web.Apps.CurrencyConverter.BusinessEntities.Countries;

public class CountryDetailsBE
{
    /// <summary>
    /// The API-defined ID for the country
    /// </summary>
    public string Id {get; init;}
    
    /// <summary>
    /// The formal name of the country
    /// </summary>
    public string Name {get; init;}

    /// <summary>
    /// The 3-character code for the country
    /// </summary>
    public string CountryCode {get; init;}
    
    /// <summary>
    /// The formal name for the country's currency
    /// </summary>
    public string CurrencyName {get; init;}

    /// <summary>
    /// The 3-character code for the country's currency
    /// </summary>
    public string CurrencyCode {get; init;}

    /// <summary>
    /// The ascii-based symbol for the country's currency
    /// </summary>
    public string CurrencySymbol {get; init;}
}