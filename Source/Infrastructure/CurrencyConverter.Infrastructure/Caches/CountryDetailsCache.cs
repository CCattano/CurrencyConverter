using System;
using System.Collections.Generic;
using System.Linq;
using Torty.Web.Apps.CurrencyConverter.BusinessEntities.Countries;

namespace Torty.Web.Apps.CurrencyConverter.Infrastructure.Caches;

public interface ICountryDetailsCache
{
    /// <summary>
    /// Reset the content of the cache with a new data state
    ///
    /// The cache state will be persisted for 1 day
    /// </summary>
    /// <param name="countryDetails"></param>
    void SetValues(List<CountryDetailsBE> countryDetails);
    
    /// <summary>
    /// Fetch a value from the cache via country name.
    /// Returns the value if found otherwise null.
    /// </summary>
    /// <param name="countryName"></param>
    /// <returns></returns>
    CountryDetailsBE GetValueByNameOrDefault(string countryName);

    /// <summary>
    /// Fetch a value from the cache via a country's 3-character country code.
    /// Returns the value if found otherwise null.
    /// </summary>
    /// <param name="countryCode"></param>
    /// <returns></returns>
    CountryDetailsBE GetValueByCountryCodeOrDefault(string countryCode);

    /// <summary>
    /// Get a currency's symbol from the cache via the currency's 3-character code.
    /// Returns the value if found otherwise null.
    /// </summary>
    /// <param name="currencyCode"></param>
    /// <returns></returns>
    string GetCurrencySymbolByCurrencyCodeOrDefault(string currencyCode);
    
    /// <summary>
    /// Returns a bool indicating if the cache has expired and needs rehydrated
    /// </summary>
    /// <returns></returns>
    bool IsCacheExpired();

    /// <summary>
    /// Refreshes the cache's Time To Live so the cache is considered valid.
    ///
    /// This should only be used if you are unable to fetch a fresh data
    /// source for the cache and need to use the current cache data.
    ///
    /// This will allow the cache to be considered valid for an extra 30 minutes
    /// After which an attempt to update the cache should be done by the upstream cache user
    /// </summary>
    void ExtendCacheTimeToLive();
}

/// <inheritdoc cref="ICountryDetailsCache"/>
public class CountryDetailsCache : ICountryDetailsCache
{
    // Stateful data
    private List<CountryDetailsBE> _cache;
    private DateTime _lastCacheSetDateTime = DateTime.Now;
    // Default to 0 so on init cache is immediately "stale" and
    // require an upstream user to hydrate the cache w/ content
    private TimeSpan _cacheEntryTTL = TimeSpan.Zero;
    
    // Locks
    private readonly object _cacheContentLock = new();
    private readonly object _cacheTTLLock = new();

    public void SetValues(List<CountryDetailsBE> countryDetails)
    {
        lock (_cacheContentLock)
        lock (_cacheTTLLock)
        {
            _cache = countryDetails;
            _lastCacheSetDateTime = DateTime.Now;
            _cacheEntryTTL = TimeSpan.FromDays(1);
        }
    }
    
    public CountryDetailsBE GetValueByNameOrDefault(string countryName)
    {
        if (_IsExpiredFetch()) return null;

        string searchVal = countryName.ToLower();
        CountryDetailsBE countryDetails;
        lock (_cacheContentLock)
        {
            countryDetails =
                _cache?.FirstOrDefault(country => country.Name.ToLower().Contains(searchVal));
        }
        return countryDetails;
    }
    
    public CountryDetailsBE GetValueByCountryCodeOrDefault(string countryCode)
    {
        if (_IsExpiredFetch()) return null;

        string searchVal = countryCode.ToLower();
        CountryDetailsBE countryDetails;
        lock (_cacheContentLock)
        {
            countryDetails =
                _cache?.FirstOrDefault(country => country.CountryCode.ToLower() == searchVal);
        }
        return countryDetails;
    }
    
    public string GetCurrencySymbolByCurrencyCodeOrDefault(string currencyCode)
    {
        if (_IsExpiredFetch()) return null;

        string searchVal = currencyCode.ToLower();
        List<CountryDetailsBE> countryDetails;
        lock (_cacheContentLock)
        {
            countryDetails = _cache?.Where(country => country.CurrencyCode.ToLower() == searchVal).ToList();
        }

        return countryDetails?.FirstOrDefault()?.CurrencySymbol;
    }

    public bool IsCacheExpired()
    {
        bool isExpired;

        lock (_cacheTTLLock)
        {
            isExpired = (DateTime.Now - _lastCacheSetDateTime) > _cacheEntryTTL;
        }

        return isExpired;
    }

    public void ExtendCacheTimeToLive()
    {
        lock (_cacheTTLLock)
        {
            // If another lock holder beat us to this by a few ms that's fine
            // We'll just set these again it's not a big deal
            _lastCacheSetDateTime = DateTime.Now;
            _cacheEntryTTL = TimeSpan.FromMinutes(30);
        }
        Console.WriteLine("Country Cache TTL was extended by 30 minutes.");
    }

    private bool _IsExpiredFetch()
    {
        if (!IsCacheExpired()) return false;

        lock (_cacheContentLock)
        {
            // By time we get here someone else in another thread could have been
            // holding the _cacheContentLock b/c they were setting new values
            // We need to double check the reason we're here is still valid
            if (!IsCacheExpired()) return false;

            _cache = null;
            return true;
        }
    }
}