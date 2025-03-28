using Microsoft.AspNetCore.Routing;

namespace AdPlatformStorage.Server.Core
{
    
    /// <summary>
    /// 
    /// </summary>
    public interface IEndpointsConfigurator
    {
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeBuilder"></param>
        void Configure(
            IEndpointRouteBuilder routeBuilder);
    
    }
}
