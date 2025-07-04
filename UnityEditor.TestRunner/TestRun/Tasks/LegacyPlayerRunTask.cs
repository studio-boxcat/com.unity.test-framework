using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class LegacyPlayerRunTask : TestTaskBase
    {
        public LegacyPlayerRunTask()
        {
            SupportsResumingEnumerator = true;
        }

        public override string GetName()
        {
            return "Build Test Player";
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            yield return null; // Allow for setting the test job data after a resume.
            var executionSettings = testJobData.executionSettings;
            var launcher = new PlayerLauncher(testJobData.PlayModeSettings, executionSettings.targetPlatform, executionSettings.overloadTestRunSettings, executionSettings.playerHeartbeatTimeout, testJobData.InitTestScene, testJobData.PlaymodeTestsController);
            launcher.Run();
            testJobData.PlayerBuildOptions = launcher.playerBuildOptions.BuildPlayerOptions; // This can be removed once the player build options are created in a separate task

            yield return null;
        }
    }
}
