using System;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

[assembly: FunctionsStartup(typeof(BackendApi.Startup))]
namespace BackendApi
{
    public class Startup : FunctionsStartup
    {

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder
                   .AddEnvironmentVariables();

            base.ConfigureAppConfiguration(builder);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureAppSettings(builder.Services);
            ConfigureHttpClient(builder.Services);
        }

        private static void ConfigureAppSettings(IServiceCollection services)
        {
            var options = new DefaultOpenApiConfigurationOptions()
            {
                OpenApiVersion = OpenApiVersionType.V3,
                Info = new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "ChatBot Function",
                    Description = "This is the function app as a chatbot."
                }
            };

            var codespaces = bool.TryParse(Environment.GetEnvironmentVariable("OpenApi__RunOnCodespaces"), out var isCodespaces) && isCodespaces;
            if (codespaces)
            {
                options.IncludeRequestingHostName = false;
            }

            services.AddSingleton<IOpenApiConfigurationOptions>(options);
        }

        private static void ConfigureHttpClient(IServiceCollection services)
        {
            services.AddHttpClient("chat");
        }
    }
}