using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class UnitOfWorkTests
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Unit of work";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_call_Rollback()
        {
            Mock<ISession> mockSession = new Mock<ISession>();
            mockSession.SetupAllProperties();
            mockSession.Setup(e => e.CommitAsync())
                .Callback(() =>
                {
                    throw new Exception("Intentional exception");
                })
                .Returns(Task.CompletedTask);

            UnitOfWork unitOfWork = new UnitOfWork(mockSession.Object);

            Func<Task> act = async () => { await unitOfWork.CommitAsync(); };

            act.ShouldThrowExactly<Exception>().WithMessage("Intentional exception");
        }
    }
}