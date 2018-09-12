using App.Metrics.Health;
using ContractFeatures.Models;
using Flurl.Http;
using FlurlClientWrapper.Core.AuthenticationStrategies;
using InvoicePrint.Extensions;
using InvoicePrint.Managers;
using LogApiRequest.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Extensions.Logging;
using SnakeCaseValueProviderFactory.Core;
using ConfigurationExtensions = InvoicePrint.Extensions.ConfigurationExtensions;
using ILogger = NLog.ILogger;

namespace InvoicePrint
{
    public class Startup
    {
        private static readonly ILogger Logger = LogManager.GetLogger("ContractFeatures");

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            ConfigurationExtensions.Environment = env.EnvironmentName;
            ConfigurationExtensions.Current = builder.Build();
            //if (ConfigurationExtensions.IsDevelopment())
            //    builder.AddUserSecrets<Startup>();

            FlurlHttp.Configure(settings =>
            {
                settings.BeforeCall =
                    call => Logger.Info(new LogData("Calling api",
                            $"{{ \"method\": \"{call.Request?.Method}\", \"url\": \"{call.Request?.RequestUri}\", \"headers\": \"{call.Request?.Headers?.ToString()} {call.Request?.Content?.Headers?.ToString()}\", , \"content\": \"{call.Request?.Content?.ReadAsStringAsync().Result}\" }}")
                        .ToJson());
                settings.OnError = call => Logger.Error(call.Exception,
                    new LogData("Error on api call",
                            (call.Exception as FlurlHttpException)?.GetResponseJsonAsync().Result)
                        .ToJson());
            });
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(ConfigurationExtensions.Current);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAntiforgery(options => { options.SuppressXFrameOptionsHeader = false; });

            const long threshold = 7000000000; // ~7 GB
            const long thresholdVirtualMemory = 2560000000000; // ~256 GB
            AppMetricsHealth.CreateDefaultBuilder()
                .HealthChecks.AddProcessPrivateMemorySizeCheck("Private Memory Size", threshold, true)
                .HealthChecks.AddProcessVirtualMemorySizeCheck("Virtual Memory Size", thresholdVirtualMemory, true)
                .HealthChecks.AddProcessPhysicalMemoryCheck("Working Set", threshold, true)
                .BuildAndAddTo(services);
            services.AddMvc();

            AddMvc(services);
        }

        private static void AddMvc(IServiceCollection services)
        {
          
            services.AddMvc(options =>
                {
                    options.ValueProviderFactories.Add(new SnakeCaseQueryStringValueProviderFactory());
                })
                .AddJsonOptions(json =>
                {
                    json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    json.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };
                });
           
            ConfigureActivitiesApi(services);
            ConfigureContractsApi(services);
            services.AddTransient<IApiManager, ApiManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
          
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto });
            app.UseLogApiRequest();
            app.UseCors(builder =>
                builder.WithOrigins("https://stg-status.cb.com", "https://status.cb.com",
                        "https://finance-axiom.cb.com", "https://stg-finance-axiom.cb.com")
            .AllowAnyMethod()
            .AllowAnyHeader());

            loggerFactory.AddNLog();
            app.UseMiddleware<LogApiRequestMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStatusCodePagesWithReExecute("/Error");
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseHealthAllEndpoints();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=InvoicePrint}/{action=Details}/{id?}");
            });

        }

        private static void ConfigureActivitiesApi(IServiceCollection services)
        {
            var postContractFeatureUpdatesUrl =
                $"{ConfigurationExtensions.Current["ActivitiesApi:Protocol"]}://{ConfigurationExtensions.Current["ActivitiesApi:Domain"]}/{ConfigurationExtensions.Current["ActivitiesApi:ContractFeaturesUpdateEndpoint"]}";
            var authorizationMode = ConfigurationExtensions.Current["ActivitiesApi:AuthorizationMode"];
            IAuthenticationStrategy authStrategy = null;
            if (authorizationMode.Equals("Partner"))
                authStrategy = ConfigurePartnerAuthentication("ActivitiesApi");
            else if (authorizationMode.Equals("OAuth2"))
                authStrategy = ConfigureOAuthAuthentication("ActivitiesApi");
            else
                Logger.Error(new LogData("Activities api.Unsupported authentication strategy", string.Empty).ToJson());
            //services.AddTransient<IActivitiesApi, ActivitiesApi>(
            //    provider => new ActivitiesApi(postContractFeatureUpdatesUrl, authStrategy, new HttpContextAccessor()));
        }

        private static void ConfigureContractsApi(IServiceCollection services)
        {
            var contractFeaturesUrl =
                $"{ConfigurationExtensions.Current["ContractsApi:Protocol"]}://{ConfigurationExtensions.Current["ContractsApi:Domain"]}/{ConfigurationExtensions.Current["ContractsApi:ContractFeaturesEndpoint"]}";

            // For now just assume Oauth
            IAuthenticationStrategy authStrategy = ConfigureOAuthAuthentication("ContractsApi");
            //services.AddTransient<IContractsApi, ContractsApi>(
            //    provider => new ContractsApi(authStrategy, new HttpContextAccessor(), contractFeaturesUrl));
        }

        private static IAuthenticationStrategy ConfigurePartnerAuthentication(string configKey)
        {
            var partnerId =
                ConfigurationExtensions.Current[$"{configKey}:PartnerStrategy:Id"];
            var partnerPassword = ConfigurationExtensions.Current[$"{configKey}:PartnerStrategy:Password"];
            return new PartnerAuthenticationStrategy(partnerId, partnerPassword);
        }

        private static IAuthenticationStrategy ConfigureOAuthAuthentication(string configKey)
        {
            var clientId = ConfigurationExtensions.Current[$"{configKey}:OAuth2AuthenticationStrategy:ClientId"];
            var clientSecret =
                ConfigurationExtensions.Current[$"{configKey}:OAuth2AuthenticationStrategy:ClientSecret"];
            var tokenUrl = ConfigurationExtensions.Current[$"{configKey}:OAuth2AuthenticationStrategy:Address"];
            return new OAuthAuthenticationStrategy(tokenUrl, clientId, clientSecret);
        }
    }
}
