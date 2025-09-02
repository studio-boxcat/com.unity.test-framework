#nullable enable
using System.Linq;
using System.Reflection;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorLoadedTestAssemblyProvider
    {
        public static readonly EditorLoadedTestAssemblyProvider Instance = new();

        private Assembly[]? _editMode;
        private Assembly[]? _playMode;

        public Assembly[] GetAssembliesGroupedByType(TestPlatform mode)
        {
            if (_editMode is null)
            {
                var assemblies = EditorAssemblies.loadedAssemblies;
                _editMode = new[] { assemblies.First(x => x.FullName.StartsWithOrdinal("Universe.Tests.Editor,")) };
                _playMode = new[] { assemblies.First(x => x.FullName.StartsWithOrdinal("Universe.Tests.Play,")) };
            }

            return mode switch
            {
                TestPlatform.EditMode => _editMode,
                TestPlatform.PlayMode => _playMode!,
                _ => throw new System.ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
    }
}