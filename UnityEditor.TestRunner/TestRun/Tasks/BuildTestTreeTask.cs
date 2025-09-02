using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class BuildTestTreeTask : TestTaskBase
    {
        private TestPlatform m_TestPlatform;

        public BuildTestTreeTask(TestPlatform testPlatform)
        {
            m_TestPlatform = testPlatform;
            RerunAfterResume = true;
        }

        internal EditorLoadedTestAssemblyProvider m_testAssemblyProvider = EditorLoadedTestAssemblyProvider.Instance;
        internal Func<string[], int, IAsyncTestAssemblyBuilder> m_testAssemblyBuilderFactory = (orderedTestNames, seed) => new UnityTestAssemblyBuilder(orderedTestNames, seed);
        internal ICallbacksDelegator m_CallbacksDelegator = CallbacksDelegator.instance;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree != null)
            {
                yield break;
            }

            var assembliesEnumerator = m_testAssemblyProvider.GetAssembliesGroupedByType(m_TestPlatform);
            var assemblies = assembliesEnumerator.ToArray();
            var buildSettings = UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(m_TestPlatform);
            var testAssemblyBuilder = m_testAssemblyBuilderFactory(testJobData.executionSettings.orderedTestNames, testJobData.executionSettings.randomOrderSeed);
            var enumerator = testAssemblyBuilder.BuildAsync(assemblies, m_TestPlatform, buildSettings);
            while (enumerator.MoveNext())
            {
                yield return null;
            }

            var testList = enumerator.Current;
            if (testList== null)
            {
                throw new Exception("Test list not retrieved.");
            }

            testJobData.testTree = testList;
            m_CallbacksDelegator.TestTreeRebuild(testList);
        }
    }
}
