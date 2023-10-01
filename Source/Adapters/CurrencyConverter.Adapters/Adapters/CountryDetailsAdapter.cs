using Torty.Web.Apps.CurrencyConverter.BusinessEntities.Countries;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Caches;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter.Types;

namespace Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;

public interface ICountryDetailsAdapter
{
    /// <summary>
    /// Returns a CountryDetailsBE for a given country name if found, otherwise returns null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<CountryDetailsBE> GetCountryByNameOrDefault(string name);

    /// <summary>
    /// Returns a CountryDetailsBE for a given 3-character country code if found, otherwise returns null
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<CountryDetailsBE> GetCountryByCountryCodeOrDefault(string code);
    
    /// <summary>
    /// For a given currency's 3-character code, returns that currency's ascii symbol if found otherwise returns null.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<string> GetCurrencySymbolByCurrencyCodeOrDefault(string code);
}

/// <inheritdoc cref="ICountryDetailsAdapter"/>
public class CountryDetailsAdapter : ICountryDetailsAdapter
{
    private readonly ICountryDetailsCache _cache;
    private readonly ICurrencyConverter _client;

    public CountryDetailsAdapter(ICountryDetailsCache cache, ICurrencyConverter client)
    {
        _cache = cache;
        _client = client;
    }

    public async Task<CountryDetailsBE> GetCountryByNameOrDefault(string name)
    {
        CountryDetailsBE countryDetails = _cache.GetValueByNameOrDefault(name);

        if (countryDetails == null && _cache.IsCacheExpired())
        {
            await _TryRefreshExpiredCache();
            countryDetails = _cache.GetValueByNameOrDefault(name);
        }

        return countryDetails;
    }
    
    public async Task<CountryDetailsBE> GetCountryByCountryCodeOrDefault(string code)
    {
        CountryDetailsBE countryDetails = _cache.GetValueByCountryCodeOrDefault(code);

        if (countryDetails == null && _cache.IsCacheExpired())
        {
            await _TryRefreshExpiredCache();
            countryDetails = _cache.GetValueByCountryCodeOrDefault(code);
        }

        return countryDetails;
    }

    public async Task<string> GetCurrencySymbolByCurrencyCodeOrDefault(string code)
    {
        string currencySymbol = _cache.GetCurrencySymbolByCurrencyCodeOrDefault(code);

        if (currencySymbol == null && _cache.IsCacheExpired())
        {
            await _TryRefreshExpiredCache();
            currencySymbol = _cache.GetCurrencySymbolByCurrencyCodeOrDefault(code);
        }
        
        return currencySymbol;
    }

    private async Task _TryRefreshExpiredCache()
    {
        // The CountryDetailsCache uses a series of locks to read/write data
        // Before we make an API request let's check one more time that the cache is expired
        // We may have been the last person to hold the cache lock right before a different
        // thread went and updated the cache, so we may not need to make this web request
        if (_cache.IsCacheExpired())
        {
            List<CountryDetails> freshCountryDetailData = await _client.GetAllCountryData();

            if (freshCountryDetailData.Count == 0)
            {
                // We could not fetch new country data from the API
                // The free-tier of the API is not guaranteed to always be up and available
                // We'll extend the validity of our cache by another 30 minutes
                // This will cause us to come back here and try to update it in 30 minute's time
                _cache.ExtendCacheTimeToLive();
            }
            else
            {
                // We have latest country data from the API, we'll cache it for 24 hours
                List<CountryDetailsBE> countryDetailsForCache = freshCountryDetailData
                    .Select(country => new CountryDetailsBE
                    {
                        Id = country.Id,
                        Name = country.Name,
                        CountryCode = country.Alpha3,
                        CurrencyName = country.CurrencyName,
                        CurrencyCode = country.CurrencyId,
                        CurrencySymbol = country.CurrencySymbol
                    })
                    .ToList();

                _cache.SetValues(countryDetailsForCache);
            }
        }
    }
}