using System;
using UnityEditor.TestRunner.CommandLineParser;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class SettingsBuilder : ISettingsBuilder
    {
        private Action<string> m_LogWarningAction;
        private Func<bool> m_ScriptCompilationFailedCheck;

        public SettingsBuilder(Action<string> logWarningAction, Func<bool> scriptCompilationFailedCheck)
        {
            m_LogWarningAction = logWarningAction;
            m_ScriptCompilationFailedCheck = scriptCompilationFailedCheck;
        }

        public Api.ExecutionSettings BuildApiExecutionSettings(string[] commandLineArgs)
        {
            var quit = false;
            string testPlatform = TestMode.EditMode.ToString();
            string[] testFilters = null;

            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("quit", () => { quit = true; }),
                new CommandLineOption("testPlatform", platform => { testPlatform = platform; }),
                new CommandLineOption("testFilter", filters => { testFilters = filters; })
            );
            optionSet.Parse(commandLineArgs);

            DisplayQuitWarningIfQuitIsGiven(quit);

            CheckForScriptCompilationErrors();

            var filter = new Filter
            {
                testMode = testPlatform.ToLower() == "editmode" ? TestMode.EditMode : TestMode.PlayMode,
                groupNames = testFilters,
            };

            return new Api.ExecutionSettings
            {
                filters = new[] { filter },
                targetPlatform = null,
            };
        }

        public ExecutionSettings BuildExecutionSettings(string[] commandLineArgs)
        {
            string resultFilePath = null;

            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("testResults", filePath => { resultFilePath = filePath; })
            );
            optionSet.Parse(commandLineArgs);

            return new ExecutionSettings
            {
                TestResultsFile = resultFilePath,
            };
        }

        private void DisplayQuitWarningIfQuitIsGiven(bool quitIsGiven)
        {
            if (quitIsGiven)
            {
                m_LogWarningAction("Running tests from command line arguments will not work when \"quit\" is specified.");
            }
        }

        private void CheckForScriptCompilationErrors()
        {
            if (m_ScriptCompilationFailedCheck())
            {
                throw new SetupException(SetupException.ExceptionType.ScriptCompilationFailed);
            }
        }
    }
}
