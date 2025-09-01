using System.Collections.Generic;
using System.Windows;

namespace wpfUI
{
    public class NetlistNode
    {
        public int Id { get; }
        public List<UIElement> ConnectedElements { get; } = new List<UIElement>();

        public NetlistNode(int id)
        {
            Id = id;
        }
    }

    public class NetlistComponent
    {
        public string Name { get; }
        public string Type { get; }
        public NetlistNode Node1 { get; set; }
        public NetlistNode Node2 { get; set; }

        public NetlistComponent(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}