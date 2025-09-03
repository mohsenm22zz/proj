using System;

namespace CircuitSimulator
{
    public abstract class Component
    {
        public string Name { get; set; }
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }

        protected Component()
        {
            Name = "";
            Node1 = null;
            Node2 = null;
        }

        public abstract double GetCurrent();
        public abstract double GetVoltage();
        public virtual void SetCurrent(double current) { }
    }
}