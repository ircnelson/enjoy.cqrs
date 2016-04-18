﻿namespace MyCQRS.Bus
{
    public interface IUnitOfWork
    {
        void Commit();
        void Rollback();
    }
}