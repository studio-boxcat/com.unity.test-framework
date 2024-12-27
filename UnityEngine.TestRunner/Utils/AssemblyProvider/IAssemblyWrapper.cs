using System;
using System.Reflection;

namespace UnityEngine.TestTools.Utils
{
    internal interface IAssemblyWrapper
    {
        Assembly Assembly { get; }
        string Name { get; }
        string Location { get; }
        string[] GetReferencedAssemblies();
    }
}
