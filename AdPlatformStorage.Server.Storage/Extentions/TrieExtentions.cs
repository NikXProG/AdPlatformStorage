using AdPlatformStorage.Server.Storage.Trie;


namespace AdPlatformStorage.Server.Storage.Extentions
{
    
    public static class TrieExtentions
    {

        public static void Print<T>(this Trie<T> trie)
        {
            Print(trie.Root);
        }

        private static void Print<T>(this TrieNode<T> node, string currentKey = null!, int depth = 0)
        {
            if (!node.ValueIsEmpty)
            { 
                Console.WriteLine(" ".PadLeft(depth * 2) + $"{currentKey ?? "null"} : {(!node.ValueIsEmpty ? string.Join(',', node.Value) : "null")}");
            }
       
            foreach (var child in node.Children)
            {
                Print(child.Value, currentKey + child.Key, depth + 1);
            }
        }
    }
}
