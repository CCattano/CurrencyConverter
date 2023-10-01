using System.Net;
using Microsoft.OpenApi.Models;
using Torty.Web.Apps.CurrencyConverter.Adapters.Adapters;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Caches;
using Torty.Web.Apps.CurrencyConverter.Infrastructure.Clients.CurrencyConverter;

namespace Torty.Web.Apps.CurrencyConverter.WebService;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        #region FRAMEWORK

        services.AddHttpContextAccessor();
        services.AddControllers();

        #endregion

        #region ADAPTERS

        services.AddScoped<ICommandsAdapter, CommandsAdapter>();
        services.AddScoped<ICurrencyConversionAdapter, CurrencyConversionAdapter>();
        services.AddScoped<ICountryDetailsAdapter, CountryDetailsAdapter>();
        
        #endregion

        #region CACHES

        services.AddCountryDetailsCache();

        #endregion

        #region CLIENTS

        services.AddCurrencyConverter();

        #endregion

        #region EXTERNAL

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CurrencyConverter", Version = "v1" });
        });

        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

        #endregion
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.MapWhen(
            // When request path is /status/isalive.
            path => path.Request.Path.Value == "/status/isalive",
            builder => builder.Run(async context =>
            {
                const string response = "CurrencyConverter Lambda is currently running.";
                Console.WriteLine(response); // WriteLine for AWS Lambda Application Log
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                // Return this message.
                await context.Response.WriteAsync(response);
            })
        );

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyConverter v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}