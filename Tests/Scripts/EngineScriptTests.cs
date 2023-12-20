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
            Assert.That(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.That(engineScript != null, "EngineScript not loaded properly");

            Assert.That(dataSet.Name == engineScript.Name);
            Assert.That(dataSet.Title == engineScript.Title);
            Assert.That(dataSet.Author == engineScript.Author);

            Assert.That(dataSet.FileCount == engineScript.Files.Count);

            Assert.That(engineScript.IsValid());
            Assert.That(!engineScript.IsAltered(true));
            Assert.That(engineScript.Official);

            LuaManager luaManager = new LuaManager();

            bool loadEngine = luaManager.LoadEngine(engineScript);
            Assert.That(loadEngine);

            if (!Directory.Exists(dataSet.SourceFolder))
            {
                Directory.CreateDirectory(dataSet.SourceFolder);
            }

            foreach (var setting in luaManager.ActiveEngine.Settings.Where(s => s.Kind == EngineSettingKind.Setup).OrderBy(s => -s.Index))
            {
                Assert.That(dataSet.Settings.ContainsKey(setting.Name));
                if (setting is EngineSettingCombobox settingCombobox)
                {
                    settingCombobox.Value = (int) dataSet.Settings[setting.Name];
                }

                if (setting is EngineSettingFolderBrowser settingFolder)
                {
                    Assert.That(settingFolder.CanAutoDetect == dataSet.CanAutoDetect);
                    if (dataSet.CanAutoDetect)
                    {
                        Assert.That(settingFolder.OnAutoDetect != null);

                        Assert.DoesNotThrow(() =>
                        {
                            string s = settingFolder.OnAutoDetect?.Call().FirstOrDefault() as string;
                            Assert.That(s != null);
                        });
                    }

                    settingFolder.Value = (string) dataSet.Settings[setting.Name];
                }
            }

            Assert.That(luaManager.ActiveEngine.OnSetupValidate != null);
            Assert.DoesNotThrow(() =>
            {
                bool? b = luaManager.ActiveEngine.OnSetupValidate.Call().First() as bool?;
                Assert.That(b != null);
                Assert.That(b != null && b.Value);
            });

            if (luaManager.ActiveEngine.OnSetupSuggestProfileName != null)
            {
                Assert.DoesNotThrow(() =>
                {
                    string s = luaManager.ActiveEngine.OnSetupSuggestProfileName.Call().First() as string;
                    Assert.That(!string.IsNullOrEmpty(s));
                    Assert.That(HgUtility.IsValidFileName(s));
                    Assert.That(dataSet.SuggestProfileName == s);
                });
            }

            if (luaManager.ActiveEngine.ReadMe != null)
            {
                Assert.DoesNotThrow(() =>
                {
                    string s = luaManager.ActiveEngine.ReadMe.Call().First() as string;
                    Assert.That(!string.IsNullOrEmpty(s));
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

            Assert.That(expected == produced);

            luaManager.Release();

            Directory.Delete(dataSet.SourceFolder, true);
        }

        protected void OpenProfile(DataSet dataSet)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Scripts", dataSet.Name);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Assert.That(directoryInfo != null, "Scripts directory is null");
            Assert.That(directoryInfo.Exists, "Scripts directory does not exist");

            EngineScript engineScript = EngineScriptManager.LoadEngineScript(directoryInfo);
            Assert.That(engineScript != null, "EngineScript not loaded properly");

            Assert.That(dataSet.Name == engineScript.Name);
            Assert.That(dataSet.Title == engineScript.Title);
            Assert.That(dataSet.Author == engineScript.Author);

            Assert.That(dataSet.FileCount == engineScript.Files.Count);

            Assert.That(engineScript.IsValid());
            Assert.That(!engineScript.IsAltered(true));
            Assert.That(engineScript.Official);

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


            if (luaManager.ActiveEngine.OnInitialized != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnInitialized.Call(); });
            }


            if (luaManager.ActiveEngine.OnLoaded != null)
            {
                Assert.DoesNotThrow(() => { luaManager.ActiveEngine.OnLoaded.Call(); });
            }

            Assert.That(1 == catCount);
            Assert.That(1 == snapCount);

            // End of open

            luaManager.SaveSettings(profileFile);
            luaManager.SaveSnapshots(profileFile);
            ProfileFile.Save(profileFile);

            Assert.DoesNotThrow(() => { profileFile.Release(); });

            string expected = File.ReadAllText(profilePathOrigin);
            expected = expected.Replace(@"%SOURCEFOLDER%", dataSet.SourceFolder.Replace(@"\", @"\\"));

            string produced = File.ReadAllText(profilePathSimulation);

            Assert.That(expected == produced);

            luaManager.Release();

            Directory.Delete(dataSet.SourceFolder, true);
        }

        #endregion
    }
}