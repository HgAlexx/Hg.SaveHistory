using System.Collections.Generic;
using System.IO;
using Hg.SaveHistory.Utilities;
using NUnit.Framework;

namespace Tests.Scripts
{
    [SetUpFixture]
    public class EngineScriptSetupTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Logger.Level = LogLevel.None;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Delete all scripts data folders
            string[] paths = new[] {@"DOOM2016", @"DOOMEternal"};

            foreach (var p in paths)
            {
                string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data", p);

                Directory.Delete(path, true);
            }
        }
    }
}