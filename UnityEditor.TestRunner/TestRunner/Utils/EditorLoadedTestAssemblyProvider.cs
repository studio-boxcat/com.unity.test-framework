using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorLoadedTestAssemblyProvider : IEditorLoadedTestAssemblyProvider
    {
        private const string k_NunitAssemblyName = "nunit.framework";
        private const string k_TestRunnerAssemblyName = "UnityEngine.TestRunner";
        internal const string k_PerformanceTestingAssemblyName = "Unity.PerformanceTesting";

        private readonly IEditorAssembliesProxy m_EditorAssembliesProxy;
        private readonly ScriptAssembly[] m_AllEditorScriptAssemblies;
        private readonly PrecompiledAssembly[] m_AllPrecompiledAssemblies;

        public EditorLoadedTestAssemblyProvider(IEditorCompilationInterfaceProxy compilationInterfaceProxy, IEditorAssembliesProxy editorAssembliesProxy)
        {
            m_EditorAssembliesProxy = editorAssembliesProxy;
            m_AllEditorScriptAssemblies = compilationInterfaceProxy.GetAllEditorScriptAssemblies();
            m_AllPrecompiledAssemblies = compilationInterfaceProxy.GetAllPrecompiledAssemblies();
        }

        public List<IAssemblyWrapper> GetAssembliesGroupedByType(TestPlatform mode)
        {
            var assemblies = GetAssembliesGroupedByTypeAsync(mode);
            while (assemblies.MoveNext())
            {
            }

            return assemblies.Current.Where(pair => mode.IsFlagIncluded(pair.Key)).SelectMany(pair => pair.Value).ToList();
        }

        public IEnumerator<IDictionary<TestPlatform, List<IAssemblyWrapper>>> GetAssembliesGroupedByTypeAsync(TestPlatform mode)
        {
            var loadedAssemblies = m_EditorAssembliesProxy.loadedAssemblies.ToDictionary(p => p.Name, p => p);

            IDictionary<TestPlatform, List<IAssemblyWrapper>> result = new Dictionary<TestPlatform, List<IAssemblyWrapper>>
            {
                {TestPlatform.EditMode, new List<IAssemblyWrapper>() },
                {TestPlatform.PlayMode, new List<IAssemblyWrapper>() }
            };
            var filteredAssemblies = FilterAssembliesWithTestReference(loadedAssemblies);

            foreach (var loadedAssembly in filteredAssemblies)
            {
                var assemblyName = new FileInfo(loadedAssembly.Location).Name;
                var scriptAssemblies = m_AllEditorScriptAssemblies.Where(x => x.Filename == assemblyName).ToList();
                var precompiledAssemblies = m_AllPrecompiledAssemblies.Where(x => new FileInfo(x.Path).Name == assemblyName).ToList();
                if (scriptAssemblies.Count < 1 && precompiledAssemblies.Count < 1)
                {
                    continue;
                }

                var assemblyFlags = scriptAssemblies.Any() ? scriptAssemblies.Single().Flags : precompiledAssemblies.Single().Flags;
                var assemblyType = (assemblyFlags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly ? TestPlatform.EditMode : TestPlatform.PlayMode;
                result[assemblyType].Add(loadedAssembly);
                yield return null;
            }

            yield return result;
        }

        private static IAssemblyWrapper[] FilterAssembliesWithTestReference(Dictionary<string, IAssemblyWrapper> loadedAssemblies)
        {
            var filteredResults = new Dictionary<IAssemblyWrapper, bool>();
            // Reuse one dictionary instead of re-creating it for each assembly
            var resultsAlreadyAnalyzed = new Dictionary<IAssemblyWrapper, bool>();

            foreach (var assembly in loadedAssemblies.Values)
            {
                FilterAssemblyForTestReference(assembly, loadedAssemblies, filteredResults, resultsAlreadyAnalyzed);
            }

            return filteredResults.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();
        }

        private static void FilterAssemblyForTestReference(IAssemblyWrapper assemblyToFilter, Dictionary<string, IAssemblyWrapper> loadedAssemblies,
            IDictionary<IAssemblyWrapper, bool> filterResults, IDictionary<IAssemblyWrapper, bool> resultsAlreadyAnalyzed)
        {
            if(resultsAlreadyAnalyzed.TryAdd(assemblyToFilter, true) is false)
                return;

            var references = assemblyToFilter.GetReferencedAssemblies();
            if (AnyTestReference(references))
            {
                filterResults[assemblyToFilter] = true;
                return;
            }

            foreach (var reference in references)
            {
                var referencedAssembly = loadedAssemblies.GetValueOrDefault(reference);
                if (referencedAssembly == null)
                {
                    continue;
                }

                FilterAssemblyForTestReference(referencedAssembly, loadedAssemblies, filterResults, resultsAlreadyAnalyzed);

                if (filterResults.ContainsKey(referencedAssembly) && filterResults[referencedAssembly])
                {
                    filterResults[assemblyToFilter] = true;
                    return;
                }
            }

            filterResults[assemblyToFilter] = false;
        }

        private static readonly HashSet<string> s_TestReferences = new()
        {
            k_NunitAssemblyName,
            k_TestRunnerAssemblyName,
            k_PerformanceTestingAssemblyName
        };

        private static bool AnyTestReference(string[] references)
        {
            // Avoid linq to reduce allocations.
            foreach (var reference in references)
            {
                if (s_TestReferences.Contains(reference))
                    return true;
            }
            return false;
        }
    }
}
