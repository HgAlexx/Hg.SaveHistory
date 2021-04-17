using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;
using Newtonsoft.Json;

namespace Hg.SaveHistory.Managers
{
    public delegate void SettingEventHandler();

    public class SettingsManager
    {
        #region Fields & Properties

        private const string SettingsFilename = "settings.json";

        public event SettingEventHandler AutoBackupSoundNotificationChanged;
        public event SettingEventHandler AutoSelectLastSnapshotChanged;

        public event SettingEventHandler HighlightSelectedSnapshotChanged;
        public event SettingEventHandler HighlightSelectedSnapshotColorChanged;

        public event SettingEventHandler HotKeysActiveChanged;
        public event SettingEventHandler HotKeysSoundChanged;

        public event SettingEventHandler NotificationModeChanged;

        public event SettingEventHandler PinnedProfilesChanged;
        public event SettingEventHandler RecentProfilesChanged;

        public event SettingEventHandler SaveSizeAndPositionChanged;

        public event SettingEventHandler ScreenshotQualityChanged;
        public event SettingEventHandler SnapToScreenEdgesChanged;

        public event SettingEventHandler SortKindChanged;
        public event SettingEventHandler SortOrderChanged;

        private Settings _settings;

        private string _settingsContent;
        private string _settingsFilePath;

        public bool AutoBackupSoundNotification
        {
            get => _settings.AutoBackupSoundNotification;
            set
            {
                _settings.AutoBackupSoundNotification = value;
                AutoBackupSoundNotificationChanged?.Invoke();
            }
        }

        public bool AutoSelectLastSnapshot
        {
            get => _settings.AutoSelectLastSnapshot;
            set
            {
                _settings.AutoSelectLastSnapshot = value;
                AutoSelectLastSnapshotChanged?.Invoke();
            }
        }

        public bool HighlightSelectedSnapshot
        {
            get => _settings.HighlightSelectedSnapshot;
            set
            {
                _settings.HighlightSelectedSnapshot = value;
                HighlightSelectedSnapshotChanged?.Invoke();
            }
        }

        public Color HighlightSelectedSnapshotColor
        {
            get => _settings.HighlightSelectedSnapshotColor;
            set
            {
                _settings.HighlightSelectedSnapshotColor = value;
                HighlightSelectedSnapshotColorChanged?.Invoke();
            }
        }

        public bool HotKeysActive
        {
            get => _settings.HotKeysActive;
            set
            {
                _settings.HotKeysActive = value;
                HotKeysActiveChanged?.Invoke();
            }
        }

        public bool HotKeysSound
        {
            get => _settings.HotKeysSound;
            set
            {
                _settings.HotKeysSound = value;
                HotKeysSoundChanged?.Invoke();
            }
        }

        public List<HotKeyToAction> HotKeyToActions => _settings.HotKeyToActions;

        public Point? Location
        {
            get => _settings.Location;
            set => _settings.Location = value;
        }

        public MessageMode NotificationMode
        {
            get => _settings.NotificationMode;
            set
            {
                _settings.NotificationMode = value;
                NotificationModeChanged?.Invoke();
            }
        }

        public List<string> PinnedProfiles => _settings.PinnedProfiles;


        public List<string> RecentProfiles => _settings.RecentProfiles;

        public bool SaveSizeAndPosition
        {
            get => _settings.SaveSizeAndPosition;
            set
            {
                _settings.SaveSizeAndPosition = value;
                SaveSizeAndPositionChanged?.Invoke();
            }
        }

        public ScreenshotQuality ScreenshotQuality
        {
            get => _settings.ScreenshotQuality;
            set
            {
                _settings.ScreenshotQuality = value;
                ScreenshotQualityChanged?.Invoke();
            }
        }

        public Size? Size
        {
            get => _settings.Size;
            set => _settings.Size = value;
        }

        public bool SnapToScreenEdges
        {
            get => _settings.SnapToScreenEdges;
            set
            {
                _settings.SnapToScreenEdges = value;
                SnapToScreenEdgesChanged?.Invoke();
            }
        }

        #endregion

        #region Members

        public SettingsManager()
        {
            ResetSettings();
        }

        public void AddPinnedProfiles(string path)
        {
            if (PinnedProfiles.Contains(path))
            {
                PinnedProfiles.Remove(path);
            }

            PinnedProfiles.Add(path);
            PinnedProfiles.Sort();
            PinnedProfilesChanged?.Invoke();
        }

        public void AddRecentProfiles(string path)
        {
            if (RecentProfiles.Contains(path))
            {
                RecentProfiles.Remove(path);
            }

            RecentProfiles.Insert(0, path);
            RecentProfilesChanged?.Invoke();
        }

