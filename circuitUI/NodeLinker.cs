using System.Collections.Generic;

namespace wpfUI
{
    public class NodeLinker
    {
        private readonly Dictionary<string, string> _parent;
        private readonly Dictionary<string, int> _rank;

        public NodeLinker()
        {
            _parent = new Dictionary<string, string>();
            _rank = new Dictionary<string, int>();
        }

        public void AddNode(string node)
        {
            if (_parent.ContainsKey(node)) return;
            _parent[node] = node;
            _rank[node] = 0;
        }

        public string Find(string node)
        {
            if (_parent[node] == node)
                return node;
            return _parent[node] = Find(_parent[node]);
        }

        public void Union(string node1, string node2)
        {
            var root1 = Find(node1);
            var root2 = Find(node2);

            if (root1 == root2) return;
            if (_rank[root1] > _rank[root2])
            {
                _parent[root2] = root1;
            }
            else if (_rank[root1] < _rank[root2])
            {
                _parent[root1] = root2;
            }
            else
            {
                _parent[root2] = root1;
                _rank[root1]++;
            }
        }
    }
}