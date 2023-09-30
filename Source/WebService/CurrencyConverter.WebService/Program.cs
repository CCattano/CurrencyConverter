using Torty.Web.Apps.CurrencyConverter.WebService;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync();