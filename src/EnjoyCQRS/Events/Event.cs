using System;
using EnjoyCQRS.Collections;

namespace EnjoyCQRS.Events
{
    public class Event<TEvent> : IEventWrapper
        where TEvent : IDomainEvent
    {
        IMetadataCollection IEventWrapper.Attributes => Attributes;
        IDomainEvent IEventWrapper.InnerEvent => InnerEvent;

        public IMetadataCollection Attributes { get; }
        public TEvent InnerEvent { get; }
        
        public Event(TEvent innerEvent, IMetadataCollection attributes)
        {
            if (innerEvent == null) throw new ArgumentNullException(nameof(innerEvent));

            InnerEvent = innerEvent;
            Attributes = attributes;
        }
    }

    public interface IEventWrapper : IDomainEvent
    {
        IMetadataCollection Attributes { get; }
        IDomainEvent InnerEvent { get; }
    }
}
