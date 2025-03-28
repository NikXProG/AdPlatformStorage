using Microsoft.AspNetCore.Hosting;

namespace AdPlatformStorage.Server.Core
{
   
    public interface IWebHostConfigurator
    {
   
        void Configure(IWebHostBuilder webHostBuilder);
   
    }
    
}
