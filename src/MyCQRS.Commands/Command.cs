﻿using System;

namespace MyCQRS.Commands
{
    public abstract class Command : ICommand
    {
        public Guid AggregateId { get; }

        protected Command(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}