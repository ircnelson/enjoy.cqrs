using System;
using System.Collections.Generic;

namespace EnjoyCQRS.Configuration
{
    public interface IEnjoyTypeScanner
    {
        IEnumerable<Type> Scan();
    }
}