namespace AdPlatformStorage.Server.Storage.Trie
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class TrieNode<TValue>
    {
        
        #region Fields

        private readonly Dictionary<char, TrieNode<TValue>> _children;
        private readonly HashSet<TValue> _values;

        #endregion
        
        #region Constructors
        public TrieNode(int alphabetSize)
        {
            _children = new Dictionary<char, TrieNode<TValue>>(alphabetSize);
            _values = new HashSet<TValue>();
        }
        
        #endregion
        
        #region Properties
        public bool ValueIsEmpty => _values.Count == 0;

        public HashSet<TValue> Value => new(_values);

        public Dictionary<char, TrieNode<TValue>> Children => _children;
        
        #endregion
        
        #region Methods
        public void AddValue(TValue value)
        {
            _values.Add(value);
        }
        
        public void AddValues(IEnumerable<TValue> values)
        {
            _values.UnionWith(values);
        }
        
        public void ClearValues()
        {
            _values.Clear();
        }
        
        public bool TryRemoveChild(char key)
        {
            return _children.Remove(key);
        }
        
        #endregion
    }    
}
