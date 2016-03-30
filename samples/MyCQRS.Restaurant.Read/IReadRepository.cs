using System;
using System.Collections.Generic;

namespace MyCQRS.Restaurant.Read
{
    public interface IReadRepository<TModel>
        where TModel : class 
    {
        IEnumerable<TModel> GetAll();

        TModel GetById(Guid id);

        void Insert(TModel model);

        void Update(TModel model);
    }
}