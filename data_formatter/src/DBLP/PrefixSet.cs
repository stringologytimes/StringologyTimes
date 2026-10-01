class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; } = new();
    public int WordIndex { get; set; } = -1;
}

class PrefixSet
{
    private TrieNode root = new();
    private List<string> keyList = new();
    private List<string> valueList = new();

    public void Add(string key, string value)
    {
        this.keyList.Add(key);
        this.valueList.Add(value);
        var index = this.keyList.Count - 1;
        var node = root;

        foreach (char c in key)
        {
            if (!node.Children.TryGetValue(c, out var next))
            {
                next = new TrieNode();
                node.Children[c] = next;
            }

            node = next;
        }

        node.WordIndex = index;
    }
    public string GetKey(int index)
    {
        return this.keyList[index];
    }
    public string GetValue(int index)
    {
        return this.valueList[index];
    }
    public void Clear()
    {
        this.root = new TrieNode();
        this.keyList.Clear();
        this.valueList.Clear();
    }

    public int PrefixSearch(string s)
    {
        var node = root;

        foreach (char c in s)
        {
            if (node.WordIndex >= 0)
                return node.WordIndex;

            if (!node.Children.TryGetValue(c, out node))
                return -1;
        }

        return node.WordIndex;
    }

    private void RecoverKeyListSub(TrieNode node, string prefix, List<string> keyList)
    {
        if (node.WordIndex >= 0)
        {
            keyList.Add(prefix + this.keyList[node.WordIndex]);
        }
        foreach (var child in node.Children)
        {
            RecoverKeyListSub(child.Value, prefix + child.Key, keyList);
        }
    }

    public List<string> RecoverKeyList()
    {
        var keyList = new List<string>();
        RecoverKeyListSub(this.root, "", keyList);
        return keyList;
    }
}