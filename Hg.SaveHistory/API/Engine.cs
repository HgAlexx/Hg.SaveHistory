using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hg.SaveHistory.Types;
using NLua;

namespace Hg.SaveHistory.API
{
    public class Engine
    {
        #region Fields & Properties

        // Main Events
        public LuaFunction OnActionSnapshotBackup = null;
        public LuaFunction OnActionSnapshotRestore = null;

        public event EventHandlerAutoBackupOccurred OnAutoBackupOccurred;

        public event EventHandlerCategoriesChanged OnCategoriesChanges;

        // Before the engine is unloaded
        public LuaFunction OnClose = null;

        // After GUI initialized, Before loading snapshots/categories
        public LuaFunction OnInitialize = null;

        public event MessageEventHandler OnMessage;

        // After engine and settings loaded
        public LuaFunction OnOpen = null;

        public LuaFunction OnSetupSuggestProfileName = null;

        public LuaFunction OnSetupValidate = null;

        public event EventHandlerSnapshotsChanged OnSnapshotsChanges;

        public List<EngineSnapshotCategory> Categories { get; } = new List<EngineSnapshotCategory>();

        public string Description { get; }

        public EngineSnapshot LastSnapshot { get; set; }

        public string Name { get; }

        [LuaHide] public List<string> ProcessNames { get; } = new List<string>();

        public ScreenshotHelper ScreenshotHelper { get; }

        public List<EngineSetting> Settings { get; } = new List<EngineSetting>();

        public List<EngineSnapshotColumnDefinition> SnapshotColumnsDefinition { get; } =
            new List<EngineSnapshotColumnDefinition>();

        public List<EngineSnapshot> Snapshots { get; } = new List<EngineSnapshot>();

        public string SnapshotsFolder { get; }

        public string Title { get; }


        [LuaHide] public EngineWatcher Watcher { get; set; }

        #endregion

        #region Members

        public Engine(EngineScript engineScript, string snapshotsFolder)
        {
            if (snapshotsFolder != null)
            {
                SnapshotsFolder = snapshotsFolder;
                if (!Directory.Exists(SnapshotsFolder))
                {
                    Directory.CreateDirectory(SnapshotsFolder);
                }

                Logger.Debug(SnapshotsFolder);
            }

            ScreenshotHelper = new ScreenshotHelper(this);

            Name = engineScript.Name;
            Title = engineScript.Title;
            Description = engineScript.Description;

            // Default, mandatory columns
            SnapshotColumnsDefinition.Add(new EngineSnapshotColumnDefinition
            {
                Key = "SavedAt",
                HeaderText = "Saved At",
                Order = 0, // first column
            });

            SnapshotColumnsDefinition.Add(new EngineSnapshotColumnDefinition
            {
                Key = "Notes",
                HeaderText = "Notes",
                Order = 999, // last column
            });
        }

        // Actions
        public bool ActionSnapshotBackup(ActionSource actionSource, params object[] args)
        {
            Logger.Information(MethodBase.GetCurrentMethod().Name);

            if (OnActionSnapshotBackup.Call(actionSource, args).First() is bool b)
            {
                return b;
            }

            return false;
        }

        public bool ActionSnapshotRestore(ActionSource actionSource, EngineSnapshot snapshot, params object[] args)
        {
            Logger.Information(MethodBase.GetCurrentMethod().Name);

            if (OnActionSnapshotRestore.Call(actionSource, snapshot, args).First() is bool b)
            {
                return b;
            }

            return false;
        }


        public void AddProcessName(string name)
        {
            if (!ProcessNames.Contains(name))
            {
                ProcessNames.Add(name);
            }
        }

        public int AddSetting(EngineSetting setting)
        {
            EngineSetting found = Settings.FirstOrDefault(s => s.Name == setting.Name);
            if (found != null)
            {
                return -1;
            }

            Settings.Add(setting);
            setting.Index = Settings.Count - 1;
            return setting.Index;
        }

        public bool AddSnapshotColumnDefinition(EngineSnapshotColumnDefinition columnDefinition)
        {
            EngineSnapshotColumnDefinition found =
                SnapshotColumnsDefinition.FirstOrDefault(c => c.Key == columnDefinition.Key);
            if (found != null)
            {
                return false;
            }

            SnapshotColumnsDefinition.Add(columnDefinition);
            columnDefinition.Order = SnapshotColumnsDefinition.Count - 1;
            return true;
        }


        public void AutoBackupOccurred(bool success)
        {
            OnAutoBackupOccurred?.Invoke(success);
        }

        public void CategoriesChanges()
        {
            Logger.Information(MethodBase.GetCurrentMethod().Name);

            OnCategoriesChanges?.Invoke();
        }

        public EngineSetting GetSettingByName(string name)
        {
            return Settings.FirstOrDefault(setting => setting.Name == name);
        }

        public void SetupWatcher(EngineWatcher watcher)
        {
            Watcher = watcher;
        }

        public void SnapshotsChanges()
        {
            Logger.Information(MethodBase.GetCurrentMethod().Name);

            OnSnapshotsChanges?.Invoke();
        }

        #endregion

        //private DialogResult Message(string text, string caption, MessageType type, MessageMode mode)
        //{
        //    Logger.Debug("Message:" + text);

        //    if (OnMessage != null)
        //    {
        //        return Message(text, caption, type, mode);
        //    }

        //    return DialogResult.None;
        //}
    }
}