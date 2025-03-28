using System.Net;
using AdPlatformStorage.Server.Core;
using AdPlatformStorage.Server.REST.API.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AdPlatformStorage.Server.REST.API
{
    public class WebHostConfigurator : IWebHostConfigurator
    {
        public void Configure(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .ConfigureKestrel((context, options) =>
                {
                    var fileUploadSettings = new FileUploadSettings();
        
                    context.Configuration.Bind(nameof(FileUploadSettings), fileUploadSettings);

                    
                    var serverSettings = new ServerSettings();
                    context.Configuration.Bind(nameof(ServerSettings), serverSettings);


                    options.Limits.MaxRequestBodySize = fileUploadSettings.MaxFileSize;
                
                    options.Listen(
                        IPAddress.Parse(serverSettings.ListenAddress),
                        serverSettings.ListenPort,
                        listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1;
                            // listenOptions.UseHttps(serverSettings.CertPath, serverSettings.CertPassword);
                        });
                });
        }
    }
}