        public void DetectAndLoadFile()
        {
            _settingsFilePath = "";

            string exeFolder = Path.GetDirectoryName(Application.ExecutablePath);
            if (exeFolder == null)
            {
                throw new ApplicationException("Unable to get Application folder path");
            }

            string portableSettingsFile = Path.Combine(exeFolder, SettingsFilename);
            if (File.Exists(portableSettingsFile))
            {
                _settingsFilePath = portableSettingsFile;
                LoadSettings();
                return;
            }

            string appDataSettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Hg\Hg.SaveHistory\settings.json");
            if (File.Exists(appDataSettingsFile))
            {
                _settingsFilePath = appDataSettingsFile;
                LoadSettings();
                return;
            }

            if (_settingsFilePath == "")
            {
                DialogResult result = MessageBox.Show(
                    @"Where do you want the application settings to be saved ?" + Environment.NewLine +
                    @"- In your Windows profile (Default mode) => click Yes" + Environment.NewLine +
                    @"- In the application directory (Portable mode) => click No",
                    @"First Time Setup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                {
                    _settingsFilePath = appDataSettingsFile;
                }
                else
                {
                    _settingsFilePath = portableSettingsFile;
                }
            }

            if (_settingsFilePath != "")
            {
                LoadSettings();
            }
            else
            {
                MessageBox.Show(@"Unable to load settings :( Exiting...", @"Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        public void LoadSettings()
        {
            _settingsContent = "";
            if (File.Exists(_settingsFilePath))
            {
                _settingsContent = File.ReadAllText(_settingsFilePath);
                if (_settingsContent != "")
                {
                    try
                    {
                        _settings = JsonConvert.DeserializeObject<Settings>(_settingsContent,
                            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

                        if (_settings?.PinnedProfiles != null && _settings.PinnedProfiles.Count > 0)
                        {
                            _settings.PinnedProfiles.RemoveAll(path => !File.Exists(path));
                        }


                        if (_settings?.RecentProfiles != null && _settings.RecentProfiles.Count > 0)
                        {
                            _settings.RecentProfiles.RemoveAll(path => !File.Exists(path));
                            _settings.RecentProfiles.RemoveAll(path => _settings.PinnedProfiles.Contains(path));
                        }

                        PinnedProfiles.Sort();


                        CompleteHotKeyToActions();

                        NotifyAll();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Error loading settings: " + e.Message, LogLevel.Debug);
                        ResetSettings();
                    }
                }
            }
            else
            {
                ResetSettings();
            }
        }

        public void RemovePinnedProfiles(string path)
        {
            if (PinnedProfiles.Contains(path))
            {
                PinnedProfiles.Remove(path);
            }

            PinnedProfiles.Sort();
            PinnedProfilesChanged?.Invoke();
        }

        public void RemoveRecentProfiles(string path)
        {
            if (RecentProfiles.Contains(path))
            {
                RecentProfiles.Remove(path);
            }

            RecentProfilesChanged?.Invoke();
        }

        public void ResetSettings()
        {
            _settings = new Settings();

            CompleteHotKeyToActions();

            NotifyAll();
        }

        public void SaveSettings()
        {
            _settingsContent = JsonConvert.SerializeObject(_settings, Formatting.Indented,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

            string folder = Path.GetDirectoryName(_settingsFilePath);
            if (folder != null)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            File.WriteAllText(_settingsFilePath, _settingsContent);
        }

        private void CompleteHotKeyToActions()
        {
            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotPrevious))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotPrevious,
                    HotKey = new HotKey(Keys.Up, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotNext))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotNext,
                    HotKey = new HotKey(Keys.Down, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.CategoryPrevious))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.CategoryPrevious,
                    HotKey = new HotKey(Keys.Left, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.CategoryNext))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.CategoryNext,
                    HotKey = new HotKey(Keys.Right, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotFirst))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotFirst,
                    HotKey = new HotKey(Keys.PageUp, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotLast))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotLast,
                    HotKey = new HotKey(Keys.PageDown, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotBackup))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotBackup,
                    HotKey = new HotKey(Keys.NumPad1, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotDelete))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotDelete,
                    HotKey = new HotKey(Keys.NumPad2, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SnapshotRestore))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SnapshotRestore,
                    HotKey = new HotKey(Keys.NumPad3, true, true, false)
                });
            }

            if (!HotKeyToActions.Exists(i => i.Action == HotKeyAction.SettingSwitchAutoBackup))
            {
                HotKeyToActions.Add(new HotKeyToAction
                {
                    Enabled = true,
                    Action = HotKeyAction.SettingSwitchAutoBackup,
                    HotKey = new HotKey(Keys.NumPad8, true, true, false)
                });
            }
        }

        private void NotifyAll()
        {
            AutoBackupSoundNotificationChanged?.Invoke();
            AutoSelectLastSnapshotChanged?.Invoke();

            HighlightSelectedSnapshotChanged?.Invoke();
            HighlightSelectedSnapshotColorChanged?.Invoke();

            HotKeysActiveChanged?.Invoke();
            HotKeysSoundChanged?.Invoke();

            NotificationModeChanged?.Invoke();

            PinnedProfilesChanged?.Invoke();
            RecentProfilesChanged?.Invoke();

            SaveSizeAndPositionChanged?.Invoke();
            SnapToScreenEdgesChanged?.Invoke();

            ScreenshotQualityChanged?.Invoke();

            SortKindChanged?.Invoke();
            SortOrderChanged?.Invoke();
        }

        #endregion
    }
}