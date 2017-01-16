using System;
using Moq;
using EnjoyCQRS.Logger;

namespace EnjoyCQRS.UnitTests.Shared
{
    public static class MockHelper
    {
        public static Mock<ILogger> GetMockLogger()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockLogger.Setup(e => e.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()));

            return mockLogger;
        }

        public static Mock<ILoggerFactory> GetMockLoggerFactory(ILogger logger)
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockLoggerFactory.Setup(e => e.Create(It.IsAny<string>())).Returns(logger);

            return mockLoggerFactory;
        }

        public static ILoggerFactory CreateLoggerFactory(ILogger logger)
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockLoggerFactory.Setup(e => e.Create(It.IsAny<string>())).Returns(logger);

            return mockLoggerFactory.Object;
        }
    }
}