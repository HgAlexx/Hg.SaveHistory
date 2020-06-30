using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Properties;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;
using NLua;
using NLua.Exceptions;
using Logger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.Managers
{
    public class LuaManager
    {
        #region Fields & Properties

        private Lua _lua;

        public Engine ActiveEngine { get; private set; }

        #endregion

        #region Members

        public LuaManager()
        {
            _lua = new Lua();
            _lua.State.Encoding = Encoding.UTF8;

            SetSandbox();
        }

        public bool LoadEngine(EngineScript engineScript)
        {
            return LoadEngineAndProfile(engineScript, null);
        }

        public bool LoadEngineAndProfile(EngineScript engineScript, ProfileFile profileFile)
        {
            // init Engine instance
            if (profileFile == null)
            {
                ActiveEngine = new Engine(engineScript, null);
            }
            else
            {
                string name = Path.GetFileNameWithoutExtension(profileFile.FilePath);
                ActiveEngine = new Engine(engineScript, Path.Combine(profileFile.RootFolder, name));
            }

            // Set engine variables (name, namespace)
            _lua["_engine_"] = ActiveEngine;
            _lua.NewTable("_engine_namespace_");

            foreach (EngineScriptFile backupEngineFile in engineScript.Files)
            {
                if (backupEngineFile.IsToc)
                {
                    continue;
                }

                if (!LoadFile(backupEngineFile))
                {
                    ActiveEngine = null;
                    return false;
                }
            }

            if (profileFile != null)
            {
                LoadSettings(profileFile.Settings);
                LoadSnapshots(profileFile.Snapshots);
            }

            return true;
        }

        public void Release()
        {
            ActiveEngine?.Release();
            ActiveEngine = null;

            _lua.Dispose();
            _lua = null;
        }

        public void SaveSettings(ProfileFile profileFile)
        {
            profileFile.Settings.Clear();
            foreach (var setting in ActiveEngine.Settings)
            {
                if (setting is EngineSettingCheckbox engineSettingCheckbox)
                {
                    profileFile.Settings.Add(new ProfileSettingBoolean
                        {Name = engineSettingCheckbox.Name, Value = engineSettingCheckbox.Value, Kind = engineSettingCheckbox.Kind});
                }

                if (setting is EngineSettingCombobox settingCombobox)
                {
                    profileFile.Settings.Add(new ProfileSettingInteger
                        {Name = settingCombobox.Name, Value = settingCombobox.Value, Kind = settingCombobox.Kind});
                }

                if (setting is EngineSettingFolderBrowser settingFolder)
                {
                    profileFile.Settings.Add(new ProfileSettingString
                        {Name = settingFolder.Name, Value = settingFolder.Value, Kind = settingFolder.Kind});
                }

                if (setting is EngineSettingTextbox settingTextbox)
                {
                    profileFile.Settings.Add(new ProfileSettingString
                        {Name = settingTextbox.Name, Value = settingTextbox.Value, Kind = settingTextbox.Kind});
                }
            }
        }

        public void SaveSnapshots(ProfileFile profileFile)
        {
            profileFile.Snapshots.Clear();
            profileFile.Snapshots.AddRange(ActiveEngine.Snapshots);
        }

        private string AllowedGlobals()
        {
            string[] values =
            {
                // System
                nameof(Environment),
                nameof(TimeSpan),
                nameof(DateTime),
                nameof(StringSplitOptions),

                // System.IO
                nameof(Directory),
                nameof(DirectoryInfo),
                nameof(File),
                nameof(FileInfo),
                nameof(Path),

                // Hg API
                nameof(Logger),
                nameof(BackupHelper),
                nameof(BackupHelperCanCopyMode),
                nameof(ActionSource),

                nameof(EngineSettingKind),
                nameof(EngineSettingFolderBrowser),
                nameof(EngineSettingCombobox),
                nameof(EngineSettingCheckbox),
                nameof(EngineSettingTextbox),

                nameof(EngineSnapshotColumnDefinition),

                nameof(EngineSnapshotCustomValueBoolean),
                nameof(EngineSnapshotCustomValueDateTime),
                nameof(EngineSnapshotCustomValueInteger),
                nameof(EngineSnapshotCustomValueString),
                nameof(EngineSnapshotCustomValueTimeSpan),

                nameof(EngineSnapshotStatus),
                nameof(EngineSnapshotCategory),
                nameof(EngineSnapshot),

                nameof(EngineWatcher),

                nameof(Engine),

                // Hg Global functions
                nameof(HgUtility),
                nameof(HgConverter),
                nameof(HgSteamHelper),

                // Hg Scripts Specific
                nameof(HgScriptSpecific),

                // Utility lua functions
                //"Utility",
            };

            string env = "{ ";

            foreach (string value in values)
            {
                env += value + " = " + value + ", ";
            }

            env += "nil }";

            return env;
        }

        private bool LoadFile(EngineScriptFile engineScriptFile)
        {
            if (File.Exists(engineScriptFile.FileFullName))
            {
                try
                {
                    string content = File.ReadAllText(engineScriptFile.FileFullName);

                    _lua["_hg_chunk_"] = content;
                    _lua["_hg_chunk_name_"] = engineScriptFile.FileName;
                    _lua.DoString(@"_sandbox_.run(_hg_chunk_, _hg_chunk_name_, nil, _engine_, _engine_namespace_)");
                    _lua["_hg_chunk_"] = null;
                    _lua["_hg_chunk_name_"] = null;
                }
                catch (Exception ex)
                {
                    if (ex is LuaScriptException luaEx)
                    {
                        Logger.Log(luaEx.Message, LogLevel.Debug);
                    }

                    Logger.LogException(ex);
                    return false;
                }
                finally
                {
                    _lua["_hg_chunk_"] = null;
                    _lua["_hg_chunk_name_"] = null;
                }

                return true;
            }

            return false;
        }

        private void LoadSettings(List<ProfileSetting> settings)
        {
            foreach (EngineSetting engineSetting in ActiveEngine.Settings)
            {
                // Get user value and update engine
                ProfileSetting setting = settings.FirstOrDefault(s => s.Name == engineSetting.Name);
                if (setting != null)
                {
                    if (engineSetting is EngineSettingCombobox engineSettingCombobox &&
                        setting is ProfileSettingInteger profileSettingInteger)
                    {
                        engineSettingCombobox.Value = profileSettingInteger.Value;
                    }

                    if (engineSetting is EngineSettingFolderBrowser engineSettingFolder &&
                        setting is ProfileSettingString profileSettingString)
                    {
                        engineSettingFolder.Value = profileSettingString.Value;
                    }

                    if (engineSetting is EngineSettingCheckbox engineSettingCheckbox &&
                        setting is ProfileSettingBoolean profileSettingBoolean)
                    {
                        engineSettingCheckbox.Value = profileSettingBoolean.Value;
                    }

                    if (engineSetting is EngineSettingTextbox engineSettingTextbox &&
                        setting is ProfileSettingString profileSettingString2)
                    {
                        engineSettingTextbox.Value = profileSettingString2.Value;
                    }
                }
            }
        }

        private void LoadSnapshots(List<EngineSnapshot> snapshots)
        {
            ActiveEngine.Snapshots.Clear();
            ActiveEngine.Snapshots.AddRange(snapshots);
        }

        //private void RegisterFunction(Type type, string methodName, Type[] types, string luaName)
        //{
        //    var method = type.GetMethod(methodName, types);
        //    _lua.RegisterFunction(luaName, method);
        //}

        //private void RegisterFunction(Type type, string methodName, string luaName)
        //{
        //    var method = type.GetMethod(methodName);
        //    _lua.RegisterFunction(luaName, method);
        //}

        //private void RegisterFunction(object obj, string methodName, Type[] types, string luaName)
        //{
        //    var method = obj.GetType().GetMethod(methodName, types);
        //    _lua.RegisterFunction(luaName, obj, method);
        //}

        //private void RegisterFunction(object obj, string methodName, string luaName)
        //{
        //    var method = obj.GetType().GetMethod(methodName);
        //    _lua.RegisterFunction(luaName, obj, method);
        //}

        private void SetSandbox()
        {
            _lua.LoadCLRPackage();

            _lua.DoString(@" import ('mscorlib', 'System') ");
            _lua.DoString(@" import ('mscorlib', 'System.IO') ");

            _lua.DoString(@" import ('Hg.SaveHistory', 'Hg.SaveHistory.API') ");

            // declare sandbox
            _lua["_sandbox_"] = _lua.DoString(Resources.Lua_Sandbox).First();

            // setup environment
            _lua.DoString(@"_sandbox_.init({env = " + AllowedGlobals() + "})");

            // unbind import function
            _lua.DoString(@"import = function (...) end");
        }

        #endregion
    }
}