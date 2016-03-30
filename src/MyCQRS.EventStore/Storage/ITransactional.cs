using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCQRS.EventStore.Storage
{
    public interface ITransactional
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
