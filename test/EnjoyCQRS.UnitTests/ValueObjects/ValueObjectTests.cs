using EnjoyCQRS.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.ValueObjects
{
    public class ValueObjectTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "ValueObjects";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_be_identical()
        {
            FullName fullName1 = new FullName("Walter", "White");
            FullName fullName2 = new FullName("Walter", "White");

            fullName1.Should().Be(fullName2);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_be_equals_by_comparison()
        {
            FullName fullName1 = new FullName("Walter", "White");
            FullName fullName2 = new FullName("Walter", "White");

            (fullName1 == fullName2).Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_compare_two_instances_Given_that_right_operand_is_nullable()
        {
            FullName fullName1 = null;
            FullName fullName2 = new FullName("Walter", "White");

            (fullName2 == fullName1).Should().BeFalse();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_compare_two_instances_Given_that_left_operand_is_nullable()
        {
            FullName fullName1 = new FullName("Walter", "White"); 
            FullName fullName2 = null;

            (fullName2 == fullName1).Should().BeFalse();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_be_the_same_HashCode()
        {
            FullName fullName1 = new FullName("Walter", "White");
            FullName fullName2 = new FullName("Walter", "White");

            (fullName1.GetHashCode() == fullName2.GetHashCode()).Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Theory]
        [InlineData("Walter", "Walter2")]
        [InlineData("Walter", null)]
        public void Should_not_be_identical(string firstName1, string firstName2)
        {
            FullName fullName1 = new FullName(firstName1, "White");

            FullName fullName2 = new FullName(firstName2, "White");

            fullName1.Should().NotBe(fullName2);
        }

        [Trait(CategoryName, CategoryValue)]
        [Theory]
        [InlineData("Walter", "Walter2")]
        [InlineData("Walter", null)]
        public void Should_not_be_equals_by_comparison(string firstName1, string firstName2)
        {
            FullName fullName1 = new FullName(firstName1, "White");

            FullName fullName2 = new FullName(firstName2, "White");

            (fullName1 != fullName2).Should().BeTrue();
        }
    }

    public class FullName : ValueObject<FullName>
    {
        public string FirstName { get; }
        public string LastName { get; }

        public FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}