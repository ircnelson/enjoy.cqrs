using System;

namespace MyCQRS.Bus.Direct
{
    public class RouteNotRegisteredException : Exception
    {
        public RouteNotRegisteredException(Type messageType) : base($"No route specified for message '{messageType.FullName}'")
        {
        }
    }
}