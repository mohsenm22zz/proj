namespace CircuitSimulator
{
    public abstract class Component
    {
        public string Name { get; set; }
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }
        public double Value { get; set; }

        protected Component(string name, Node node1, Node node2, double value)
        {
            Name = name;
            Node1 = node1;
            Node2 = node2;
            Value = value;
        }
    }
}