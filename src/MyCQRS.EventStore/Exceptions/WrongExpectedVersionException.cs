using System;

namespace MyCQRS.EventStore.Exceptions
{
    public class WrongExpectedVersionException : Exception
    {
        public WrongExpectedVersionException(string message) : base(message)
        {
        }
    }
}