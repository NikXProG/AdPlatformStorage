using AdPlatformStorage.Server.Core;
using AdPlatformStorage.Server.REST.API.Controller;
using AdPlatformStorage.Server.REST.API.Services;
using DryIoc;
using Microsoft.Extensions.Configuration;

namespace AdPlatformStorage.Server.REST.API
{
    
    public sealed class ServiceRegistrator:
        IServiceRegistrator
    {
    
        #region RGU.WebProgramming.Server.Core.IServiceRegistrator implementation
    
        /// <inheritdoc cref="IServiceRegistrator.Register" />
        public void Register(
            IRegistrator registrator,
            IConfiguration configuration)
        {
            registrator.RegisterMany<ApplicationConfigurator>(Reuse.Singleton);
            registrator.Register<IWebHostConfigurator, WebHostConfigurator>(Reuse.Singleton);
            registrator.Register<AdvertisingController>(Reuse.Singleton);
            registrator.Register<AddPlatformService> (Reuse.Singleton);
        
        }
    
        #endregion
    
    }
}
