using System;
using System.IO;
using System.Reflection;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Types;
using Logger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.Managers
{
    public delegate void BackupStatusChangedEventHandler();

    public delegate bool ProcessRunningEventHandler();

    public class WatcherManager
    {
        #region Fields & Properties

        public event BackupStatusChangedEventHandler AutoBackupStatusChanged;

        public ProcessRunningEventHandler IsProcessRunning;

        private readonly string _parentName;

        private readonly string _parentPath;

        private AutoBackupStatus _autoBackupEnabled;

        private bool _exiting;

        private FileSystemWatcher _fileSystemWatcherMain;
        private FileSystemWatcher _fileSystemWatcherParentFolder;

        private EngineWatcher _watcher;

        public AutoBackupStatus AutoBackupStatus
        {
            get => _autoBackupEnabled;
            set
            {
                _autoBackupEnabled = value;
                AutoBackupStatusChanged?.Invoke();
            }
        }

        #endregion

        #region Members

        public WatcherManager(EngineWatcher watcher)
        {
            _watcher = watcher;
            _parentPath = Path.GetDirectoryName(_watcher.Path);
            if (!string.IsNullOrEmpty(_parentPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_watcher.Path);
                _parentName = directoryInfo.Name;
                if (string.IsNullOrEmpty(_parentName))
                {
                    _parentName = null;
                }

                // ReSharper disable once RedundantAssignment
                directoryInfo = null;
            }
            else
            {
                _parentPath = null;
                _parentName = null;
            }

            AutoBackupStatus = AutoBackupStatus.Disabled;

            Logger.Debug("WatcherManager: _parentPath=", _parentPath);
            Logger.Debug("WatcherManager: _parentName=", _parentName);
        }

        public void Release()
        {
            Logger.Information(MethodBase.GetCurrentMethod().DeclaringType.Name, ".", MethodBase.GetCurrentMethod().Name);

            _exiting = true;

            IsProcessRunning = null;

            AutoBackupStatusChanged = null;

            if (_fileSystemWatcherParentFolder != null)
            {
                _fileSystemWatcherParentFolder.EnableRaisingEvents = false;

                _fileSystemWatcherParentFolder.Created -= FileSystemWatcherParentFolderOnCreated;
                _fileSystemWatcherParentFolder.Deleted -= FileSystemWatcherParentFolderOnDeleted;
            }

            _fileSystemWatcherParentFolder = null;

            if (_fileSystemWatcherMain != null)
            {
                _fileSystemWatcherMain.EnableRaisingEvents = false;

                if (_watcher.WatchCreated)
                {
                    _fileSystemWatcherMain.Created -= FileSystemWatcherMainOnCreated;
                }

                if (_watcher.WatchChanged)
                {
                    _fileSystemWatcherMain.Changed -= FileSystemWatcherMainOnChanged;
                }

                if (_watcher.WatchDeleted)
                {
                    _fileSystemWatcherMain.Deleted -= FileSystemWatcherMainOnDeleted;
                }

                if (_watcher.WatchRenamed)
                {
                    _fileSystemWatcherMain.Renamed -= FileSystemWatcherMainOnRenamed;
                }
            }

            _fileSystemWatcherMain = null;

            _watcher = null;
        }

        public AutoBackupStatus SetAutoBackup(bool enable)
        {
            Logger.Debug("SetAutoBackup=", enable);

            _autoBackupEnabled = SetWatchers(enable);
            return _autoBackupEnabled;
        }

        public AutoBackupStatus SetWatchers(bool enable)
        {
            Logger.Debug("SetWatchers=", enable);

            if (enable)
            {
                try
                {
                    if (_watcher.WatchParent && _parentPath != null && _parentName != null)
                    {
                        if (_fileSystemWatcherParentFolder == null)
                        {
                            _fileSystemWatcherParentFolder = new FileSystemWatcher
                            {
                                Path = _parentPath,
                                Filter = _parentName,
                                EnableRaisingEvents = false,
                                NotifyFilter = NotifyFilters.DirectoryName
                            };
                            _fileSystemWatcherParentFolder.Created += FileSystemWatcherParentFolderOnCreated;
                            _fileSystemWatcherParentFolder.Deleted += FileSystemWatcherParentFolderOnDeleted;
                        }

                        _fileSystemWatcherParentFolder.EnableRaisingEvents = true;
                    }

                    if (!Directory.Exists(_watcher.Path))
                    {
                        if (_fileSystemWatcherMain != null)
                        {
                            _fileSystemWatcherMain.EnableRaisingEvents = false;
                        }

                        _fileSystemWatcherMain = null;

                        Logger.Debug("SetWatchers Waiting");
                        return AutoBackupStatus.Waiting;
                    }

                    if (_fileSystemWatcherMain == null)
                    {
                        MakeMainWatcher();
                    }

                    SetMainWatcher(true);

                    Logger.Debug("SetWatchers Enabled");
                    return AutoBackupStatus.Enabled;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }

                Logger.Debug("SetWatchers Disabled");
                return AutoBackupStatus.Disabled;
            }

            if (_fileSystemWatcherMain != null)
            {
                _fileSystemWatcherMain.EnableRaisingEvents = false;
            }

            if (_fileSystemWatcherParentFolder != null)
            {
                _fileSystemWatcherParentFolder.EnableRaisingEvents = false;
            }

            Logger.Debug("SetWatchers Disabled");
            return AutoBackupStatus.Disabled;
        }

        private void DoOnEvent(EngineWatcherEventType eventType, object arg)
        {
            if (_exiting)
            {
                return;
            }

            if (IsProcessRunning != null && IsProcessRunning())
            {
                try
                {
                    _watcher.OnEvent?.Call(eventType, arg);
                }
                catch (Exception ex)
                {
                    Logger.Error("", ex.Message);
                    Logger.LogException(ex);
                }
            }
            else
            {
                Logger.Debug("DoOnEvent: Changes detected but Process is not running :(");
            }
        }

        private void FileSystemWatcherMainOnChanged(object sender, FileSystemEventArgs args)
        {
            DoOnEvent(EngineWatcherEventType.Changed, args);
        }

        private void FileSystemWatcherMainOnCreated(object sender, FileSystemEventArgs args)
        {
            DoOnEvent(EngineWatcherEventType.Created, args);
        }

        private void FileSystemWatcherMainOnDeleted(object sender, FileSystemEventArgs args)
        {
            DoOnEvent(EngineWatcherEventType.Deleted, args);
        }

        private void FileSystemWatcherMainOnRenamed(object sender, RenamedEventArgs args)
        {
            DoOnEvent(EngineWatcherEventType.Renamed, args);
        }

        private void FileSystemWatcherParentFolderOnCreated(object sender, FileSystemEventArgs e)
        {
            Logger.Debug("FileSystemWatcherParentFolderOnCreated");

            if (_exiting)
            {
                return;
            }

            if (_autoBackupEnabled == AutoBackupStatus.Waiting)
            {
                MakeMainWatcher();
                SetMainWatcher(true);
                AutoBackupStatus = AutoBackupStatus.Enabled;
            }
        }

        private void FileSystemWatcherParentFolderOnDeleted(object sender, FileSystemEventArgs e)
        {
            Logger.Debug("FileSystemWatcherParentFolderOnDeleted");

            if (_exiting)
            {
                return;
            }

            if (_fileSystemWatcherMain != null)
            {
                SetMainWatcher(false);
                _fileSystemWatcherMain = null;
            }

            if (_autoBackupEnabled == AutoBackupStatus.Enabled)
            {
                AutoBackupStatus = AutoBackupStatus.Waiting;
            }
        }

        private void MakeMainWatcher()
        {
            Logger.Debug("MakeMainWatcher");

            // Create new
            _fileSystemWatcherMain = new FileSystemWatcher
            {
                Path = _watcher.Path,
                Filter = _watcher.Filter,
                EnableRaisingEvents = false,
                NotifyFilter = (NotifyFilters) _watcher.NotifyFilter
            };

            if (_watcher.WatchCreated)
            {
                _fileSystemWatcherMain.Created += FileSystemWatcherMainOnCreated;
            }

            if (_watcher.WatchChanged)
            {
                _fileSystemWatcherMain.Changed += FileSystemWatcherMainOnChanged;
            }

            if (_watcher.WatchDeleted)
            {
                _fileSystemWatcherMain.Deleted += FileSystemWatcherMainOnDeleted;
            }

            if (_watcher.WatchRenamed)
            {
                _fileSystemWatcherMain.Renamed += FileSystemWatcherMainOnRenamed;
            }

            Logger.Debug("MakeMainWatcher: _fileSystemWatcherMain created");
        }

        private void SetMainWatcher(bool enable)
        {
            Logger.Debug("SetMainWatcher=", enable);

            if (_fileSystemWatcherMain == null)
            {
                return;
            }

            if (enable)
            {
                _fileSystemWatcherMain.EnableRaisingEvents = true;
                Logger.Debug("SetMainWatcher Enabled");
            }
            else
            {
                _fileSystemWatcherMain.EnableRaisingEvents = false;
                Logger.Debug("SetMainWatcher Disabled");
            }
        }

        #endregion
    }
}