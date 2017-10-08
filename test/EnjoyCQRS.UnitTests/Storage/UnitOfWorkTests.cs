using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    [Trait("Unit", "Unit of work")]
    public class UnitOfWorkTests
    {   
        [Fact]
        public void Should_call_Rollback_1()
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

            mockSession.Verify(e => e.Rollback());
        }

        [Fact]
        public void Should_call_Rollback_2()
        {
            Mock<ISession> mockSession = new Mock<ISession>();
            mockSession.SetupAllProperties();
            mockSession.Setup(e => e.SaveChangesAsync())
                .Callback(() =>
                {
                    throw new Exception("Intentional exception");
                })
                .Returns(Task.CompletedTask);

            UnitOfWork unitOfWork = new UnitOfWork(mockSession.Object);

            Func<Task> act = async () => { await unitOfWork.CommitAsync(); };

            act.ShouldThrowExactly<Exception>().WithMessage("Intentional exception");

            mockSession.Verify(e => e.Rollback());
        }
    }
}