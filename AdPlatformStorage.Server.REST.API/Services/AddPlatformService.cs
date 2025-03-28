using AdPlatformStorage.Server.REST.API.Models;
using AdPlatformStorage.Server.Storage.Trie;
using Microsoft.Extensions.Caching.Memory;
using AdPlatformStorage.Server.Core.Exceptions;

namespace AdPlatformStorage.Server.REST.API.Services;

public class AddPlatformService : IDisposable
{
    
    #region Fields

    /// <summary>
    /// 
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// 
    /// </summary>
    private bool _isDisposed = false;

    
    /// <summary>
    /// Storage used to manage advertising platform data.
    /// </summary>
    /// <remarks>
    /// In the future, this can be extended to interact with a different storage solution,  
    /// such as a database.
    /// </remarks>
    private ITrie<string> _storage;

    #endregion
    
    #region Constructors
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    public AddPlatformService(IMemoryCache cache)
    {
        try
        {
            _cache = cache;
        }
        catch
        {
            Dispose();
            throw;
        }
     
    }
    
    #endregion
    
    #region Methods
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="platforms"></param>
    /// <param name="cancellationToken"></param>
    public async Task SaveAdPlatformAsync(List<AdPlatformModel> platforms, 
        CancellationToken cancellationToken = default)
    {

        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:/?#[]@!$&'()*+,;=";
        
        _storage = new Trie<string>(alphabet);
    
        if (_cache is MemoryCache concreteMemoryCache) 
        {
            concreteMemoryCache.Clear();
        }
        
        await Task.Run(() =>
        {
            foreach (var platform in platforms)
            {
                foreach (var local in platform.Sources)
                {
                    _storage.Add(local, platform.Name);
                }
            }
        }, cancellationToken);
        
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RestException.NotFoundStorageException"></exception>
    public async Task<AdPlatformModel> GetAdPlatformAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {

        if (_storage == null)
        {
            throw new RestException.NotFoundStorageException(
                $"No storage available. Please create storage first via POST /api/storage",
                nameof(AddPlatformService));
        }
        
        /*( ( Trie<string>)_storage).Print();*/
        
        if (_cache.TryGetValue(key, out IEnumerable<string>? data))
        {
            return new AdPlatformModel
            {
                Name = key,
                Sources = data.ToList()
            };

        }
        
        var newData =  await _storage.GetAccumulateValuePathAsync(key, cancellationToken);
        
        _cache.Set(
            key, 
            newData, 
            new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            );

        return new AdPlatformModel
        {
            Name = key,
            Sources = newData.ToList()
        };
        
    }
    
    
    #endregion
    
    /// <summary>
    /// freeing up resources
    /// </summary>
    public void Dispose()
    {
        
        if (_isDisposed)
            return;
        
        if (_cache != null)
        {
            _cache.Dispose();
        }
        
        _isDisposed = true;
    }

    
    
}