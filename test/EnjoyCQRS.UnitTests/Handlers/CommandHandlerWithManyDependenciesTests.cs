using System;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.BarAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Handlers
{
    public class PrintSomethingTests : CommandTestFixture<ManyDependenciesCommand, ManyDependenciesCommandHandler, Bar>
    {
        protected override void SetupDependencies()
        {
            OnDependency<IBooleanService>().Setup(e => e.DoSomething()).Returns(true);
            OnDependency<IStringService>()
                .Setup(e => e.PrintWithFormat(It.IsAny<string>()))
                .Returns<string>(str => $"** {str} **");
        }

        protected override ManyDependenciesCommand When()
        {
            return new ManyDependenciesCommand("Hello World");
        }

        [Fact]
        public void Should_output_formatted_text()
        {
            CommandHandler.Output.Should().Be("** Hello World **");
        }
    }

    public class CaughtExceptionCommandHandlerTests : CommandTestFixture<ManyDependenciesCommand, ManyDependenciesCommandHandler, Bar>
    {
        protected override void SetupDependencies()
        {
            OnDependency<IBooleanService>().Setup(e => e.DoSomething()).Returns(true);
            OnDependency<IStringService>().Setup(e => e.PrintWithFormat(It.IsAny<string>()));
        }

        protected override ManyDependenciesCommand When()
        {
            return new ManyDependenciesCommand(string.Empty);
        }

        [Fact]
        public void Should_throw_ArgumentNullException()
        {
            CaughtException.Should().BeOfType<ArgumentNullException>();
        }
    }
}