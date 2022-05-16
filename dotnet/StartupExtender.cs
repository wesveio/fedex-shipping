namespace Vtex
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using FedexShipping.Services;
    using FedexShipping.Data;

    public class StartupExtender
    {
        public void ExtendConstructor(IConfiguration config, IWebHostEnvironment env)
        {

        }

        public void ExtendConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFedExRateRequest, FedExRateRequest>();
            services.AddTransient<IFedExAvailabilityRequest, FedExAvailabilityRequest>();
            services.AddTransient<IFedExTrackRequest, FedExTrackRequest>();
            services.AddTransient<IFedExEstimateDeliveryRequest, FedExEstimateDeliveryRequest>();
            services.AddTransient<IFedExCacheRepository, FedExCacheRespository>();
            services.AddTransient<ILogisticsRepository, LogisticsRepository>();

            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddSingleton<IMerchantSettingsRepository, MerchantSettingsRepository>();
            services.AddSingleton<ICachedKeys, CachedKeys>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
        }

        // This method is called inside Startup.Configure() before calling app.UseRouting()
        public void ExtendConfigureBeforeRouting(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() before calling app.UseEndpoint()
        public void ExtendConfigureBeforeEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() after calling app.UseEndpoint()
        public void ExtendConfigureAfterEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}