using System;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class RepositoryTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Repository";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_pass_null_instance_of_LoggerFactory()
        {
            var session = Mock.Of<ISession>();

            Action act = () => new Repository(null, session);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_pass_null_instance_of_Session()
        {
            Action act = () => new Repository(CreateLoggerFactory(CreateLoggerMock().Object), null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        private static Mock<ILogger> CreateLoggerMock()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockLogger.Setup(e => e.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()));

            return mockLogger;
        }

        private static ILoggerFactory CreateLoggerFactory(ILogger logger)
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(e => e.Create(It.IsAny<string>())).Returns(logger);

            return mockLoggerFactory.Object;
        }
    }
}