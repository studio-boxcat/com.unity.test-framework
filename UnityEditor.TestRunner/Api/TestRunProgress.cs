namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class TestRunProgress
    {
        public string RunGuid;
        public ExecutionSettings ExecutionSettings;

        public bool HasFinished;

        public float Progress;
        public string CurrentStepName;
        public string CurrentStageName;
    }
}
