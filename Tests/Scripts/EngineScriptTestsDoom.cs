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
    public class EngineScriptTestsDoom : EngineScriptTests
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
            string[] paths = {@"DOOM2016", @"DOOMEternal"};

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
            TimeSpan d = new TimeSpan(0, 0, 0, 0, 1);
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        [TestCaseSource(nameof(DataSets))]
        public void OpenProfile(DataSetDoom dataSetDoom)
        {
            base.OpenProfile(dataSetDoom);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CreateProfile(DataSetDoom dataSetDoom)
        {
            base.CreateProfile(dataSetDoom);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CheckWatcher(DataSetDoom dataSetDoom)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSetDoom.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSetDoom.SourceFolder))
            {
                Directory.CreateDirectory(dataSetDoom.SourceFolder);
            }

            string profilePathOrigin = Path.Combine(dataSetDoom.DataRoot, @"Original", dataSetDoom.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSetDoom.SourceFolder, dataSetDoom.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSetDoom.SourceFolder.Replace(@"\", @"\\"));
            File.WriteAllText(profilePathSimulation, content);

            ProfileFile profileFile = ProfileFile.Load(profilePathSimulation);
            Assert.IsNotNull(profileFile);

            LuaManager luaManager = new LuaManager();
            Assert.IsNotNull(luaManager);

            bool loadEngine = luaManager.LoadEngineAndProfile(engineScript, profileFile);
            Assert.IsTrue(loadEngine);

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

            Assert.IsNotNull(luaManager.ActiveEngine.Watcher);

            var setting = luaManager.ActiveEngine.SettingByName("IncludeDeath");
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

            Assert.AreEqual(1, catCount);
            Assert.AreEqual(1, snapCount);

            // End of open

            Assert.AreEqual(AutoBackupStatus.Disabled, watcherManager.AutoBackupStatus);

            watcherManager.SetAutoBackup(true);
            Assert.AreEqual(AutoBackupStatus.Waiting, watcherManager.AutoBackupStatus);

            Directory.CreateDirectory(Path.Combine(dataSetDoom.SourceFolder, @"GAME-AUTOSAVE0"));
            Thread.Sleep(100);
            Assert.AreEqual(AutoBackupStatus.Enabled, watcherManager.AutoBackupStatus);

            string fileSource = Path.Combine(dataSetDoom.DataRoot, @"Original", @"GAME-AUTOSAVE0");
            string fileDest = Path.Combine(dataSetDoom.SourceFolder, @"GAME-AUTOSAVE0");

            foreach (var files in dataSetDoom.Watchers.OrderBy(x => x.Key))
            {
                backupCount = 0;

                foreach (var file in files.Value.Files)
                {
                    File.Copy(
                        Path.Combine(fileSource, file),
                        Path.Combine(fileDest, file + ".temp"));
                }

                foreach (var file in files.Value.Files)
                {
                    File.Move(
                        Path.Combine(fileDest, file + ".temp"),
                        Path.Combine(fileDest, file));
                    Thread.Sleep(100);
                }

                Thread.Sleep(500);

                Assert.AreEqual(1, backupCount);
                Assert.AreEqual(1, luaManager.ActiveEngine.Snapshots.Count);

                EngineSnapshot snapshot = luaManager.ActiveEngine.Snapshots.First();
                Assert.AreSame(snapshot, luaManager.ActiveEngine.LastSnapshot);

                Assert.AreEqual(files.Value.SnapshotSaveAt, RoundSavedAt(snapshot.SavedAt));
                Assert.AreEqual(files.Value.SnapshotDifficulty, snapshot.CustomValueByKey("Difficulty").ToString());
                Assert.AreEqual(files.Value.SnapshotMapDesc, snapshot.CustomValueByKey("MapDesc").ToString());
                Assert.AreEqual(files.Value.SnapshotMapName, snapshot.CustomValueByKey("MapName").ToString());
                Assert.AreEqual(files.Value.SnapshotPlayedTime, snapshot.CustomValueByKey("PlayedTime").ToString());
                Assert.AreEqual(files.Value.SnapshotDeath, snapshot.CustomValueByKey("Death").ToString());

                luaManager.ActiveEngine.Snapshots.Clear();

                foreach (var file in files.Value.Files)
                {
                    File.Delete(Path.Combine(fileDest, file));
                }

                Thread.Sleep(100);
            }

            watcherManager.Release();

            luaManager.Release();

            Directory.Delete(dataSetDoom.SourceFolder, true);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CheckBackupRestore(DataSetDoom dataSetDoom)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSetDoom.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSetDoom.SourceFolder))
            {
                Directory.CreateDirectory(dataSetDoom.SourceFolder);
            }

            string profilePathOrigin = Path.Combine(dataSetDoom.DataRoot, @"Original", dataSetDoom.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSetDoom.SourceFolder, dataSetDoom.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSetDoom.SourceFolder.Replace(@"\", @"\\"));
            File.WriteAllText(profilePathSimulation, content);

            ProfileFile profileFile = ProfileFile.Load(profilePathSimulation);
            Assert.IsNotNull(profileFile);

            LuaManager luaManager = new LuaManager();
            Assert.IsNotNull(luaManager);

            bool loadEngine = luaManager.LoadEngineAndProfile(engineScript, profileFile);
            Assert.IsTrue(loadEngine);

            if (luaManager.ActiveEngine.OnOpened != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnOpened.Call(); });
            }

            if (luaManager.ActiveEngine.OnInitialized != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnInitialized.Call(); });
            }

            var setting = luaManager.ActiveEngine.SettingByName("IncludeDeath");
            if (setting is EngineSettingCheckbox s)
            {
                s.Value = true;
            }

            // End of open

            Directory.CreateDirectory(Path.Combine(dataSetDoom.SourceFolder, @"GAME-AUTOSAVE0"));
            Thread.Sleep(100);

            string fileSource = Path.Combine(dataSetDoom.DataRoot, @"Original", @"GAME-AUTOSAVE0");
            string fileDest = Path.Combine(dataSetDoom.SourceFolder, @"GAME-AUTOSAVE0");

            foreach (var files in dataSetDoom.Watchers.OrderBy(x => x.Key))
            {
                foreach (var file in files.Value.Files)
                {
                    File.Copy(
                        Path.Combine(fileSource, file),
                        Path.Combine(fileDest, file + ".temp"));
                }

                foreach (var file in files.Value.Files)
                {
                    File.Move(
                        Path.Combine(fileDest, file + ".temp"),
                        Path.Combine(fileDest, file));
                }

                Thread.Sleep(100);

                Assert.DoesNotThrow(() =>
                {
                    luaManager.ActiveEngine.ActionSnapshotBackup(ActionSource.HotKey, files.Value.SnapshotDeath != "");
                });

                Assert.AreEqual(1, luaManager.ActiveEngine.Snapshots.Count);

                EngineSnapshot snapshot = luaManager.ActiveEngine.Snapshots.First();
                Assert.AreSame(snapshot, luaManager.ActiveEngine.LastSnapshot);

                Assert.AreEqual(files.Value.SnapshotSaveAt, RoundSavedAt(snapshot.SavedAt));
                Assert.AreEqual(files.Value.SnapshotDifficulty, snapshot.CustomValueByKey("Difficulty").ToString());
                Assert.AreEqual(files.Value.SnapshotMapDesc, snapshot.CustomValueByKey("MapDesc").ToString());
                Assert.AreEqual(files.Value.SnapshotMapName, snapshot.CustomValueByKey("MapName").ToString());
                Assert.AreEqual(files.Value.SnapshotPlayedTime, snapshot.CustomValueByKey("PlayedTime").ToString());
                Assert.AreEqual(files.Value.SnapshotDeath, snapshot.CustomValueByKey("Death").ToString());

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

            Directory.Delete(dataSetDoom.SourceFolder, true);
        }

        public static IEnumerable<DataSetDoom> DataSets()
        {
            return DataSetDoom.DataSets();
        }
    }
}