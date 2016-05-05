using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EnjoyCQRS.Configuration
{
    public class AssemblyTypeScanner : IEnjoyTypeScanner
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public AssemblyTypeScanner(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<Type> Scan()
        {
            return _assemblies.SelectMany(e => e.GetExportedTypes());
        }
    }
}