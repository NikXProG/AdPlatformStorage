using System.Text.Json.Serialization;
using AdPlatformStorage.Server.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using AdPlatformStorage.Server.REST.API.Controller;
using AdPlatformStorage.Server.REST.API.Settings;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdPlatformStorage.Server.REST.API
{
    public class ApplicationConfigurator : IApplicationConfigurator, IStartup
    {
        #region RGU.WebProgramming.Server.Core.IApplicationConfigurator implementation
    
        /// <inheritdoc cref="IApplicationConfigurator.Configure" /> 
        public void Configure(
            IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseRouting();
            applicationBuilder.UseEndpoints(endpointRouteBuilder =>
            {
                endpointRouteBuilder.MapControllers();
            });
        }
    
        #endregion
    
        #region RGU.WebProgramming.Server.Core.IStartup implementation
    
        /// <inheritdoc cref="Core.IStartup.ConfigureServices" />
        public void ConfigureServices(
            HostBuilderContext ctx,
            IServiceCollection services)
        {
            var fileUploadSettings = new FileUploadSettings();
        
            ctx.Configuration.Bind(nameof(FileUploadSettings), fileUploadSettings);
        
            services
                .Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = 524288000;
                })
                .AddMemoryCache()
                .AddControllers()
                .AddApplicationPart(typeof(AdvertisingController).Assembly)
                .AddNewtonsoftJson();
        }
    
        #endregion
    
    
    }
}
