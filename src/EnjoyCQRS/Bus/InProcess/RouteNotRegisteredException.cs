using System;

namespace EnjoyCQRS.Bus.InProcess
{
    public class RouteNotRegisteredException : Exception
    {
        public RouteNotRegisteredException(Type messageType) : base($"No route specified for message '{messageType.FullName}'")
        {
        }
    }
}