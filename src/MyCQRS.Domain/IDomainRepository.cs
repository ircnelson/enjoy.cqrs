﻿using System;

namespace MyCQRS.Domain
{
    public interface IDomainRepository
    {
        void Add<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate;
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new();
    }
}
