using System;
using System.Reflection;

namespace UnityEngine.TestTools.Utils
{
    internal class AssemblyWrapper : IAssemblyWrapper
    {
        public AssemblyWrapper(Assembly assembly)
        {
            Assembly = assembly;
            Name = assembly.GetName().Name;
        }

        public Assembly Assembly { get; }

        public string Name { get; }

        public virtual string Location
        {
            get
            {
                //Some platforms dont support this
                throw new NotImplementedException();
            }
        }

        public virtual string[] GetReferencedAssemblies()
        {
            //Some platforms dont support this
            throw new NotImplementedException();
        }
    }
}
