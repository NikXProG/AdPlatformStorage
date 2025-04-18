﻿using DryIoc;
using Microsoft.Extensions.Configuration;


namespace AdPlatformStorage.Server.Core;

public sealed class ServiceRegistratorsCompositionRoot
{
    
    #region Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <param name="registrator"></param>
    /// <param name="serviceRegistrators"></param>
    /// <param name="configuration"></param>
    public ServiceRegistratorsCompositionRoot(
        IRegistrator registrator,
        IEnumerable<IServiceRegistrator>? serviceRegistrators,
        IConfiguration configuration)
    {
        foreach (var serviceRegistrator in serviceRegistrators ?? Enumerable.Empty<IServiceRegistrator>())
        {
            serviceRegistrator?.Register(registrator, configuration);
        }
    }

    #endregion
    
}


