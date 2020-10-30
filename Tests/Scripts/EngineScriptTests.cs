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
    public class EngineScriptTests
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


        [TestCaseSource(nameof(DataSets))]
        public void CheckWatcher(DataSet dataSet)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSet.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSet.SourceFolder))
            {
                Directory.CreateDirectory(dataSet.SourceFolder);
            }

            string profilePathOrigin = Path.Combine(dataSet.DataRoot, @"Original", dataSet.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSet.SourceFolder, dataSet.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));
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

            Directory.CreateDirectory(Path.Combine(dataSet.SourceFolder, @"GAME-AUTOSAVE0"));
            Thread.Sleep(100);
            Assert.AreEqual(AutoBackupStatus.Enabled, watcherManager.AutoBackupStatus);

            string fileSource = Path.Combine(dataSet.DataRoot, @"Original", @"GAME-AUTOSAVE0");
            string fileDest = Path.Combine(dataSet.SourceFolder, @"GAME-AUTOSAVE0");

            foreach (var files in dataSet.Watchers.OrderBy(x => x.Key))
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

            Directory.Delete(dataSet.SourceFolder, true);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CheckBackupRestore(DataSet dataSet)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSet.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            if (!Directory.Exists(dataSet.SourceFolder))
            {
                Directory.CreateDirectory(dataSet.SourceFolder);
            }

            string profilePathOrigin = Path.Combine(dataSet.DataRoot, @"Original", dataSet.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSet.SourceFolder, dataSet.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));
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

            Directory.CreateDirectory(Path.Combine(dataSet.SourceFolder, @"GAME-AUTOSAVE0"));
            Thread.Sleep(100);

            string fileSource = Path.Combine(dataSet.DataRoot, @"Original", @"GAME-AUTOSAVE0");
            string fileDest = Path.Combine(dataSet.SourceFolder, @"GAME-AUTOSAVE0");

            foreach (var files in dataSet.Watchers.OrderBy(x => x.Key))
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
                    luaManager.ActiveEngine.ActionSnapshotBackup(ActionSource.HotKey, (files.Value.SnapshotDeath != ""));
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

            Directory.Delete(dataSet.SourceFolder, true);
        }


        [TestCaseSource(nameof(DataSets))]
        public void OpenProfile(DataSet dataSet)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSet.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            Assert.AreEqual(dataSet.Name, engineScript.Name);
            Assert.AreEqual(dataSet.Title, engineScript.Title);
            Assert.AreEqual(dataSet.Author, engineScript.Author);

            Assert.AreEqual(dataSet.FileCount, engineScript.Files.Count);

            Assert.IsTrue(engineScript.IsValid());
            Assert.IsFalse(engineScript.IsAltered(true));
            Assert.IsTrue(engineScript.Official);

            if (!Directory.Exists(dataSet.SourceFolder))
            {
                Directory.CreateDirectory(dataSet.SourceFolder);
            }

            string profilePathOrigin = Path.Combine(dataSet.DataRoot, @"Original", dataSet.ProfileName + "_Open.shp");
            string profilePathSimulation = Path.Combine(dataSet.SourceFolder, dataSet.ProfileName + "_Open.shp");

            File.Copy(profilePathOrigin, profilePathSimulation);

            string content = File.ReadAllText(profilePathSimulation);
            content = content.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));
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


            if (luaManager.ActiveEngine.OnInitialized != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnInitialized.Call(); });
            }


            if (luaManager.ActiveEngine.OnLoaded != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnLoaded.Call(); });
            }

            Assert.AreEqual(1, catCount);
            Assert.AreEqual(1, snapCount);

            // End of open

            luaManager.SaveSettings(profileFile);
            luaManager.SaveSnapshots(profileFile);
            ProfileFile.Save(profileFile);

            Assert.DoesNotThrow(() => { profileFile.Release(); });

            string expected = File.ReadAllText(profilePathOrigin);
            expected = expected.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));

            string produced = File.ReadAllText(profilePathSimulation);

            Assert.AreEqual(expected, produced);

            luaManager.Release();

            Directory.Delete(dataSet.SourceFolder, true);
        }

        [TestCaseSource(nameof(DataSets))]
        public void CreateProfile(DataSet dataSet)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSet.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.IsNotNull(directoryInfo, "Scripts directory is null");
            Assert.IsTrue(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.IsNotNull(engineScript, "EngineScript not loaded properly");

            Assert.AreEqual(dataSet.Name, engineScript.Name);
            Assert.AreEqual(dataSet.Title, engineScript.Title);
            Assert.AreEqual(dataSet.Author, engineScript.Author);

            Assert.AreEqual(dataSet.FileCount, engineScript.Files.Count);

            Assert.IsTrue(engineScript.IsValid());
            Assert.IsFalse(engineScript.IsAltered(true));
            Assert.IsTrue(engineScript.Official);

            LuaManager luaManager = new LuaManager();

            bool loadEngine = luaManager.LoadEngine(engineScript);
            Assert.IsTrue(loadEngine);

            if (!Directory.Exists(dataSet.SourceFolder))
            {
                Directory.CreateDirectory(dataSet.SourceFolder);
            }

            foreach (var setting in luaManager.ActiveEngine.Settings.Where(s => s.Kind == EngineSettingKind.Setup).OrderBy(s => -s.Index))
            {
                Assert.IsTrue(dataSet.Settings.ContainsKey(setting.Name));
                if (setting is EngineSettingCombobox settingCombobox)
                {
                    settingCombobox.Value = (int) dataSet.Settings[setting.Name];
                }

                if (setting is EngineSettingFolderBrowser settingFolder)
                {
                    Assert.IsTrue(settingFolder.CanAutoDetect == dataSet.CanAutoDetect);
                    if (dataSet.CanAutoDetect)
                    {
                        Assert.IsNotNull(settingFolder.OnAutoDetect);

                        Assert.DoesNotThrow(() =>
                        {
                            string s = settingFolder.OnAutoDetect?.Call().FirstOrDefault() as string;
                            Assert.IsNotNull(s);
                        });
                    }

                    settingFolder.Value = (string) dataSet.Settings[setting.Name];
                }
            }

            Assert.IsNotNull(luaManager.ActiveEngine.OnSetupValidate);
            Assert.DoesNotThrow(() =>
            {
                bool? b = luaManager.ActiveEngine.OnSetupValidate.Call().First() as bool?;
                Assert.IsNotNull(b);
                Assert.IsTrue(b.Value);
            });

            if (luaManager.ActiveEngine.OnSetupSuggestProfileName != null)
            {
                Assert.DoesNotThrow(() =>
                {
                    string s = luaManager.ActiveEngine.OnSetupSuggestProfileName.Call().First() as string;
                    Assert.IsFalse(string.IsNullOrEmpty(s));
                    Assert.IsTrue(HgUtility.IsValidFileName(s));
                    Assert.AreEqual(dataSet.SuggestProfileName, s);
                });
            }

            var profileFile = new ProfileFile {EngineScriptName = engineScript.Name, Name = dataSet.ProfileName};

            luaManager.SaveSnapshots(profileFile);
            luaManager.SaveSettings(profileFile);

            string filePath = Path.Combine(dataSet.SourceFolder, dataSet.ProfileName + "_Create.shp");

            profileFile.FilePath = filePath;
            ProfileFile.Save(profileFile);

            Assert.DoesNotThrow(() => { profileFile.Release(); });

            string expected = File.ReadAllText(Path.Combine(dataSet.DataRoot, "Original", dataSet.ProfileName + "_Create.shp"));
            expected = expected.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));

            string produced = File.ReadAllText(filePath);

            Assert.AreEqual(expected, produced);

            luaManager.Release();

            Directory.Delete(dataSet.SourceFolder, true);
        }

        public static IEnumerable<DataSet> DataSets()
        {
            return DataSet.DataSets();
        }

        /// <summary>
        /// Remove bits after milliseconds
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime RoundSavedAt(DateTime dt)
        {
            TimeSpan d = new TimeSpan(0, 0, 0, 0, 1);
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }
    }

    public class DataSetWatcher
    {
        public List<string> Files;
        public string SnapshotDeath { get; set; }
        public string SnapshotDifficulty { get; set; }
        public string SnapshotMapDesc { get; set; }
        public string SnapshotMapName { get; set; }
        public string SnapshotPlayedTime { get; set; }
        public DateTime SnapshotSaveAt { get; set; }
    }

    public class DataSet
    {
        #region Fields & Properties

        public string Author { get; set; }
        public bool CanAutoDetect { get; set; }

        public string DataRoot { get; set; }
        public int FileCount { get; set; }
        public string Name { get; set; }
        public string ProfileName { get; set; }
        public Dictionary<string, object> Settings { get; set; }

        public string SourceFolder { get; set; }
        public string SuggestProfileName { get; set; }
        public string Title { get; set; }
        public Dictionary<int, DataSetWatcher> Watchers { get; set; }

        #endregion

        #region Members

        public static IEnumerable<DataSet> DataSets()
        {
            string rand = DateTime.Now.ToString("HH-mm-ss-fff");
            string data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOM2016");
            string path = Path.Combine(data, "Simulation", rand);

            var set = new DataSet
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOM2016",
                Title = @"DOOM 2016",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOM2016",
                SuggestProfileName = "DOOM 2016 - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcher>()
            };

            var watcher = new DataSetWatcher()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 19, 03, 24, 03),
                SnapshotDifficulty = "I'm too young to die",
                SnapshotMapDesc = "The UAC",
                SnapshotMapName = "game/sp/intro/intro",
                SnapshotPlayedTime = "0:02:46",
                SnapshotDeath = ""

            };
            watcher.Files = new List<string>
            {
                "checkpoint.dat",
                "checkpoint_mapstart.dat",
                "game_duration.dat",
                "checkpoint_alt.dat",
                "game.details",
                "game.details.verify"
            };
            set.Watchers.Add(1, watcher);

            watcher = new DataSetWatcher()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 19, 03, 24, 03),
                SnapshotDifficulty = "I'm too young to die",
                SnapshotMapDesc = "The UAC",
                SnapshotMapName = "game/sp/intro/intro",
                SnapshotPlayedTime = "0:02:46",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game_duration.dat",
                "checkpoint_alt.dat",
                "game.details",
                "game.details.verify"
            };
            set.Watchers.Add(2, watcher);

            yield return set;


            data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOMEternal", @"Steam");
            path = Path.Combine(data, "Simulation", rand, "userdata", "13371337", "782330");
            set = new DataSet
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOMEternal",
                Title = @"DOOM Eternal",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"Platform", 1},
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOMEternal_Steam",
                SuggestProfileName = "DOOM Eternal - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcher>()
            };

            watcher = new DataSetWatcher()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 27, 01, 16, 44, 948),
                SnapshotDifficulty = "Hurt me plenty",
                SnapshotMapDesc = "Cultist Base",
                SnapshotMapName = "game/sp/e1m3_cult/e1m3_cult",
                SnapshotPlayedTime = "1:40:32",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game.details",
            };
            set.Watchers.Add(1, watcher);

            yield return set;

            data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOMEternal", @"Bethesda");
            path = Path.Combine(data, "Simulation", rand, "DOOMEternal", "base", "savegame", "d8d67efe-5bae-453a-aad5-08a71028d4dd");
            set = new DataSet
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOMEternal",
                Title = @"DOOM Eternal",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"Platform", 2},
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOMEternal_Bethesda",
                SuggestProfileName = "DOOM Eternal - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcher>()
            };

            watcher = new DataSetWatcher()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 27, 0, 44, 12, 444),
                SnapshotDifficulty = "Hurt me plenty",
                SnapshotMapDesc = "Cultist Base",
                SnapshotMapName = "game/sp/e1m3_cult/e1m3_cult",
                SnapshotPlayedTime = "1:40:32",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game.details",
            };
            set.Watchers.Add(1, watcher);

            yield return set;
        }

        #endregion
    }
}