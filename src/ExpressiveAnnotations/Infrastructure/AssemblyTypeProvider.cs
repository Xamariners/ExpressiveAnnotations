using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressiveAnnotations.Infrastructure
{
    using System.Reflection;

    internal interface ITypeProvider
    {
        IEnumerable<Type> GetTypes();
    }

    internal class AssemblyTypeProvider : ITypeProvider
    {
        public AssemblyTypeProvider(Assembly assembly)
        {
            Assembly = assembly;
        }

        private Assembly Assembly { get; set; }
        public IEnumerable<Type> GetTypes() { return Assembly.ExportedTypes; }
    }
}
