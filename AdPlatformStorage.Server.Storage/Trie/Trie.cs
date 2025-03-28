namespace AdPlatformStorage.Server.Storage.Trie
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Trie<TValue> : ITrie<TValue>
    {
        
        #region Fields

        private readonly HashSet<char> _alphabet;
        private readonly bool[] _allowedChars;

        #endregion
        
        #region Constructors
        
        public Trie(string alphabet)
        {
            if (string.IsNullOrEmpty(alphabet))
                throw new ArgumentException("Alphabet cannot be null or empty.", nameof(alphabet));

            _alphabet = new HashSet<char>(alphabet);
            
            if (_alphabet.Count != alphabet.Length)
                throw new ArgumentException("Alphabet must contain unique characters.", nameof(alphabet));
            
            _allowedChars = new bool[char.MaxValue + 1];
            foreach (var c in _alphabet)
                _allowedChars[c] = true;

            Root = new TrieNode<TValue?>(_alphabet.Count);
        }

        
        #endregion
        
        #region Properties
        
        public TrieNode<TValue?> Root { get; }

        #endregion
        
        #region Methods
        public bool TryGetValue(string key, out HashSet<TValue?> value)
        {
            if (!TryGetNode(key, out var node) || node.ValueIsEmpty)
            {
                value = null;
                return false;
            }

            value = node.Value;
            return true;
        }

        public void Remove(string key)
        {
            if (!TryGetNode(key, out var node) || node.ValueIsEmpty)
                throw new KeyNotFoundException("Key not found.");

            node.ClearValues();
            RemoveEmptyNodes(key);
        }


        public HashSet<TValue> Obtain(string key)
        {
            return TryGetValue(key, out var value)
                ? value 
                : throw new KeyNotFoundException("Key not found.");
        }

        public HashSet<TValue> this[string key]
        {
            get => Obtain(key);
            set => AddCollection(key, value);
        }

        public void Add(string key, TValue value)
        {
            ValidateKey(key);
            var node = GetOrCreateNode(key);
            node.AddValue(value); 
        }

        public void AddCollection(string key, HashSet<TValue> collection)
        {
            ValidateKey(key);
            var node = GetOrCreateNode(key);
            node.AddValues(collection); 
        }

        public List<TValue> GetAccumulateValuePath(string key)
        {
            var nodes = SearchNode(key);
            var result = new List<TValue>(nodes.Count * 2); 

            foreach (var node in nodes)
            {
                if (node.Value?.Count > 0)
                    result.AddRange(node.Value);
            }

            return result;
        }

        public async Task<List<TValue>> GetAccumulateValuePathAsync(string key, 
            CancellationToken cancellationToken = default)
        {
            var nodes = await Task.Run(() => SearchNode(key), cancellationToken);
            var result = new List<TValue>(nodes.Count * 2);

            await Task.Run(() =>
            {
                foreach (var node in nodes)
                {
                    if (node.Value?.Count > 0)
                        result.AddRange(node.Value);
                }
            }, cancellationToken);

            return result;
        }

      

        private bool TryGetNode(string key, out TrieNode<TValue?> node)
        {
            node = Root;
            foreach (var k in key)
            {
                if (!_allowedChars[k] || !node.Children.TryGetValue(k, out node))
                {
                    node = null;
                    return false;
                }
            }
            return true;
        }

        private TrieNode<TValue?> GetOrCreateNode(string key)
        {
            var node = Root;
            foreach (var k in key)
            {
                var children = node.Children;
                if (!children.TryGetValue(k, out var child))
                {
                    child = new TrieNode<TValue?>(_alphabet.Count);
                    children[k] = child;
                }
                node = child;
            }
            return node;
        }

        private Stack<TrieNode<TValue?>> SearchNode(string key)
        {
        
            var node = Root;
            var path = new Stack<TrieNode<TValue?>>(key.Length);

            foreach (var k in key)
            {
                if (!_allowedChars[k])
                    throw new ArgumentException($"Invalid character '{k}' in key");

                if (!node.Children.TryGetValue(k, out node))
                    return new Stack<TrieNode<TValue?>>();

                path.Push(node);
            }

            return path;
            
        }

        private void ValidateKey(string key)
        {
            foreach (var c in key)
            {
                if (c > char.MaxValue || !_allowedChars[c])
                    throw new ArgumentException($"Invalid character '{c}' in key");
            }
        }

        private void RemoveEmptyNodes(string key)
        {
           
            var path = SearchNode(key);
            if (path.Count == 0) return;

            var node = path.Pop();
            while (path.Count > 0 && node.ValueIsEmpty && node.Children.Count == 0)
            {
                var parent = path.Peek();
                parent.Children.Remove(key[path.Count - 1]);
                node = path.Pop();
            }
        }
        
        
        #endregion
        
    }
}