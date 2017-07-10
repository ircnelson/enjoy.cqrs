using System;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public class AllUserView
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
        public int BirthMonth { get; set; }
        public int BirthYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public TimeSpan? Lifetime { get; set; }
    }
}
