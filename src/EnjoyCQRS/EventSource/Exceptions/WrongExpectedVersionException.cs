using System;

namespace EnjoyCQRS.EventSource.Exceptions
{
    public class WrongExpectedVersionException : Exception
    {
        public WrongExpectedVersionException(string message) : base(message)
        {
        }
    }
}