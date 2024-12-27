using System.Linq;
using System.Reflection;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorAssemblyWrapper : AssemblyWrapper
    {
        private string[] _referencedAssemblies;

        public EditorAssemblyWrapper(Assembly assembly)
            : base(assembly) {}

        public override string[] GetReferencedAssemblies()
        {
            return _referencedAssemblies ??= Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        }

        public override string Location { get { return Assembly.Location; } }
    }
}
