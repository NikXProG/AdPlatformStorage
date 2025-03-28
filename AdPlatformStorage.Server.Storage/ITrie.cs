namespace AdPlatformStorage.Server.Storage
{
    
}

public interface ITrie<TValue> 
{
   
    public  HashSet<TValue> Obtain(
        string key);

    public void Remove(
        string key);
    
    public void Add(
        string key, 
        TValue value);
    
    public void AddCollection(
        string key,
        HashSet<TValue> value);
    
    public bool TryGetValue(
        string key, 
        out HashSet<TValue?> value);
    
    public List<TValue> GetAccumulateValuePath(
        string key);

    public Task<List<TValue>> GetAccumulateValuePathAsync(
        string key,
        CancellationToken token);

    public HashSet<TValue> this[string key]
    {
        get;
        set;
    }
    
    
    
}