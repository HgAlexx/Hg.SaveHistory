using System.IO;
using System.Linq;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Managers;
using Hg.SaveHistory.Types;
using NUnit.Framework;

namespace Tests.Scripts
{
    public class EngineScriptTests
    {
        #region Members

        protected void CreateProfile(DataSet dataSet)
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

            if (luaManager.ActiveEngine.ReadMe != null)
            {
                Assert.DoesNotThrow(() =>
                {
                    string s = luaManager.ActiveEngine.ReadMe.Call().First() as string;
                    Assert.IsFalse(string.IsNullOrEmpty(s));
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

        protected void OpenProfile(DataSet dataSet)
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

        #endregion
    }
}