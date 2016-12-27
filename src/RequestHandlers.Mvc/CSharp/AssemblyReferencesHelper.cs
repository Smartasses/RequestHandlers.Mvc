using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RequestHandlers.Mvc.CSharp
{
    class AssemblyReferencesHelper
    {
        private readonly Dictionary<string, Assembly> _neededAssemblies;

        public AssemblyReferencesHelper()
        {
            _neededAssemblies = new Dictionary<string, Assembly>();
        }
        public AssemblyReferencesHelper AddReferenceForTypes(params Type[] types)
        {
            types.Select(x => x.GetTypeInfo().Assembly)
                .Distinct()
                .ToList().ForEach(AddAssembly);
            return this;
        }

        public IEnumerable<PortableExecutableReference> GetReferences() => _neededAssemblies.Keys.Select(x => MetadataReference.CreateFromFile(x));
        private void AddAssembly(Assembly assembly)
        {
            if (_neededAssemblies.ContainsKey(assembly.Location)) return;
            _neededAssemblies.Add(assembly.Location, assembly);
            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                var refAssembly = Assembly.Load(referencedAssembly);
                AddAssembly(refAssembly);
            }
        }
    }
}