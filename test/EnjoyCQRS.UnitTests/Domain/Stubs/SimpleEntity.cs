using System;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    public class SimpleEntity
    {
        public Guid Id { get; }
        public bool Enabled { get; private set; }
        public string Name { get; private set; }

        public SimpleEntity(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void Disable()
        {
            Enabled = false;
        }
    }
}