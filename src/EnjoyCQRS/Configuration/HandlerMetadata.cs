using System;

namespace EnjoyCQRS.Configuration
{
    public struct HandlerMetadata
    {
        public HandlerType HandlerType { get; }
        public Type DtoType { get; }

        public HandlerMetadata(Type dtoType, HandlerType handlerType)
        {
            HandlerType = handlerType;
            DtoType = dtoType;
        }

    }
}