using System;
using System.Collections.Generic;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate.Projections
{
    public class BarProjection
    {
        public Guid Id { get; set; }
        public string LastText { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class BarWithoutMessagesProjection
    {
        public Guid Id { get; set; }
        public string LastText { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BarWithIdOnlyProjection
    {
        public Guid Id { get; set; }
    }
}
