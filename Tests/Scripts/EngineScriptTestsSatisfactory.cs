using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Managers;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;
using NUnit.Framework;
using Logger = Hg.SaveHistory.Utilities.Logger;

namespace Tests.Scripts
{
    /// <summary>
    ///     Functional tests (using unit test framework for conveniences)
    /// </summary>
    [TestFixture]
    public class EngineScriptTestsSatisfactory : EngineScriptTests
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
            string[] paths = {@"Satisfactory"};

            foreach (var p in paths)
            {
                string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data", p);

                Directory.Delete(path, true);
            }
        }

        /// <summary>
        ///     Remove bits after milliseconds
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime RoundSavedAt(DateTime dt)
        {
            var d = dt.AddMilliseconds(-dt.Millisecond);
            return d;
        }

        [TestCaseSource(nameof(DataSets))]
        public void OpenProfile(DataSetSatisfactory dataSetSatisfactory)
        {
            base.OpenProfile(dataSetSatisfactory);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CreateProfile(DataSetSatisfactory dataSetSatisfactory)
        {
            base.CreateProfile(dataSetSatisfactory);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CheckBackupRestore(DataSetSatisfactory dataSetSatisfactory)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSetSatisfactory.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.That(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.That(engineScript != null, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSetSatisfactory.SourceFolder))
            {
                Directory.CreateDirectory(dataSetSatisfactory.SourceFolder);
            }

            string profilePathOrigin =
                Path.Combine(dataSetSatisfactory.DataRoot, @"Original", dataSetSatisfactory.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSetSatisfactory.SourceFolder, dataSetSatisfactory.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSetSatisfactory.SourceFolder.Replace(@"\", @"\\"));
            File.WriteAllText(profilePathSimulation, content);

            ProfileFile profileFile = ProfileFile.Load(profilePathSimulation);
            Assert.That(profileFile != null);

            LuaManager luaManager = new LuaManager();

            bool loadEngine = luaManager.LoadEngineAndProfile(engineScript, profileFile);
            Assert.That(loadEngine);

            if (luaManager.ActiveEngine.OnOpened != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnOpened.Call(); });
            }

            if (luaManager.ActiveEngine.OnInitialized != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnInitialized.Call(); });
            }

            var setting = luaManager.ActiveEngine.SettingByName("IncludeAutosave");
            if (setting is EngineSettingCheckbox s)
            {
                s.Value = true;
            }

            // End of open

            Directory.CreateDirectory(dataSetSatisfactory.SourceFolder);
            Thread.Sleep(100);

            string fileSource = Path.Combine(dataSetSatisfactory.DataRoot, @"Original");
            string fileDest = dataSetSatisfactory.SourceFolder;

            foreach (var files in dataSetSatisfactory.Watchers.OrderBy(x => x.Key))
            {
                foreach (var file in files.Value.Files)
                {
                    File.Copy(
                        Path.Combine(fileSource, file),
                        Path.Combine(fileDest, file));
                }

                Thread.Sleep(100);

                Assert.DoesNotThrow(() =>
                {
                    luaManager.ActiveEngine.ActionSnapshotBackup(ActionSource.HotKey, files.Value.SnapshotAutosave != "");
                });

                Assert.That(1 == luaManager.ActiveEngine.Snapshots.Count);

                EngineSnapshot snapshot = luaManager.ActiveEngine.Snapshots.First();
                Assert.That(snapshot == luaManager.ActiveEngine.LastSnapshot);

                Assert.That(files.Value.SnapshotSaveAt == RoundSavedAt(snapshot.SavedAt));
                Assert.That(files.Value.SnapshotBuildVersion == snapshot.CustomValueByKey("BuildVersion").ToString());
                Assert.That(files.Value.SnapshotSaveVersion == snapshot.CustomValueByKey("SaveVersion").ToString());
                Assert.That(files.Value.SnapshotSessionName == snapshot.CustomValueByKey("SessionName").ToString());
                Assert.That(files.Value.SnapshotPlayedTime == snapshot.CustomValueByKey("PlayedTime").ToString());
                Assert.That(files.Value.SnapshotAutosave == snapshot.CustomValueByKey("Autosave").ToString());

                Assert.DoesNotThrow(() =>
                {
                    luaManager.ActiveEngine.ActionSnapshotRestore(ActionSource.HotKey, luaManager.ActiveEngine.LastSnapshot);
                });

                luaManager.ActiveEngine.Snapshots.Clear();

                foreach (var file in files.Value.Files)
                {
                    File.Delete(Path.Combine(fileDest, file));
                }

                Thread.Sleep(100);
            }

            luaManager.Release();

            Directory.Delete(dataSetSatisfactory.SourceFolder, true);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CheckWatcher(DataSetSatisfactory dataSetSatisfactory)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSetSatisfactory.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.That(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.That(engineScript != null, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSetSatisfactory.SourceFolder))
            {
                Directory.CreateDirectory(dataSetSatisfactory.SourceFolder);
            }

            string profilePathOrigin =
                Path.Combine(dataSetSatisfactory.DataRoot, @"Original", dataSetSatisfactory.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSetSatisfactory.SourceFolder, dataSetSatisfactory.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSetSatisfactory.SourceFolder.Replace(@"\", @"\\"));
            File.WriteAllText(profilePathSimulation, content);

            ProfileFile profileFile = ProfileFile.Load(profilePathSimulation);
            Assert.That(profileFile != null);

            LuaManager luaManager = new LuaManager();

            bool loadEngine = luaManager.LoadEngineAndProfile(engineScript, profileFile);
            Assert.That(loadEngine);

            if (luaManager.ActiveEngine.OnOpened != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnOpened.Call(); });
            }

            int catCount = 0;
            luaManager.ActiveEngine.OnCategoriesChanges += () => { catCount++; };

            int snapCount = 0;
            luaManager.ActiveEngine.OnSnapshotsChanges += () => { snapCount++; };

            int backupCount = 0;
            luaManager.ActiveEngine.OnAutoBackupOccurred += success =>
            {
                if (success)
                {
                    backupCount++;
                }
            };

            if (luaManager.ActiveEngine.OnInitialized != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnInitialized.Call(); });
            }

            Assert.That(luaManager.ActiveEngine.Watcher != null);

            var setting = luaManager.ActiveEngine.SettingByName("IncludeAutosave");
            if (setting is EngineSettingCheckbox s)
            {
                s.Value = true;
            }

            WatcherManager watcherManager = new WatcherManager(luaManager.ActiveEngine.Watcher)
            {
                IsProcessRunning = () => true
            };

            // int backupStatus = 0;
            watcherManager.AutoBackupStatusChanged += () =>
            {
                // backupStatus++;
            };

            if (luaManager.ActiveEngine.OnLoaded != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnLoaded.Call(); });
            }

            Assert.That(1 == catCount);
            Assert.That(1 == snapCount);

            // End of open

            Assert.That(AutoBackupStatus.Disabled == watcherManager.AutoBackupStatus);

            Thread.Sleep(100);

            watcherManager.SetAutoBackup(true);

            Assert.That(AutoBackupStatus.Enabled == watcherManager.AutoBackupStatus);

            string fileSource = Path.Combine(dataSetSatisfactory.DataRoot, @"Original");
            string fileDest = dataSetSatisfactory.SourceFolder;

            foreach (var files in dataSetSatisfactory.Watchers.OrderBy(x => x.Key))
            {
                backupCount = 0;

                foreach (var file in files.Value.Files)
                {
                    // this engine requires file changes
                    //using (var fileStream = File.Create(Path.Combine(fileDest, file)))
                    //{ }

                    //Thread.Sleep(100);

                    using (var fileStream = File.OpenWrite(Path.Combine(fileDest, file)))
                    {
                        var bytes = File.ReadAllBytes(Path.Combine(fileSource, file));
                        fileStream.Write(bytes, 0, bytes.Length);
                    }

                    Thread.Sleep(100);
                }

                Thread.Sleep(500);

                Assert.That(backupCount >= 1);
                Assert.That(1 == luaManager.ActiveEngine.Snapshots.Count);

                EngineSnapshot snapshot = luaManager.ActiveEngine.Snapshots.First();
                Assert.That(snapshot == luaManager.ActiveEngine.LastSnapshot);

                Assert.That(files.Value.SnapshotSaveAt == RoundSavedAt(snapshot.SavedAt));
                Assert.That(files.Value.SnapshotBuildVersion == snapshot.CustomValueByKey("BuildVersion").ToString());
                Assert.That(files.Value.SnapshotSaveVersion == snapshot.CustomValueByKey("SaveVersion").ToString());
                Assert.That(files.Value.SnapshotSessionName == snapshot.CustomValueByKey("SessionName").ToString());
                Assert.That(files.Value.SnapshotPlayedTime == snapshot.CustomValueByKey("PlayedTime").ToString());
                Assert.That(files.Value.SnapshotAutosave == snapshot.CustomValueByKey("Autosave").ToString());

                luaManager.ActiveEngine.Snapshots.Clear();

                foreach (var file in files.Value.Files)
                {
                    File.Delete(Path.Combine(fileDest, file));
                }

                Thread.Sleep(100);
            }

            watcherManager.Release();

            luaManager.Release();

            Directory.Delete(dataSetSatisfactory.SourceFolder, true);
        }


        public static IEnumerable<DataSetSatisfactory> DataSets()
        {
            return DataSetSatisfactory.DataSets();
        }
    }
}