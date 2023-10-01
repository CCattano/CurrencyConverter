using System.Text.RegularExpressions;
using Torty.Web.Apps.CurrencyConverter.BusinessEntities.Countries;

namespace Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;

public interface ICommandsAdapter
{
    Task<string> ParseCommand(string command);
}

public class CommandsAdapter : ICommandsAdapter
{
    private readonly ICurrencyConversionAdapter _conversionAdapter;
    private readonly ICountryDetailsAdapter _countryDetailsAdapter;

    private struct HelpMessages
    {
        public const string HelpMessage =
            @"!converter supports 2 actions, ""convert"" and ""code"". Use ""!converter convert help"" or ""!converter code help"" to learn more.";

        public const string ConvertHelpMessage =
            "The \"convert\" action converts an amount of money from one currency to another." +
            " It expects the following format: \"amount currency-code-A to currency-code-B\"."+
            " Example: !converter convert 10 USD to EUR";

        public const string CodeHelpMessage =
            "The \"code\" action returns the currency code for up to 2 countries." +
            " You can search by name (Ireland) or code (IRL)." +
            " If requesting codes for 2 countries separate them by commas." +
            " Examples: !converter code USA - !converter code United States - !converter code USA,Ireland - !converter code United States,IRL";
    }

    public CommandsAdapter(ICurrencyConversionAdapter conversionAdapter, ICountryDetailsAdapter countryDetailsAdapter)
    {
        _conversionAdapter = conversionAdapter;
        _countryDetailsAdapter = countryDetailsAdapter;
    }

    public async Task<string> ParseCommand(string command)
    {
        string validationMsg = _TryGetActionFromCommand(command, out Actions? action);
        if (!string.IsNullOrEmpty(validationMsg))
            return validationMsg;

        string response = action! switch
        {
            Actions.Help => HelpMessages.HelpMessage,
            Actions.Convert => await _ProcessConvertAction(command),
            Actions.Code => await _ProcessCodeAction(command)
        };

        return response;
    }

    private async Task<string> _ProcessConvertAction(string command)
    {
        string actionDetails = command["convert ".Length..];

        if (actionDetails.ToLower() == "help")
            return HelpMessages.ConvertHelpMessage;

        if (char.IsSymbol(actionDetails[0]))
            actionDetails = actionDetails[1..];
        
        if (new Regex(@"^[0-9,.]+\s[a-zA-Z]{3}\sto\s[a-zA-Z]{3}$").IsMatch(actionDetails))
        {
            string[] detailParts = actionDetails.Split(' ');
            string amount = detailParts[0];
            string from = detailParts[1];
            string to = detailParts[3];

            string response = await _conversionAdapter.ConvertCurrency(amount, from, to);
            return response;
        }

        return "Could not process convert request. Use \"!converter" +
               " convert help\" to learn how to use the convert action.";
    }

    private async Task<string> _ProcessCodeAction(string command)
    {
        string actionDetails = command["code ".Length..];

        if (actionDetails.ToLower() == "help")
            return HelpMessages.CodeHelpMessage;

        List<string> namesOrCodes = actionDetails.Split(',').Select(name => name.Trim()).ToList();

        Regex countryCodeRegex = new(@"^[a-zA-Z]{3}$");
        List<string> resultPerSearch = new();
        foreach (string nameOrCode in namesOrCodes)
        {
            bool isCountryCode = countryCodeRegex.IsMatch(nameOrCode);
            CountryDetailsBE countryDetails = isCountryCode
                ? await _countryDetailsAdapter.GetCountryByCountryCodeOrDefault(nameOrCode)
                : await _countryDetailsAdapter.GetCountryByNameOrDefault(nameOrCode);

            string result = $"Search Term: {nameOrCode} - ";
            if (countryDetails == null)
                result += "Could not determine intended country.";
            else
                result += $"Country Identified: {countryDetails.Name} - Currency Code: {countryDetails.CurrencyCode}.";
            resultPerSearch.Add(result);
        }

        string response = string.Join(' ', resultPerSearch);
        return response;
    }

    private static string _TryGetActionFromCommand(string command, out Actions? action)
    {
        const string defaultErrMsg =
            @"Could not identify the action to perform. Use the ""help"" (!converter help) action" +
            " to learn how !converter works. If this issue persists, bug Torty to look into it.";
        
        action = null;
        command = command?.Trim().ToLower() ?? string.Empty;

        if (!command.Contains(' ') && command != "help")
            return defaultErrMsg;

        if (command == "help")
        {
            action = Actions.Help;
            return null;
        }

        string actionStr = command.Split(" ").First();

        action = actionStr switch
        {
            "convert" => Actions.Convert,
            "code" => Actions.Code,
            _ => null
        };

        string errMsg = action.HasValue ? null : defaultErrMsg;
        return errMsg;
    }
}

enum Actions
{
    Convert,
    Code,
    Help
}