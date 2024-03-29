﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Controls;
using Hg.SaveHistory.Managers;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;
using Hg.SaveHistory.Wizards;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Logger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.Forms
{
    public partial class FormMain : Form
    {
        #region Fields & Properties

        private const int NotifyIconBalloonTimeout = 1500;

        private readonly EngineScriptManager _engineScriptManager;

        private readonly object _restoreSaveLock = new object();

        private readonly SettingsManager _settingsManager;

        private readonly Version _version;

        private ProfileFile _activeProfileFile;

        private bool _autoBackupEnabled;

        private int _borderOffset;

        private Comparison<EngineSnapshot> _comparer;

        private FormDebugConsole _debugConsole;

        private HotKeysManager _hotKeysManager;

        private bool _loading;

        private LuaManager _luaManager;

        private EngineSnapshot _selectedSnapshot;

        private string _sortKey;

        private SortOrder _sortOrder = SortOrder.Ascending;

        private WatcherManager _watcherManager;

        #endregion

        #region Members

        public FormMain()
        {
            InitializeComponent();
            _version = Assembly.GetExecutingAssembly().GetName().Version;

            string applicationDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            if (applicationDirectory == null)
            {
                throw new ApplicationException("Unable to get Application folder path");
            }

            _settingsManager = new SettingsManager();

            _settingsManager.RecentProfilesChanged += RefreshRecentProfiles;
            _settingsManager.PinnedProfilesChanged += RefreshPinnedProfiles;

            _settingsManager.AutoSelectLastSnapshotChanged += () =>
            {
                autoSelectLastToolStripMenuItem.Checked = _settingsManager.AutoSelectLastSnapshot;
            };

            _settingsManager.AutoBackupSoundNotificationChanged += () =>
            {
                soundNotificationToolStripMenuItem.Checked = _settingsManager.AutoBackupSoundNotification;
            };

            _settingsManager.HighlightSelectedSnapshotChanged += () =>
            {
                highlightSelectedToolStripMenuItem.Checked = _settingsManager.HighlightSelectedSnapshot;

                if (_activeProfileFile == null)
                {
                    return;
                }

                listViewSnapshot_SelectedIndexChanged(null, null);

                if (!_settingsManager.HighlightSelectedSnapshot)
                {
                    listViewSnapshot.Items.Cast<ListViewItem>()
                        .ToList().ForEach(item =>
                        {
                            item.BackColor = listViewSnapshot.BackColor;
                            item.ForeColor = listViewSnapshot.ForeColor;
                        });
                }
            };

            _settingsManager.HighlightSelectedSnapshotColorChanged += () =>
            {
                if (_activeProfileFile != null)
                {
                    listViewSnapshot_SelectedIndexChanged(null, null);
                }
            };

            _settingsManager.NotificationModeChanged += () =>
            {
                messageBoxToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.MessageBox;
                statusBarToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.Status;
            };

            _settingsManager.SaveSizeAndPositionChanged += () =>
            {
                saveSizeAndPositionToolStripMenuItem.Checked = _settingsManager.SaveSizeAndPosition;
            };

            _settingsManager.SnapToScreenEdgesChanged += () =>
            {
                snapToScreenEdgesToolStripMenuItem.Checked = _settingsManager.SnapToScreenEdges;
            };

            _settingsManager.StartWithWindowsChanged += () =>
            {
                startWithWindowsToolStripMenuItem.Checked = _settingsManager.StartWithWindows;
                CheckStartWithWindowsStatus();
            };

            _settingsManager.StartMinimizedChanged += () => { startMinimizedToolStripMenuItem.Checked = _settingsManager.StartMinimized; };

            _settingsManager.MinimizedToTrayChanged += () =>
            {
                minimizedToTrayToolStripMenuItem.Checked = _settingsManager.MinimizedToTray;
            };

            _settingsManager.ShowTrayNotificationChanged += () =>
            {
                showTrayNotificationToolStripMenuItem.Checked = _settingsManager.ShowTrayNotification;
            };

            _settingsManager.OpenLastUsedProfileOnStartupChanged += () =>
            {
                openLastUsedProfileOnStartupToolStripMenuItem.Checked = _settingsManager.OpenLastUsedProfileOnStartup;
            };

            _settingsManager.ScreenshotQualityChanged += () =>
            {
                giflowSizeToolStripMenuItem.Checked = _settingsManager.ScreenshotQuality == ScreenshotQuality.Gif;
                jpgmediumToolStripMenuItem.Checked = _settingsManager.ScreenshotQuality == ScreenshotQuality.Jpg;
                pnghugeSizeToolStripMenuItem.Checked = _settingsManager.ScreenshotQuality == ScreenshotQuality.Png;

                ScreenshotHelper.ScreenshotFormat = _settingsManager.ScreenshotQuality;
            };

            _settingsManager.HotKeysActiveChanged += () =>
            {
                hotKeysToolStripMenuItem.Checked = _settingsManager.HotKeysActive;
                enabledHotKeysToolStripMenuItem.Checked = _settingsManager.HotKeysActive;

                if (_settingsManager.HotKeysActive)
                {
                    _hotKeysManager?.Hook();
                    if (_hotKeysManager != null)
                    {
                        toolStripHotKey.Text = "Hot Keys: Enabled";
                        SetToolStripHotKeyStyle(toolStripHotKey, IconChar.CheckCircle, Color.LimeGreen);
                    }
                }
                else
                {
                    _hotKeysManager?.UnHook();
                    if (_hotKeysManager != null)
                    {
                        toolStripHotKey.Text = "Hot Keys: Disabled";
                        SetToolStripHotKeyStyle(toolStripHotKey, IconChar.TimesCircle, Color.Red);
                    }
                }

                if (_hotKeysManager == null)
                {
                    toolStripHotKey.Text = "Hot Keys: Idle";
                    SetToolStripHotKeyStyle(toolStripHotKey, IconChar.Circle, Color.Gray);
                }
            };

            _settingsManager.HotKeysSoundChanged += () => { soundToolStripMenuItem.Checked = _settingsManager.HotKeysSound; };

            _engineScriptManager = new EngineScriptManager();
            _engineScriptManager.ScanFolder(Path.Combine(applicationDirectory, "Scripts"));

            ApplyStyle();

            HideProfilePage();
            HideReadMePage();

            profileToolStripMenuItem.Enabled = false;
        }

        private void CheckStartWithWindowsStatus()
        {
            if (_settingsManager == null)
            {
                return;
            }

            const string appName = @"Hg.SaveHistory";
            const string registryKeyPath = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            string appPath = Application.ExecutablePath;
            try
            {
                var myKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                {
                    using (var subKey = myKey.OpenSubKey(
                               registryKeyPath,
                               RegistryKeyPermissionCheck.ReadWriteSubTree, 
                               RegistryRights.QueryValues | RegistryRights.SetValue))
                    {
                        if (subKey == null)
                        {
                            return;
                        }

                        if (!_settingsManager.StartWithWindows)
                        {
                            if (subKey.GetValueNames().Contains(appName))
                            {
                                subKey.DeleteValue(appName, false);
                            }
                        }

                        if (_settingsManager.StartWithWindows)
                        {
                            if (!subKey.GetValueNames().Contains(appName))
                            {
                                subKey.SetValue(appName, "\"" + appPath + "\"", RegistryValueKind.String);
                            }
                        }

                        subKey.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("StartWithWindows: unable to register or unregister the application into the Windows registry.");
                Logger.Error(ex.Message);
            }
        }

        public void CategorySelectNext()
        {
            if (comboBoxCategories.Items.Count <= 0)
            {
                return;
            }

            int current = comboBoxCategories.SelectedIndex;
            current++;

            if (current < 0 || current >= comboBoxCategories.Items.Count)
            {
                return;
            }

            comboBoxCategories.SelectedIndex = current;
            comboBoxCategories_SelectionChangeCommitted(null, null);
        }

        public void CategorySelectPrevious()
        {
            if (comboBoxCategories.Items.Count <= 0)
            {
                return;
            }

            int current = comboBoxCategories.SelectedIndex;
            current--;

            if (current < 0 || current >= comboBoxCategories.Items.Count)
            {
                return;
            }

            comboBoxCategories.SelectedIndex = current;
            comboBoxCategories_SelectionChangeCommitted(null, null);
        }

        public IntPtr GetProcessPtr()
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return IntPtr.Zero;
            }

            var processPtr = IntPtr.Zero;
            foreach (var process in Process.GetProcesses())
            {
                if (!_luaManager.ActiveEngine.ProcessNames.Contains(process.ProcessName))
                {
                    continue;
                }

                processPtr = process.MainWindowHandle;
                if (processPtr == IntPtr.Zero)
                {
                    processPtr = process.Handle;
                }

                break;
            }

            return processPtr;
        }

        public bool IsOnScreen(Form form)
        {
            Rectangle rect = new Rectangle(form.Left, form.Top, form.Width, form.Height);

            return Screen.AllScreens.Select(screen => screen.Bounds).Aggregate(Rectangle.Union).Contains(rect);
        }

        public void SnapshotSelectActiveTab()
        {
            if (tabControlSaves.SelectedTab != tabPageActiveSaves)
            {
                tabControlSaves.SelectedTab = tabPageActiveSaves;
            }
        }

        public void SnapshotSelectFirst()
        {
            if (listViewSnapshot.Items.Count <= 0)
            {
                return;
            }

            listViewSnapshot.SelectedIndices.Clear();
            listViewSnapshot.SelectedIndices.Add(0);
            if (listViewSnapshot.SelectedItems.Count == 1)
            {
                listViewSnapshot.SelectedItems[0].EnsureVisible();
                listViewSnapshot.FocusedItem = listViewSnapshot.SelectedItems[0];
            }

            listViewSnapshot_SelectedIndexChanged(null, null);
        }

        public void SnapshotSelectLast()
        {
            if (listViewSnapshot.Items.Count <= 0)
            {
                return;
            }

            int current = listViewSnapshot.Items.Count - 1;
            if (current >= 0 && current < listViewSnapshot.Items.Count)
            {
                listViewSnapshot.SelectedIndices.Clear();
                listViewSnapshot.SelectedIndices.Add(current);
                if (listViewSnapshot.SelectedItems.Count == 1)
                {
                    listViewSnapshot.SelectedItems[0].EnsureVisible();
                    listViewSnapshot.FocusedItem = listViewSnapshot.SelectedItems[0];
                }

                listViewSnapshot_SelectedIndexChanged(null, null);
            }
        }

        public void SnapshotSelectNext()
        {
            if (listViewSnapshot.Items.Count <= 0)
            {
                return;
            }

            int current = listViewSnapshot.SelectedIndices.Count > 0
                ? listViewSnapshot.SelectedIndices[0]
                : -1;
            current++;

            if (current >= listViewSnapshot.Items.Count)
            {
                current = listViewSnapshot.Items.Count - 1;
            }

            if (current >= 0 && current < listViewSnapshot.Items.Count)
            {
                listViewSnapshot.SelectedIndices.Clear();
                listViewSnapshot.SelectedIndices.Add(current);
                if (listViewSnapshot.SelectedItems.Count == 1)
                {
                    listViewSnapshot.SelectedItems[0].EnsureVisible();
                    listViewSnapshot.FocusedItem = listViewSnapshot.SelectedItems[0];
                }

                listViewSnapshot_SelectedIndexChanged(null, null);
            }
        }

        public void SnapshotSelectPrevious()
        {
            if (listViewSnapshot.Items.Count <= 0)
            {
                return;
            }

            int current = listViewSnapshot.SelectedIndices.Count > 0
                ? listViewSnapshot.SelectedIndices[0]
                : -1;
            current--;

            if (current < 0)
            {
                current = 0;
            }

            if (current < listViewSnapshot.Items.Count)
            {
                listViewSnapshot.SelectedIndices.Clear();
                listViewSnapshot.SelectedIndices.Add(current);
                if (listViewSnapshot.SelectedItems.Count == 1)
                {
                    listViewSnapshot.SelectedItems[0].EnsureVisible();
                    listViewSnapshot.FocusedItem = listViewSnapshot.SelectedItems[0];
                }

                listViewSnapshot_SelectedIndexChanged(null, null);
            }
        }

        internal DialogResult Message(string text, string caption, MessageType type, MessageMode mode)
        {
            var dialogResult = DialogResult.None;

            if (InvokeRequired)
            {
                Invoke(new Action(() => { dialogResult = Message(text, caption, type, mode); }));
            }
            else
            {
                if (notifyIcon.Visible && !string.IsNullOrEmpty(text))
                {
                    switch (type)
                    {
                        case MessageType.Information:
                            ShowNotificationBalloon(@"Information", text, ToolTipIcon.Info);
                            break;
                        case MessageType.Warning:
                            ShowNotificationBalloon(@"Warning", text, ToolTipIcon.Warning);
                            break;
                        case MessageType.Error:
                            ShowNotificationBalloon(@"Error", text, ToolTipIcon.Error);
                            break;
                    }

                    return dialogResult;
                }

                if ((mode == MessageMode.User && _settingsManager.NotificationMode == MessageMode.MessageBox) ||
                    mode == MessageMode.MessageBox)
                {
                    if (type != MessageType.None)
                    {
                        MessageBoxButtons button = MessageBoxButtons.OK;
                        MessageBoxIcon icon = MessageBoxIcon.Information;
                        switch (type)
                        {
                            case MessageType.Error:
                                button = MessageBoxButtons.OK;
                                icon = MessageBoxIcon.Error;
                                break;
                            case MessageType.Information:
                                button = MessageBoxButtons.OK;
                                icon = MessageBoxIcon.Information;
                                break;
                            case MessageType.Question:
                                button = MessageBoxButtons.YesNoCancel;
                                icon = MessageBoxIcon.Question;
                                break;
                            case MessageType.Warning:
                                button = MessageBoxButtons.OK;
                                icon = MessageBoxIcon.Warning;
                                break;
                        }

                        dialogResult = MessageBox.Show(text, caption, button, icon);
                    }
                }

                if ((mode == MessageMode.User && _settingsManager.NotificationMode == MessageMode.Status) || mode == MessageMode.Status)
                {
                    toolStripStatus.Image = null;

                    switch (type)
                    {
                        case MessageType.Error:
                            SetToolStripStatusStyle(toolStripStatus, IconChar.TimesCircle, Color.Red);
                            break;
                        case MessageType.Information:
                            SetToolStripStatusStyle(toolStripStatus, IconChar.ExclamationCircle, Color.Blue);
                            break;
                        case MessageType.Question:
                            SetToolStripStatusStyle(toolStripStatus, IconChar.QuestionCircle, Color.Blue);
                            break;
                        case MessageType.Warning:
                            SetToolStripStatusStyle(toolStripStatus, IconChar.ExclamationTriangle, Color.Orange);
                            break;
                    }

                    toolStripStatus.Text = text;
                }
            }

            return dialogResult;
        }

        internal void RefreshSnapshotsListView(ListView listView, EngineSnapshotStatus status, EngineSnapshotCategory category)
        {
            if (_luaManager == null)
            {
                return;
            }

            listView.BeginUpdate();

            try
            {
                int selected = -1;

                if (listView.SelectedItems.Count > 0)
                {
                    selected = listView.SelectedIndices[0];
                }

                listView.Items.Clear();

                foreach (var snapshot in _luaManager.ActiveEngine.Snapshots.Where(snapshot =>
                             category == null || category.Id == 0 || snapshot.CategoryId == category.Id))
                {
                    var listViewItem = new ListViewItem();
                    bool first = true;

                    foreach (var columnDefinition in _luaManager.ActiveEngine.SnapshotColumnsDefinition.OrderBy(c => c.Order))
                    {
                        string value = "";
                        switch (columnDefinition.Key)
                        {
                            case "SavedAt":
                                value = snapshot.SavedAt.ToString(snapshot.SavedAtToStringFormat);
                                break;
                            case "Notes":
                                value = snapshot.Notes?.Split('\n').FirstOrDefault();
                                break;
                            default:
                                if (snapshot.CustomValues.TryGetValue(columnDefinition.Key, out var snapshotCustomValue))
                                {
                                    value = snapshotCustomValue.ToString();
                                }

                                break;
                        }

                        if (first)
                        {
                            listViewItem.Text = value;
                            first = false;
                        }
                        else
                        {
                            listViewItem.SubItems.Add(value);
                        }
                    }

                    listViewItem.Tag = snapshot;

                    if (snapshot.Status == status)
                    {
                        listView.Items.Add(listViewItem);
                    }
                }

                // Auto Select Last Snapshot
                if (status == EngineSnapshotStatus.Active)
                {
                    AutoSelectLastSnapshot(listView);
                }

                AutoResizeColumns(listView);

                ListViewSetSelected(listView, selected);
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        internal void SetSnapshotDetails(ListView listView, EngineSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            listView.Enabled = true;
            listView.BeginUpdate();
            try
            {
                listView.Items.Clear();

                var names = new List<string>();

                foreach (var columnDefinition in _luaManager.ActiveEngine.SnapshotColumnsDefinition.OrderBy(c => c.Order))
                {
                    ListViewItem listViewItem = null;
                    switch (columnDefinition.Key)
                    {
                        case "SavedAt":
                            listViewItem = CreateItem("Saved At",
                                snapshot.SavedAt.ToString(snapshot.SavedAtToStringFormat));
                            break;
                        case "Notes":
                            listViewItem = CreateItem("Notes", snapshot.Notes ?? "");
                            break;
                        default:
                            if (snapshot.CustomValues.TryGetValue(columnDefinition.Key, out var customValue))
                            {
                                if (customValue.ShowInDetails)
                                {
                                    names.Add(columnDefinition.Key);
                                    if (string.IsNullOrEmpty(customValue.Caption))
                                    {
                                        listViewItem = CreateItem(columnDefinition.Key, customValue.ToString());
                                    }
                                    else
                                    {
                                        listViewItem = CreateItem(customValue.Caption, customValue.ToString());
                                    }
                                }
                            }

                            break;
                    }

                    if (listViewItem != null)
                    {
                        listView.Items.Add(listViewItem);
                    }
                }

                foreach (var pair in snapshot.CustomValues.Where(pair => !names.Contains(pair.Key)))
                {
                    names.Add(pair.Key);
                    ListViewItem listViewItem = null;

                    var custom = pair.Value;
                    if (custom.ShowInDetails)
                    {
                        if (string.IsNullOrEmpty(custom.Caption))
                        {
                            listViewItem = CreateItem(pair.Key, custom.ToString());
                        }
                        else
                        {
                            listViewItem = CreateItem(custom.Caption, custom.ToString());
                        }
                    }

                    if (listViewItem == null)
                    {
                        continue;
                    }

                    names.Add(listViewItem.Text);
                    listView.Items.Add(listViewItem);
                }

                AutoResizeColumns(listView);
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private bool ActionSnapshotBackup(ActionSource actionSource)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return false;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (_luaManager.ActiveEngine.ActionSnapshotBackup(actionSource))
                    {
                        Message("The backup has been successful", "Backup Successful", MessageType.Information, MessageMode.User);
                        return true;
                    }

                    Message("The backup failed :(", "Hmm :(", MessageType.Error, MessageMode.User);
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }

            return false;
        }

        private bool ActionSnapshotDelete(ActionSource actionSource)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return false;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                EngineSnapshot snapshot = GetSelectedActiveSnapshot();
                if (snapshot != null)
                {
                    if (snapshot.Status == EngineSnapshotStatus.Active)
                    {
                        snapshot.Status = EngineSnapshotStatus.Deleted;

                        _luaManager.ActiveEngine.SnapshotsChanges();

                        Message("The deletion has been successful", "Deletion Complete", MessageType.Information, MessageMode.User);
                        return true;
                    }
                }
                else
                {
                    Message("Nothing to delete", "Nop!", MessageType.Warning, MessageMode.User);
                    return false;
                }
            }

            Message("The deletion failed :(", "Hmm :(", MessageType.Error, MessageMode.User);
            return false;
        }

        private bool ActionSnapshotRestore(ActionSource actionSource)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return false;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                EngineSnapshot snapshot = GetSelectedActiveSnapshot();
                if (snapshot != null)
                {
                    lock (_restoreSaveLock)
                    {
                        if (_watcherManager != null && (_watcherManager.AutoBackupStatus == AutoBackupStatus.Enabled ||
                                                        _watcherManager.AutoBackupStatus == AutoBackupStatus.Waiting))
                        {
                            _watcherManager.SetWatchers(false);
                            Thread.Sleep(250);
                        }

                        try
                        {
                            if (UnzipSnapshot(snapshot))
                            {
                                if (_luaManager.ActiveEngine.ActionSnapshotRestore(actionSource, snapshot))
                                {
                                    Message("The restoration has been successful", "Restoration Complete",
                                        MessageType.Information,
                                        MessageMode.User);
                                    return true;
                                }
                            }
                        }
                        finally
                        {
                            if (_watcherManager != null && (_watcherManager.AutoBackupStatus == AutoBackupStatus.Enabled ||
                                                            _watcherManager.AutoBackupStatus == AutoBackupStatus.Waiting))
                            {
                                _watcherManager.SetWatchers(true);
                                Thread.Sleep(250);
                            }
                        }
                    }
                }
                else
                {
                    Message("Nothing to restore", "Nop!", MessageType.Warning, MessageMode.User);
                    return false;
                }

                Message("The restoration failed :(", "Hmm :(", MessageType.Error, MessageMode.User);
                return false;
            }

            return false;
        }

        private bool ActionSwitchAutoBackup()
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return false;
            }

            if (_watcherManager == null)
            {
                return false;
            }

            if (_autoBackupEnabled)
            {
                _autoBackupEnabled = false;
            }
            else
            {
                _autoBackupEnabled = true;
            }

            _watcherManager.SetAutoBackup(_autoBackupEnabled);

            SetAutoBackupMessage();

            Logger.Information("buttonActionAuto: AutoBackupStatus=", _watcherManager.AutoBackupStatus);

            return true;
        }

        private void ApplyStyle()
        {
            SetButtonStyle(buttonNewProject, IconChar.File, Color.Teal);
            SetButtonStyle(buttonOpenProject, IconChar.FolderOpen, Color.Gold);

            SetPictureBoxStyle(pictureBoxPinnedProjects, IconChar.MapPin, Color.RoyalBlue);
            SetPictureBoxStyle(pictureBoxRecentProjects, IconChar.Clock, Color.RoyalBlue);

            toolStripStatus.Text = "Ready";
            SetToolStripStatusStyle(toolStripStatus, IconChar.Cog, Color.Gray);

            toolStripHotKey.Text = "Hot Keys: Idle";
            SetToolStripHotKeyStyle(toolStripHotKey, IconChar.Circle, Color.Gray);

            toolStripAutoBackup.Text = "AutoBackup: Idle";
            SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.Circle, Color.Gray);
        }

        // Try to resize column using both header and data
        private void AutoResizeColumns(ListView listView)
        {
            listView.BeginUpdate();
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            var columnSize =
                listView.Columns.Cast<ColumnHeader>().ToDictionary(
                    column => column.Index,
                    column => column.Width);

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            foreach (ColumnHeader column in listView.Columns)
            {
                if (columnSize.TryGetValue(column.Index, out var width))
                {
                    column.Width = Math.Max(width, column.Width);
                }
            }

            listView.EndUpdate();
        }

        private void AutoSelectLastSnapshot(ListView listView)
        {
            if (!_settingsManager.AutoSelectLastSnapshot || _luaManager.ActiveEngine.LastSnapshot == null)
            {
                return;
            }

            foreach (ListViewItem item in listView.Items)
            {
                if (item.Tag != _luaManager.ActiveEngine.LastSnapshot)
                {
                    continue;
                }

                listView.SelectedIndices.Clear();
                SetSnapshotInfo(_luaManager.ActiveEngine.LastSnapshot);
                item.Selected = true;
                item.Focused = true;
                _luaManager.ActiveEngine.LastSnapshot = null;
                break;
            }
        }

        private void autoSelectLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.AutoSelectLastSnapshot = !_settingsManager.AutoSelectLastSnapshot;
            autoSelectLastToolStripMenuItem.Checked = _settingsManager.AutoSelectLastSnapshot;
        }

        private void buttonActionArchive_Click(object sender, EventArgs e)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (ListViewItem selectedItem in listViewSnapshot.SelectedItems)
                    {
                        if (selectedItem.Tag is EngineSnapshot snapshot && snapshot.Status == EngineSnapshotStatus.Active)
                        {
                            if (!snapshot.Compressed)
                            {
                                Message("Archiving snapshot " + snapshot + " ... Please wait.", "", MessageType.Information,
                                    MessageMode.Status);
                                ZipSnapshot(snapshot);
                            }

                            snapshot.Status = EngineSnapshotStatus.Archived;
                        }
                    }

                    _luaManager?.ActiveEngine?.SnapshotsChanges();
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    Message("Ready", "", MessageType.Information, MessageMode.Status);
                }
            }
            else if (tabControlSaves.SelectedTab == tabPageArchivedSaves)
            {
                // nothing to do here
            }
            else if (tabControlSaves.SelectedTab == tabPageDeletedSaves)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (ListViewItem selectedItem in listViewDeleted.SelectedItems)
                    {
                        if (selectedItem.Tag is EngineSnapshot snapshot)
                        {
                            if (snapshot.Status == EngineSnapshotStatus.Deleted)
                            {
                                if (!snapshot.Compressed)
                                {
                                    Message("Archiving snapshot " + snapshot + " ... Please wait.", "", MessageType.Information,
                                        MessageMode.Status);
                                    ZipSnapshot(snapshot);
                                }

                                snapshot.Status = EngineSnapshotStatus.Archived;
                            }
                        }
                    }

                    _luaManager?.ActiveEngine?.SnapshotsChanges();
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    Message("Ready", "", MessageType.Information, MessageMode.Status);
                }
            }
        }

        private void buttonActionAuto_Click(object sender, EventArgs e)
        {
            ActionSwitchAutoBackup();
        }

        private void buttonActionAuto_Paint(object sender, PaintEventArgs e)
        {
            Button btn = (Button)sender;

            Color color = Color.Gray;
            int border = 0;

            if (_watcherManager != null)
            {
                if (_autoBackupEnabled)
                {
                    switch (_watcherManager.AutoBackupStatus)
                    {
                        case AutoBackupStatus.Disabled:
                            color = Color.Gray;
                            border = 0;
                            break;
                        case AutoBackupStatus.Enabled:
                            color = Color.LimeGreen;
                            border = 5;
                            break;
                        case AutoBackupStatus.Waiting:
                            color = Color.Orange;
                            border = 5;
                            break;
                    }
                }
                else
                {
                    switch (_watcherManager.AutoBackupStatus)
                    {
                        case AutoBackupStatus.Disabled:
                            color = Color.Gray;
                            border = 0;
                            break;
                        case AutoBackupStatus.Enabled:
                            color = Color.LimeGreen;
                            border = 5;
                            break;
                        case AutoBackupStatus.Waiting:
                            color = Color.Orange;
                            border = 5;
                            break;
                    }
                }
            }

            ControlPaint.DrawBorder(e.Graphics, btn.ClientRectangle,
                color, border, ButtonBorderStyle.Solid,
                color, border, ButtonBorderStyle.Solid,
                color, border, ButtonBorderStyle.Solid,
                color, border, ButtonBorderStyle.Solid
            );
        }

        private void buttonActionBackup_Click(object sender, EventArgs e)
        {
            ActionSnapshotBackup(ActionSource.Button);
        }

        private void buttonActionDelete_Click(object sender, EventArgs e)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                foreach (ListViewItem selectedItem in listViewSnapshot.SelectedItems)
                {
                    if (selectedItem.Tag is EngineSnapshot snapshot)
                    {
                        if (snapshot.Status == EngineSnapshotStatus.Active)
                        {
                            snapshot.Status = EngineSnapshotStatus.Deleted;
                        }
                    }
                }

                _luaManager.ActiveEngine.SnapshotsChanges();
            }
            else if (tabControlSaves.SelectedTab == tabPageArchivedSaves)
            {
                foreach (ListViewItem selectedItem in listViewArchived.SelectedItems)
                {
                    if (selectedItem.Tag is EngineSnapshot snapshot)
                    {
                        if (snapshot.Status == EngineSnapshotStatus.Archived)
                        {
                            snapshot.Status = EngineSnapshotStatus.Deleted;
                        }
                    }
                }

                _luaManager.ActiveEngine.SnapshotsChanges();
            }
            else if (tabControlSaves.SelectedTab == tabPageDeletedSaves)
            {
                if (Message("All selected snapshots will be deleted permanently, do you want to continue?", "Confirm Nuke launch?",
                        MessageType.Question, MessageMode.MessageBox) != DialogResult.Yes)
                {
                    return;
                }

                foreach (ListViewItem selectedItem in listViewDeleted.SelectedItems)
                {
                    if (!(selectedItem.Tag is EngineSnapshot snapshot) || snapshot.Status != EngineSnapshotStatus.Deleted)
                    {
                        continue;
                    }

                    if (!SnapshotNuke(snapshot))
                    {
                        continue;
                    }

                    snapshot.Status = EngineSnapshotStatus.Nuked;
                    //_luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                }

                _luaManager.ActiveEngine.SnapshotsChanges();
            }
        }

        private void buttonActionRestore_Click(object sender, EventArgs e)
        {
            if (_luaManager?.ActiveEngine == null)
            {
                return;
            }

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                ActionSnapshotRestore(ActionSource.Button);
            }
            else if (tabControlSaves.SelectedTab == tabPageArchivedSaves)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (ListViewItem selectedItem in listViewArchived.SelectedItems)
                    {
                        if (selectedItem.Tag is EngineSnapshot snapshot)
                        {
                            if (snapshot.Status == EngineSnapshotStatus.Archived)
                            {
                                if (snapshot.Compressed)
                                {
                                    Message("Activating snapshot " + snapshot + " ... Please wait.", "", MessageType.Information,
                                        MessageMode.Status);
                                    UnzipSnapshot(snapshot);
                                }

                                snapshot.Status = EngineSnapshotStatus.Active;
                            }
                        }
                    }

                    _luaManager?.ActiveEngine?.SnapshotsChanges();
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    Message("Ready", "", MessageType.Information, MessageMode.Status);
                }
            }
            else if (tabControlSaves.SelectedTab == tabPageDeletedSaves)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (ListViewItem selectedItem in listViewDeleted.SelectedItems)
                    {
                        if (selectedItem.Tag is EngineSnapshot snapshot)
                        {
                            if (snapshot.Status == EngineSnapshotStatus.Deleted)
                            {
                                if (snapshot.Compressed)
                                {
                                    Message("Activating snapshot " + snapshot + " ... Please wait.", "", MessageType.Information,
                                        MessageMode.Status);
                                    UnzipSnapshot(snapshot);
                                }

                                snapshot.Status = EngineSnapshotStatus.Active;
                            }
                        }
                    }

                    _luaManager?.ActiveEngine?.SnapshotsChanges();
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    Message("Ready", "", MessageType.Information, MessageMode.Status);
                }
            }
        }

        private void buttonNewProject_Click(object sender, EventArgs e)
        {
            NewProfileDialog();
        }

        private void buttonOpenProject_Click(object sender, EventArgs e)
        {
            OpenProfileDialog();
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string responseString = null;
            string versionFormatted = $"{_version.Major}.{_version.Minor}.{_version.Build}";
            try
            {
                string url = "https://api.github.com/repos/HgAlexx/Hg.SaveHistory/releases/latest";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = $"Hg.SaveHistory/{versionFormatted} ({Environment.OSVersion}) By: HgAlexx";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        responseString = new StreamReader(stream).ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            if (!string.IsNullOrEmpty(responseString))
            {
                Version maxVersion = _version;
                try
                {
                    var obj = JObject.Parse(responseString);
                    string value = obj.Value<string>("tag_name");
                    value = value.Substring(1); // strip "v" prefix

                    if (value.Contains("_")) // strip "_beta" suffix
                    {
                        value = value.Substring(0, value.IndexOf("_", StringComparison.Ordinal));
                    }

                    var v = new Version(value);
                    if (v > _version)
                    {
                        if (v > maxVersion)
                        {
                            maxVersion = v;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }

                if (maxVersion > _version)
                {
                    if (Message("A new version is available, do you want to open the release page?",
                            "New version available!", MessageType.Question, MessageMode.MessageBox) == DialogResult.Yes)
                    {
                        Process.Start("https://github.com/HgAlexx/Hg.SaveHistory/releases");
                    }
                }
                else
                {
                    Message(@"No new version found", @"You are up-to-date", MessageType.Information,
                        MessageMode.MessageBox);
                }
            }
            else
            {
                Message(@"Unable to check for a new version, please try again later", @"Hmm :(",
                    MessageType.Information, MessageMode.MessageBox);
            }
        }

        private void CleanSnapshots()
        {
            if (_activeProfileFile == null || !_activeProfileFile.AutoCleanupBackup)
            {
                return;
            }

            SetProgressBarVisibility(true);

            // Oldest first
            var comparer = GetSnapshotComparer("SavedAt", SortOrder.Ascending);
            _luaManager.ActiveEngine.Snapshots.Sort(comparer);

            CleanupSnapshotsCore(
                _activeProfileFile.AutoCleanupBackupArchive,
                EngineSnapshotStatus.Active,
                EngineSnapshotStatus.Archived);
            CleanupSnapshotsCore(
                _activeProfileFile.AutoCleanupBackupDelete,
                EngineSnapshotStatus.Archived,
                EngineSnapshotStatus.Deleted);
            CleanupSnapshotsCore(
                _activeProfileFile.AutoCleanupBackupNuke,
                EngineSnapshotStatus.Deleted,
                EngineSnapshotStatus.Nuked);

            SetProgressBarVisibility(false);
        }

        private void CleanupSnapshotsCore(SettingsAutoCleanupBackup settings, EngineSnapshotStatus statusLookup,
            EngineSnapshotStatus statusTarget)
        {
            if (!settings.Enabled)
            {
                return;
            }

            Message($"Processing auto cleanup of {EngineSnapshotStatusToString(statusLookup)} snapshots...",
                "Working..",
                MessageType.Information,
                MessageMode.Status);

            var snapshots = _luaManager.ActiveEngine.Snapshots.Where(s => s.Status == statusLookup).ToList();
            foreach (var snapshot in snapshots)
            {
                string targetPath = Path.Combine(_luaManager.ActiveEngine.SnapshotsFolder, snapshot.RelativePath);
                var directoryInfo = new DirectoryInfo(targetPath);
                long size = 0;
                if (!snapshot.Compressed)
                {
                    size = directoryInfo.GetDirectorySize();
                }
                else
                {
                    if (directoryInfo.Parent != null)
                    {
                        string archiveName = directoryInfo.Name + ".zip";
                        string sourcePath = Path.Combine(directoryInfo.Parent.FullName, archiveName);
                        var fileInfo = new FileInfo(sourcePath);
                        size = fileInfo.Length;
                    }
                }

                snapshot.Size = size;

                if (settings.Modes.HasFlag(AutoCleanupMode.BySize))
                {
                    if (size > settings.Size)
                    {
                        snapshot.Status = statusTarget;
                        snapshots.Remove(snapshot);
                        if (statusTarget == EngineSnapshotStatus.Archived)
                        {
                            ZipSnapshot(snapshot);
                        }

                        if (statusTarget == EngineSnapshotStatus.Nuked)
                        {
                            SnapshotNuke(snapshot);
                            //_luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                        }
                    }
                }

                if (settings.Modes.HasFlag(AutoCleanupMode.ByAge))
                {
                    var timeSpan = DateTime.UtcNow - snapshot.SavedAt;
                    if (timeSpan.TotalMinutes > settings.Age?.TotalMinutes)
                    {
                        snapshot.Status = statusTarget;
                        snapshots.Remove(snapshot);
                        if (statusTarget == EngineSnapshotStatus.Archived)
                        {
                            ZipSnapshot(snapshot);
                        }

                        if (statusTarget == EngineSnapshotStatus.Nuked)
                        {
                            SnapshotNuke(snapshot);
                            //_luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                        }
                    }
                }
            }

            if (settings.Modes.HasFlag(AutoCleanupMode.ByCount))
            {
                void ProcessCountPerCategory(int? categoryId)
                {
                    while (snapshots.Count(s => categoryId == null || s.CategoryId == categoryId) > settings.Count)
                    {
                        var snapshot = snapshots.FirstOrDefault(s => categoryId == null || s.CategoryId == categoryId);
                        if (snapshot == null)
                        {
                            continue;
                        }

                        snapshot.Status = statusTarget;
                        snapshots.Remove(snapshot);
                        if (statusTarget == EngineSnapshotStatus.Archived)
                        {
                            ZipSnapshot(snapshot);
                        }

                        if (statusTarget == EngineSnapshotStatus.Nuked)
                        {
                            SnapshotNuke(snapshot);
                            //_luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                        }
                    }
                }

                if (settings.PerCategory)
                {
                    ProcessCountPerCategory(0);
                    foreach (var category in _luaManager.ActiveEngine.Categories)
                    {
                        if (category.Id != 0)
                        {
                            ProcessCountPerCategory(category.Id);
                        }
                    }
                }
                else
                {
                    ProcessCountPerCategory(null);
                }
            }

            if (settings.Modes.HasFlag(AutoCleanupMode.ByTotalSize))
            {
                void ProcessTotalSizePerCategory(int? categoryId)
                {
                    while (snapshots.Where(s => categoryId == null || s.CategoryId == categoryId).Sum(snapshot => snapshot.Size) >=
                           settings.TotalSize)
                    {
                        var snapshot = snapshots.FirstOrDefault(s => categoryId == null || s.CategoryId == categoryId);
                        if (snapshot == null)
                        {
                            continue;
                        }

                        snapshot.Status = statusTarget;
                        snapshots.Remove(snapshot);
                        if (statusTarget == EngineSnapshotStatus.Archived)
                        {
                            ZipSnapshot(snapshot);
                        }

                        if (statusTarget == EngineSnapshotStatus.Nuked)
                        {
                            SnapshotNuke(snapshot);
                            //_luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                        }
                    }
                }

                if (settings.PerCategory)
                {
                    ProcessTotalSizePerCategory(0);
                    foreach (var category in _luaManager.ActiveEngine.Categories)
                    {
                        if (category.Id != 0)
                        {
                            ProcessTotalSizePerCategory(category.Id);
                        }
                    }
                }
                else
                {
                    ProcessTotalSizePerCategory(null);
                }
            }

            Message("",
                "",
                MessageType.Information,
                MessageMode.Status);
        }

        private void clearSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Message("Are you sure you want to reset all global settings?", "Reset global settings?",
                    MessageType.Question, MessageMode.MessageBox) == DialogResult.Yes)
            {
                ReleaseHotKeysHook();

                _settingsManager.ResetSettings();
                _settingsManager.SaveSettings();

                CreateHotKeysHook();
                InitHotKeys();
            }
        }

        private void CloseProfile()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (_activeProfileFile == null)
            {
                return;
            }

            if (_luaManager == null)
            {
                return;
            }

            ReleaseWatcher();

            _selectedSnapshot = null;

            if (_luaManager.ActiveEngine?.OnClosing?.Call().First() is bool b && b)
            {
                SaveProfile();
                _luaManager.ActiveEngine?.OnClosed?.Call();
            }

            foreach (ColumnHeader column in listViewSnapshot.Columns)
            {
                column.Tag = null;
            }

            foreach (ColumnHeader column in listViewArchived.Columns)
            {
                column.Tag = null;
            }

            foreach (ColumnHeader column in listViewDeleted.Columns)
            {
                column.Tag = null;
            }

            profileToolStripMenuItem.Enabled = false;

            listViewSnapshot.Clear();
            listViewArchived.Clear();
            listViewDeleted.Clear();

            comboBoxCategories.Items.Clear();

            foreach (Control control in flowLayoutPanelConfig.Controls)
            {
                if (control is CheckBox c)
                {
                    c.Tag = null;
                }
            }

            flowLayoutPanelConfig.Controls.Clear();

            _luaManager.Release();
            _luaManager = null;

            _activeProfileFile.Release();
            _activeProfileFile = null;

            tabPageProfile.Text = "";

            ReleaseHotKeysHook();

            HideProfilePage();
            HideReadMePage();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Logger.Information("---------------------------------Close---------------------------------");
        }

        private void closeProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseProfile();
        }

        private void comboBoxCategories_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxCategories.SelectedItem is EngineSnapshotCategory selection)
            {
                _activeProfileFile.LastSelectedCategory = selection.Id;
            }

            RefreshSnapshotLists();
        }

        private bool ComputeSnapDelta(int pos, int edge)
        {
            int delta = Math.Abs(pos - edge);

            return delta > 0 && delta <= 10;
        }

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettingsAutoCleanup formSettings = new FormSettingsAutoCleanup(
                _activeProfileFile.AutoCleanupBackupArchive,
                _activeProfileFile.AutoCleanupBackupDelete,
                _activeProfileFile.AutoCleanupBackupNuke);
            if (formSettings.ShowDialog(this) == DialogResult.OK)
            {
                SaveProfile();
            }
        }

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            contextMenu.Hide();

            if (e.ClickedItem == toolStripMenuItemRestore)
            {
                buttonActionRestore_Click(null, null);
            }

            if (e.ClickedItem == toolStripMenuItemArchive)
            {
                buttonActionArchive_Click(null, null);
            }

            if (e.ClickedItem == toolStripMenuItemDelete || e.ClickedItem == toolStripMenuItemNuke)
            {
                buttonActionDelete_Click(null, null);
            }
        }

        private void CreateHotKeysHook()
        {
            _hotKeysManager = new HotKeysManager();
            _hotKeysManager.KeyDown += OnKeyDown;
            _hotKeysManager.KeyUp += OnKeyUp;
        }

        private ListViewItem CreateItem(string name, string value)
        {
            ListViewItem item = new ListViewItem();

            if (name == null)
            {
                name = "";
            }

            if (value == null)
            {
                value = "";
            }

            item.Text = name;
            if (value.Contains(Environment.NewLine))
            {
                value = value.Replace(Environment.NewLine, " ");
            }

            item.SubItems.Add(value);

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(value))
            {
                return null;
            }

            return item;
        }

        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_debugConsole == null)
            {
                _debugConsole = new FormDebugConsole();
                _debugConsole.Show();
            }

            if (!_debugConsole.Visible)
            {
                _debugConsole.Visible = true;
            }
        }

        private void enabledCleanupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _activeProfileFile.AutoCleanupBackup = !_activeProfileFile.AutoCleanupBackup;
            autoCleanupBackupToolStripMenuItem.Checked = _activeProfileFile.AutoCleanupBackup;
            enabledCleanupToolStripMenuItem.Checked = _activeProfileFile.AutoCleanupBackup;
        }

        private void enabledHotKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.HotKeysActive = !_settingsManager.HotKeysActive;
        }

        private string EngineSnapshotStatusToString(EngineSnapshotStatus status)
        {
            switch (status)
            {
                case EngineSnapshotStatus.Active:
                    return "Active";
                case EngineSnapshotStatus.Archived:
                    return "Archived";
                case EngineSnapshotStatus.Deleted:
                    return "Deleted";
                case EngineSnapshotStatus.Nuked:
                    return "Nuked";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _settingsManager.SaveSettings();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseProfile();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            _loading = true;

            string versionFormatted = $"v{_version.Major}.{_version.Minor}.{_version.Build}";
            Text += @" " + versionFormatted;
#if DEBUG
            Text += @" - Development";
#endif

            notifyIcon.Text = Text;

            _settingsManager.DetectAndLoadFile();

            // Get default size and location
            Point defaultLocation = Location;
            Size defaultSize = Size;

            // Get location offset
            Location = new Point(0, 0);
            Point p = PointToScreen(new Point(0, 0));
            _borderOffset = p.X;

            Location = defaultLocation;

            if (_settingsManager.SaveSizeAndPosition)
            {
                // Restore location from settings
                if (_settingsManager.Location != null)
                {
                    Location = (Point)_settingsManager.Location;
                }

                // Restore size from settings
                if (_settingsManager.Size != null)
                {
                    Size = (Size)_settingsManager.Size;
                }
            }

            if (!IsOnScreen(this))
            {
                // Reset size and location to default if window if out of screen
                Location = defaultLocation;
                Size = defaultSize;
            }

            SoundManager.PreLoad();

#if DEBUG
            if (Debugger.IsAttached)
            {
                // debugConsoleToolStripMenuItem_Click(null, null);
            }
#endif

            if (_settingsManager.OpenLastUsedProfileOnStartup && !string.IsNullOrEmpty(_settingsManager.LastUsedProfilePath))
            {
                OpenProfile(_settingsManager.LastUsedProfilePath, true);
            }

            _loading = false;

            if (_settingsManager.StartMinimized)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            if (_settingsManager == null || !_settingsManager.MinimizedToTray)
            {
                return;
            }

            notifyIcon.Visible = true;
            BeginInvoke(new MethodInvoker(Hide));

            ShowNotificationBalloon(@"Information", @"Hg.SaveHistory has been minimized to tray.", ToolTipIcon.Info);
        }

        private void FormMain_ResizeBegin(object sender, EventArgs e)
        {
            tabControlMain?.SelectedTab?.SuspendLayout();
        }

        private void FormMain_ResizeEnd(object sender, EventArgs e)
        {
            try
            {
                if (_loading)
                {
                    return;
                }

                if (_settingsManager == null)
                {
                    return;
                }

                if (_settingsManager.SnapToScreenEdges)
                {
                    // Get working area of screen containing most of the form
                    var workingArea = Screen.GetWorkingArea(this);

                    if (ComputeSnapDelta(Left + _borderOffset, workingArea.Left))
                    {
                        Left = workingArea.Left - _borderOffset;
                    }

                    if (ComputeSnapDelta(Top, workingArea.Top))
                    {
                        Top = workingArea.Top;
                    }

                    if (ComputeSnapDelta(Right - _borderOffset, workingArea.Right))
                    {
                        Left = workingArea.Right - Width + _borderOffset;
                    }

                    if (ComputeSnapDelta(Bottom - _borderOffset, workingArea.Bottom))
                    {
                        Top = workingArea.Bottom - Height + _borderOffset;
                    }
                }

                if (_settingsManager.SaveSizeAndPosition)
                {
                    if (_settingsManager.Location != Location)
                    {
                        _settingsManager.Location = Location;
                    }

                    if (_settingsManager.Size != Size)
                    {
                        _settingsManager.Size = Size;
                    }
                }
            }
            finally
            {
                tabControlMain?.SelectedTab?.ResumeLayout();
            }
        }


        private EngineSnapshot GetSelectedActiveSnapshot()
        {
            if (listViewSnapshot.SelectedItems.Count == 1)
            {
                var selectedItem = listViewSnapshot.SelectedItems[0];
                if (selectedItem.Tag is EngineSnapshot snapshot)
                {
                    return snapshot;
                }
            }

            return null;
        }

        private static Comparison<EngineSnapshot> GetSnapshotComparer(string sortKey, SortOrder sortOrder)
        {
            Comparison<EngineSnapshot> comparer;
            switch (sortKey)
            {
                case "SavedAt":
                    comparer = (s1, s2) =>
                    {
                        if (s1 == null && s2 == null)
                        {
                            return 0;
                        }

                        if (s1 == null)
                        {
                            return -1;
                        }

                        if (s2 == null)
                        {
                            return 1;
                        }

                        if (sortOrder == SortOrder.Ascending)
                        {
                            return DateTime.Compare(s1.SavedAt, s2.SavedAt);
                        }

                        return DateTime.Compare(s2.SavedAt, s1.SavedAt);
                    };
                    break;
                case "Notes":
                    comparer = (s1, s2) =>
                    {
                        if (s1 == null && s2 == null)
                        {
                            return 0;
                        }

                        if (s1 == null)
                        {
                            return -1;
                        }

                        if (s2 == null)
                        {
                            return 1;
                        }

                        if (sortOrder == SortOrder.Ascending)
                        {
                            return string.CompareOrdinal(s1.Notes ?? "", s2.Notes ?? "");
                        }

                        return string.CompareOrdinal(s2.Notes ?? "", s1.Notes ?? "");
                    };
                    break;
                default:
                    comparer = (s1, s2) =>
                    {
                        if (s1 == null && s2 == null)
                        {
                            return 0;
                        }

                        if (s1 == null)
                        {
                            return -1;
                        }

                        if (s2 == null)
                        {
                            return 1;
                        }

                        EngineSnapshotCustomValueBase c1 = null;
                        if (s1.CustomValues.TryGetValue(sortKey, out var s1CustomValue))
                        {
                            c1 = s1CustomValue;
                        }

                        EngineSnapshotCustomValueBase c2 = null;
                        if (s2.CustomValues.TryGetValue(sortKey, out var s2CustomValue))
                        {
                            c2 = s2CustomValue;
                        }

                        if (c1 == null && c2 == null)
                        {
                            return 0;
                        }

                        int result;
                        if (c1 == null)
                        {
                            result = -1;
                        }
                        else if (c2 == null)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = c1.CompareTo(c2);
                        }

                        if (sortOrder == SortOrder.Ascending)
                        {
                            return result;
                        }

                        return -result;
                    };

                    break;
            }

            return comparer;
        }

        private void giflowSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.ScreenshotQuality = ScreenshotQuality.Gif;
        }

        private void HideProfilePage()
        {
            tabControlMain.Controls.Remove(tabPageProfile);
        }

        private void HideReadMePage()
        {
            richTextBoxUsage.Text = "";
            if (tabControlMain.Controls.Contains(tabPageUsage))
            {
                tabControlMain.Controls.Remove(tabPageUsage);
            }
        }

        private void highlightColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog
            {
                AllowFullOpen = true, AnyColor = true, FullOpen = true, Color = _settingsManager.HighlightSelectedSnapshotColor
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _settingsManager.HighlightSelectedSnapshotColor = colorDialog.Color;
            }
        }

        private void highlightSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.HighlightSelectedSnapshot = !_settingsManager.HighlightSelectedSnapshot;
            highlightSelectedToolStripMenuItem.Checked = _settingsManager.HighlightSelectedSnapshot;
        }

        private void InitHotKeys()
        {
            if (_hotKeysManager == null || _settingsManager == null)
            {
                return;
            }

            _hotKeysManager.HotKeys.Clear();
            foreach (HotKeyToAction hotKeyToAction in _settingsManager.HotKeyToActions)
            {
                _hotKeysManager.HotKeys.Add(hotKeyToAction);
            }

            if (_settingsManager.HotKeysActive)
            {
                _hotKeysManager.Hook();
                toolStripHotKey.Text = "Hot Keys: Enabled";
                SetToolStripHotKeyStyle(toolStripHotKey, IconChar.CheckCircle, Color.LimeGreen);
            }
            else
            {
                toolStripHotKey.Text = "Hot Keys: Disabled";
                SetToolStripHotKeyStyle(toolStripHotKey, IconChar.TimesCircle, Color.Red);
            }
        }

        private void InitWatcher()
        {
            if (_luaManager?.ActiveEngine?.Watcher == null)
            {
                return;
            }

            _watcherManager = new WatcherManager(_luaManager.ActiveEngine.Watcher)
            {
                IsProcessRunning = () => GetProcessPtr() != IntPtr.Zero
            };

            _watcherManager.AutoBackupStatusChanged += () =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(SetAutoBackupMessage));
                }
                else
                {
                    SetAutoBackupMessage();
                }
            };
        }

        private void jpgmediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.ScreenshotQuality = ScreenshotQuality.Jpg;
        }

        private void LinkLabelItemOnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is LinkLabel link)
            {
                string path = link.Tag as string;

                if (_activeProfileFile == null || _activeProfileFile.FilePath != path)
                {
                    Logger.Log("Open profile by Link click, path: " + path, LogLevel.Debug);
                    OpenProfile(path);
                }
                else
                {
                    Logger.Log("Open profile by Link click, already open: " + path, LogLevel.Debug);
                    tabControlMain.SelectedTab = tabPageProfile;
                }
            }
        }

        private void listViewArchived_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listViewArchived.Columns[e.Column].Tag is EngineSnapshotColumnDefinition column)
            {
                UpdateSortOrder(column.Key);
                SetComparer();
                SortSnapshots();
                RefreshSnapshotLists();
            }
        }

        private void listViewArchived_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonActionRestore.Enabled = false;
            buttonActionArchive.Enabled = false;
            buttonActionDelete.Enabled = false;

            contextMenu.Enabled = false;

            toolStripMenuItemRestore.Text = "Active";
            toolStripMenuItemRestore.Visible = true;
            toolStripMenuItemArchive.Visible = true;
            toolStripMenuItemDelete.Visible = true;
            toolStripMenuItemNuke.Visible = false;

            toolStripMenuItemRestore.Enabled = false;
            toolStripMenuItemArchive.Enabled = false;
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemNuke.Enabled = false;

            if (listViewArchived.Items.Count > 0)
            {
                contextMenu.Enabled = true;

                if (listViewArchived.SelectedIndices.Count == 1)
                {
                    SetSnapshotInfo(listViewArchived.SelectedItems[0].Tag as EngineSnapshot);
                }
                else
                {
                    SetSnapshotInfo(null);
                }

                if (listViewArchived.SelectedIndices.Count >= 1)
                {
                    buttonActionRestore.Enabled = true;
                    buttonActionDelete.Enabled = true;

                    toolStripMenuItemRestore.Enabled = true;
                    toolStripMenuItemDelete.Enabled = true;
                }
            }
        }

        private void listViewDeleted_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listViewDeleted.Columns[e.Column].Tag is EngineSnapshotColumnDefinition column)
            {
                UpdateSortOrder(column.Key);
                SetComparer();
                SortSnapshots();
                RefreshSnapshotLists();
            }
        }

        private void listViewDeleted_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonActionRestore.Enabled = false;
            buttonActionArchive.Enabled = false;
            buttonActionDelete.Enabled = false;

            contextMenu.Enabled = false;

            toolStripMenuItemRestore.Text = "Active";
            toolStripMenuItemRestore.Visible = true;
            toolStripMenuItemArchive.Visible = true;
            toolStripMenuItemDelete.Visible = false;
            toolStripMenuItemNuke.Visible = true;

            toolStripMenuItemRestore.Enabled = false;
            toolStripMenuItemArchive.Enabled = false;
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemNuke.Enabled = false;

            if (listViewDeleted.Items.Count > 0)
            {
                contextMenu.Enabled = true;

                if (listViewDeleted.SelectedIndices.Count == 1)
                {
                    SetSnapshotInfo(listViewDeleted.SelectedItems[0].Tag as EngineSnapshot);
                }
                else
                {
                    SetSnapshotInfo(null);
                }

                if (listViewDeleted.SelectedIndices.Count >= 1)
                {
                    buttonActionRestore.Enabled = true;
                    buttonActionArchive.Enabled = true;
                    buttonActionDelete.Enabled = true;

                    toolStripMenuItemRestore.Enabled = true;
                    toolStripMenuItemArchive.Enabled = true;
                    toolStripMenuItemNuke.Enabled = true;
                }
            }
        }

        private void ListViewSetSelected(ListView listView, int selected)
        {
            if (listView.Items.Count > 0 && listView.SelectedItems.Count == 0)
            {
                if (selected < 0)
                {
                    selected = 0;
                }

                if (selected >= listView.Items.Count)
                {
                    selected = listView.Items.Count - 1;
                }

                listView.SelectedIndices.Add(selected);
            }

            if (listView.SelectedItems.Count == 1)
            {
                listView.SelectedItems[0].EnsureVisible();
            }
            else
            {
                SetSnapshotInfo(null);
            }
        }

        private void listViewSnapshot_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listViewSnapshot.Columns[e.Column].Tag is EngineSnapshotColumnDefinition column)
            {
                UpdateSortOrder(column.Key);
                SetComparer();
                SortSnapshots();
                RefreshSnapshotLists();
            }
        }

        private void listViewSnapshot_SelectedIndexChanged(object sender, EventArgs e)
        {
            contextMenu.Enabled = false;

            toolStripMenuItemRestore.Text = "Restore";
            toolStripMenuItemRestore.Visible = true;
            toolStripMenuItemArchive.Visible = true;
            toolStripMenuItemDelete.Visible = true;
            toolStripMenuItemNuke.Visible = false;

            toolStripMenuItemRestore.Enabled = false;
            toolStripMenuItemArchive.Enabled = false;
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemNuke.Enabled = false;

            if (listViewSnapshot.Items.Count > 0)
            {
                contextMenu.Enabled = true;

                if (listViewSnapshot.SelectedIndices.Count == 1)
                {
                    buttonActionRestore.Enabled = true;
                    toolStripMenuItemRestore.Enabled = true;
                    SetSnapshotInfo(listViewSnapshot.SelectedItems[0].Tag as EngineSnapshot);
                }
                else
                {
                    buttonActionRestore.Enabled = false;
                    SetSnapshotInfo(null);
                }

                if (listViewSnapshot.SelectedIndices.Count >= 1)
                {
                    buttonActionArchive.Enabled = true;
                    buttonActionDelete.Enabled = true;

                    toolStripMenuItemArchive.Enabled = true;
                    toolStripMenuItemDelete.Enabled = true;
                }

                if (_settingsManager.HighlightSelectedSnapshot)
                {
                    listViewSnapshot.HideSelection = true;

                    listViewSnapshot.Items.Cast<ListViewItem>()
                        .ToList().ForEach(item =>
                        {
                            item.BackColor = listViewSnapshot.BackColor;
                            item.ForeColor = listViewSnapshot.ForeColor;
                        });

                    listViewSnapshot.SelectedItems.Cast<ListViewItem>()
                        .ToList().ForEach(item =>
                        {
                            item.BackColor = _settingsManager.HighlightSelectedSnapshotColor;
                            item.ForeColor = listViewSnapshot.ForeColor;
                        });
                }
                else
                {
                    listViewSnapshot.HideSelection = false;
                }
            }
            else
            {
                buttonActionRestore.Enabled = false;
                buttonActionArchive.Enabled = false;
                buttonActionDelete.Enabled = false;
            }
        }

        private bool LoadProfile(ProfileFile profileFile, EngineScript engineScript)
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            _luaManager = new LuaManager();
            _luaManager.LoadEngineAndProfile(engineScript, profileFile);

            _luaManager.ActiveEngine.OnOpened?.Call();

            _sortOrder = profileFile.SortOrder;
            _sortKey = profileFile.SortKey;

            SetComparer();

            // Load configuration controls
            foreach (var setting in _luaManager.ActiveEngine.Settings.Where(s => s.Kind == EngineSettingKind.Runtime).OrderBy(s => s.Index))
            {
                switch (setting)
                {
                    case EngineSettingCombobox settingCombobox:
                        // todo
                        break;
                    case EngineSettingFolderBrowser settingFolder:
                        // todo
                        break;
                    case EngineSettingCheckbox settingCheckbox:
                    {
                        var checkBox = new CheckBox
                            { AutoSize = false, Text = settingCheckbox.Caption, Checked = settingCheckbox.Value, Tag = settingCheckbox };

                        checkBox.CheckedChanged += (sender, args) =>
                        {
                            if (!(checkBox.Tag is EngineSettingCheckbox sc))
                            {
                                return;
                            }

                            sc.Value = checkBox.Checked;
                        };

                        toolTipHelp.SetToolTip(checkBox, settingCheckbox.HelpTooltip);
                        flowLayoutPanelConfig.Controls.Add(checkBox);
                        checkBox.AutoSize = true;
                        break;
                    }
                }
            }

            // Load list columns
            listViewSnapshot.BeginUpdate();
            listViewArchived.BeginUpdate();
            listViewDeleted.BeginUpdate();

            listViewSnapshot.Columns.Clear();
            listViewArchived.Columns.Clear();
            listViewDeleted.Columns.Clear();

            foreach (var columnDefinition in _luaManager.ActiveEngine.SnapshotColumnsDefinition.OrderBy(c => c.Order))
            {
                listViewSnapshot.Columns.Add(columnDefinition.HeaderText).Tag = columnDefinition;
                listViewArchived.Columns.Add(columnDefinition.HeaderText).Tag = columnDefinition;
                listViewDeleted.Columns.Add(columnDefinition.HeaderText).Tag = columnDefinition;
            }

            listViewSnapshot.EndUpdate();
            listViewArchived.EndUpdate();
            listViewDeleted.EndUpdate();

            listViewSnapshot.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewArchived.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewDeleted.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            _luaManager.ActiveEngine.OnCategoriesChanges += () =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(RefreshCategories));
                }
                else
                {
                    RefreshCategories();
                }
            };

            _luaManager.ActiveEngine.OnSnapshotsChanges += () =>
            {
                CleanSnapshots();

                SortSnapshots();

                if (InvokeRequired)
                {
                    Invoke(new Action(RefreshSnapshotLists));
                }
                else
                {
                    RefreshSnapshotLists();
                }

                SaveProfile();
            };

            _luaManager.ActiveEngine.OnAutoBackupOccurred += success =>
            {
                if (_settingsManager.AutoBackupSoundNotification)
                {
                    if (success)
                    {
                        SoundManager.PlaySuccess();
                    }
                    else
                    {
                        SoundManager.PlayError();
                    }
                }
            };

            _luaManager.ActiveEngine.OnInitialized?.Call();

            return true;
        }

        private void messageBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.NotificationMode = MessageMode.MessageBox;
            messageBoxToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.MessageBox;
            statusBarToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.Status;
        }

        private void minimizedToTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.MinimizedToTray = !_settingsManager.MinimizedToTray;
        }

        private void NewProfileDialog()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            // Create new profile
            FormWizardNewProfile form = new FormWizardNewProfile(_engineScriptManager);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                string profileFilename = form.ProfileFileFullName;
                if (File.Exists(profileFilename))
                {
                    _settingsManager.AddRecentProfiles(profileFilename);

                    OpenProfile(profileFilename);
                }
            }
        }

        private void newProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewProfileDialog();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon.Visible = false;
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void OnKeyDown(object sender, KeyEventArgs e, HotKeyToAction hotKeyToAction)
        {
            if (_settingsManager == null || _luaManager == null || _activeProfileFile == null)
            {
                return;
            }

            if (!_settingsManager.HotKeysActive)
            {
                return;
            }

            Logger.Information("Received HotKey: ", hotKeyToAction.Action);

            switch (hotKeyToAction.Action)
            {
                case HotKeyAction.CategoryPrevious:
                    CategorySelectPrevious();
                    e.Handled = true;
                    break;
                case HotKeyAction.CategoryNext:
                    CategorySelectNext();
                    e.Handled = true;
                    break;

                case HotKeyAction.SnapshotFirst:
                    SnapshotSelectActiveTab();
                    SnapshotSelectFirst();
                    e.Handled = true;
                    break;
                case HotKeyAction.SnapshotLast:
                    SnapshotSelectActiveTab();
                    SnapshotSelectLast();
                    e.Handled = true;
                    break;
                case HotKeyAction.SnapshotPrevious:
                    SnapshotSelectActiveTab();
                    SnapshotSelectPrevious();
                    e.Handled = true;
                    break;
                case HotKeyAction.SnapshotNext:
                    SnapshotSelectActiveTab();
                    SnapshotSelectNext();
                    e.Handled = true;
                    break;

                case HotKeyAction.SnapshotRestore:
                    SnapshotSelectActiveTab();

                    if (ActionSnapshotRestore(ActionSource.HotKey))
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlaySuccess();
                        }
                    }
                    else
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlayError();
                        }
                    }

                    e.Handled = true;
                    break;
                case HotKeyAction.SnapshotDelete:
                    SnapshotSelectActiveTab();

                    if (ActionSnapshotDelete(ActionSource.HotKey))
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlaySuccess();
                        }
                    }
                    else
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlayError();
                        }
                    }

                    e.Handled = true;
                    break;
                case HotKeyAction.SnapshotBackup:
                    SnapshotSelectActiveTab();

                    if (ActionSnapshotBackup(ActionSource.HotKey))
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlaySuccess();
                        }
                    }
                    else
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlayError();
                        }
                    }

                    e.Handled = true;
                    break;

                case HotKeyAction.SettingSwitchAutoBackup:
                    SnapshotSelectActiveTab();

                    if (ActionSwitchAutoBackup())
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlaySuccess();
                        }
                    }
                    else
                    {
                        if (_settingsManager.HotKeysSound)
                        {
                            SoundManager.PlayError();
                        }
                    }

                    e.Handled = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e, HotKeyToAction hotKeyToAction)
        {
            switch (hotKeyToAction.Action)
            {
                case HotKeyAction.CategoryPrevious:
                case HotKeyAction.CategoryNext:

                case HotKeyAction.SnapshotFirst:
                case HotKeyAction.SnapshotLast:
                case HotKeyAction.SnapshotPrevious:
                case HotKeyAction.SnapshotNext:

                case HotKeyAction.SnapshotRestore:
                case HotKeyAction.SnapshotDelete:
                case HotKeyAction.SnapshotBackup:

                case HotKeyAction.SettingSwitchAutoBackup:
                    e.Handled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void openLastUsedProfileOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.OpenLastUsedProfileOnStartup = !_settingsManager.OpenLastUsedProfileOnStartup;
        }

        private void OpenProfile(string path, bool autoOpen = false)
        {
            Logger.Information("---------------------------------Open---------------------------------");
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (File.Exists(path))
                {
                    ProfileFile profileFile = ProfileFile.Load(path);
                    EngineScript engineScript =
                        _engineScriptManager.BackupEngines.FirstOrDefault(e => e.Name == profileFile.EngineScriptName);

                    if (engineScript != null)
                    {
                        // close open profile if any
                        CloseProfile();

                        if (LoadProfile(profileFile, engineScript))
                        {
                            _settingsManager.LastUsedProfilePath = path;
                            _activeProfileFile = profileFile;

                            profileToolStripMenuItem.Enabled = true;
                            autoCleanupBackupToolStripMenuItem.Checked = _activeProfileFile.AutoCleanupBackup;
                            enabledCleanupToolStripMenuItem.Checked = _activeProfileFile.AutoCleanupBackup;
                            startAutoBackupAfterOpeningToolStripMenuItem.Checked = _activeProfileFile.StartAutoBackupAfterOpening;

                            tabPageProfile.Text = @"Profile: " + profileFile.Name;
                            ShowProfilePage();

                            if (_luaManager.ActiveEngine.ReadMe?.Call().First() is string usageContent)
                            {
                                ShowReadMePage(usageContent);
                            }

                            if (_activeProfileFile.FirstTimeRun && tabControlMain.Controls.Contains(tabPageUsage))
                            {
                                tabControlMain.SelectedTab = tabPageUsage;
                                _activeProfileFile.FirstTimeRun = false;
                            }
                            else
                            {
                                tabControlMain.SelectedTab = tabPageProfile;
                                tabControlSaves_Selected(null, null);
                            }

                            CreateHotKeysHook();

                            InitHotKeys();

                            InitWatcher();

                            _luaManager.ActiveEngine.OnLoaded?.Call();

                            if (_activeProfileFile.StartAutoBackupAfterOpening)
                            {
                                buttonActionAuto_Click(buttonActionAuto, null);
                            }
                        }
                        else
                        {
                            Message(@"Unable to load profile", "Error", MessageType.Error,
                                autoOpen ? MessageMode.Status : MessageMode.User);
                        }
                    }
                    else
                    {
                        Message(@"Unable to open profile: Engine not found", "Error", MessageType.Error,
                            autoOpen ? MessageMode.Status : MessageMode.User);
                    }
                }
                else
                {
                    Message(@"Unable to open profile: File not found", "Error", MessageType.Error,
                        autoOpen ? MessageMode.Status : MessageMode.User);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OpenProfileDialog()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (openFileDialogProfile.ShowDialog(this) == DialogResult.OK)
            {
                string profileFilename = openFileDialogProfile.FileName;
                if (!_settingsManager.PinnedProfiles.Contains(profileFilename))
                {
                    _settingsManager.AddRecentProfiles(profileFilename);
                }

                OpenProfile(profileFilename);
            }
        }

        private void openProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenProfileDialog();
        }

        private void pictureBoxScreenshot_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pictureBoxScreenshot.ImageLocation))
            {
                try
                {
                    Process.Start(pictureBoxScreenshot.ImageLocation);
                }
                catch (Exception ex)
                {
                    Logger.Log("pictureBoxScreenshot_DoubleClick: Exception: " + ex.Message, LogLevel.Debug);
                    Logger.LogException(ex);
                }
            }
        }

        private void pnghugeSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.ScreenshotQuality = ScreenshotQuality.Png;
        }

        private void RefreshCategories()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (_luaManager == null)
            {
                return;
            }

            comboBoxCategories.BeginUpdate();

            var selection = comboBoxCategories.SelectedItem as EngineSnapshotCategory;
            int id = -1;

            if (selection != null)
            {
                id = selection.Id;
                selection = null;
            }
            else if (_activeProfileFile.LastSelectedCategory != -1)
            {
                id = _activeProfileFile.LastSelectedCategory;
            }

            comboBoxCategories.Items.Clear();
            foreach (var category in _luaManager.ActiveEngine.Categories)
            {
                comboBoxCategories.Items.Add(category);
                if (category.Id == id)
                {
                    selection = category;
                }
            }

            if (selection != null && comboBoxCategories.Items.Contains(selection))
            {
                comboBoxCategories.SelectedItem = selection;
            }
            else
            {
                if (comboBoxCategories.Items.Count > 0)
                {
                    comboBoxCategories.SelectedIndex = comboBoxCategories.Items.Count - 1; // Select last category
                }
            }

            if (_luaManager.ActiveEngine.LastSnapshot != null && _settingsManager.AutoSelectLastSnapshot)
            {
                var snapshot = _luaManager.ActiveEngine.LastSnapshot;

                var category = _luaManager.ActiveEngine.Categories.SingleOrDefault(cat => cat.Id == snapshot.CategoryId);

                if (category != null && comboBoxCategories.Items.Contains(category))
                {
                    comboBoxCategories.SelectedItem = category;
                    _activeProfileFile.LastSelectedCategory = category.Id;
                }
            }

            comboBoxCategories.EndUpdate();
        }

        private void RefreshPinnedProfiles()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            panelPinnedList.Controls.Clear();
            foreach (string path in _settingsManager.PinnedProfiles.OrderByDescending(s => s))
            {
                ProfileLinkItem profileLinkItem = new ProfileLinkItem();
                string name = Path.GetFileNameWithoutExtension(path);

                profileLinkItem.linkLabelItem.Text = name;
                profileLinkItem.linkLabelItem.Tag = path;
                profileLinkItem.linkLabelItem.Click += (sender, args) =>
                {
                    LinkLabelItemOnLinkClicked(profileLinkItem.linkLabelItem, null);
                };

                profileLinkItem.linkLabelPath.Text = path;
                profileLinkItem.linkLabelPath.Tag = path;
                profileLinkItem.linkLabelPath.Click += (sender, args) =>
                {
                    LinkLabelItemOnLinkClicked(profileLinkItem.linkLabelPath, null);
                };

                profileLinkItem.pictureBoxPin.Click += (sender, args) =>
                {
                    if (_settingsManager.PinnedProfiles.Contains(path))
                    {
                        _settingsManager.RemovePinnedProfiles(path);
                    }
                };

                SetPictureBoxStyle(profileLinkItem.pictureBoxPin, IconChar.TrashAlt, Color.Gray);
                panelPinnedList.Controls.Add(profileLinkItem);
                profileLinkItem.Dock = DockStyle.Top;
            }
        }

        private void RefreshRecentProfiles()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            panelRecentList.Controls.Clear();
            foreach (string path in _settingsManager.RecentProfiles.OrderByDescending(s => s))
            {
                ProfileLinkItem profileLinkItem = new ProfileLinkItem();
                string name = Path.GetFileNameWithoutExtension(path);

                profileLinkItem.linkLabelItem.Text = name;
                profileLinkItem.linkLabelItem.Tag = path;
                profileLinkItem.linkLabelItem.Click += (sender, args) =>
                {
                    LinkLabelItemOnLinkClicked(profileLinkItem.linkLabelItem, null);
                };

                profileLinkItem.linkLabelPath.Text = path;
                profileLinkItem.linkLabelPath.Tag = path;
                profileLinkItem.linkLabelPath.Click += (sender, args) =>
                {
                    LinkLabelItemOnLinkClicked(profileLinkItem.linkLabelPath, null);
                };

                profileLinkItem.pictureBoxPin.Click += (sender, args) =>
                {
                    if (!_settingsManager.PinnedProfiles.Contains(path))
                    {
                        _settingsManager.AddPinnedProfiles(path);
                        _settingsManager.RemoveRecentProfiles(path);
                        SetPictureBoxStyle(profileLinkItem.pictureBoxPin, IconChar.MapPin, Color.Gray);
                    }
                };

                if (!_settingsManager.PinnedProfiles.Contains(path))
                {
                    SetPictureBoxStyle(profileLinkItem.pictureBoxPin, IconChar.MapPin, Color.RoyalBlue);
                }
                else
                {
                    SetPictureBoxStyle(profileLinkItem.pictureBoxPin, IconChar.MapPin, Color.Gray);
                }

                panelRecentList.Controls.Add(profileLinkItem);
                profileLinkItem.Dock = DockStyle.Top;
            }
        }

        private void RefreshSnapshotLists()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (_luaManager == null)
            {
                return;
            }

            var category = comboBoxCategories.SelectedItem as EngineSnapshotCategory;

            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                RefreshSnapshotsListView(listViewSnapshot, EngineSnapshotStatus.Active, category);
            }

            if (tabControlSaves.SelectedTab == tabPageArchivedSaves)
            {
                RefreshSnapshotsListView(listViewArchived, EngineSnapshotStatus.Archived, category);
            }

            if (tabControlSaves.SelectedTab == tabPageDeletedSaves)
            {
                RefreshSnapshotsListView(listViewDeleted, EngineSnapshotStatus.Deleted, category);
            }

            int active = 0;
            int archived = 0;
            int deleted = 0;

            foreach (var snapshot in _luaManager.ActiveEngine.Snapshots.Where(snapshot =>
                         category == null || category.Id == 0 || snapshot.CategoryId == category.Id))
            {
                switch (snapshot.Status)
                {
                    case EngineSnapshotStatus.Active:
                        active++;
                        break;
                    case EngineSnapshotStatus.Archived:
                        archived++;
                        break;
                    case EngineSnapshotStatus.Deleted:
                        deleted++;
                        break;
                }
            }

            tabPageActiveSaves.Text = $@"Active ({active})";
            tabPageArchivedSaves.Text = $@"Archived ({archived})";
            tabPageDeletedSaves.Text = $@"Deleted ({deleted})";
        }

        private void ReleaseHotKeysHook()
        {
            toolStripHotKey.Text = "Hot Keys: Idle";
            SetToolStripHotKeyStyle(toolStripHotKey, IconChar.Circle, Color.Gray);

            _hotKeysManager?.UnHook();
            _hotKeysManager?.Dispose();
            _hotKeysManager = null;
        }

        private void ReleaseWatcher()
        {
            _watcherManager?.Release();
            _watcherManager = null;
        }

        private void SaveProfile()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (_luaManager != null && _activeProfileFile != null)
            {
                _activeProfileFile.SortOrder = _sortOrder;
                _activeProfileFile.SortKey = _sortKey;

                _luaManager.SaveSnapshots(_activeProfileFile);
                _luaManager.SaveSettings(_activeProfileFile);

                ProfileFile.Save(_activeProfileFile);
            }
        }

        private void saveSizeAndPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.SaveSizeAndPosition = !_settingsManager.SaveSizeAndPosition;
            saveSizeAndPositionToolStripMenuItem.Checked = _settingsManager.SaveSizeAndPosition;
        }

        private void SetAutoBackupMessage()
        {
            if (_autoBackupEnabled)
            {
                switch (_watcherManager.AutoBackupStatus)
                {
                    case AutoBackupStatus.Disabled:
                        buttonActionAuto.Text = "Start AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Disabled";
                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.TimesCircle, Color.Red);
                        Message("Auto backup fail to start", "", MessageType.Information, MessageMode.Status);
                        _autoBackupEnabled = false;
                        break;
                    case AutoBackupStatus.Enabled:
                        buttonActionAuto.Text = "Stop AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Enabled";

                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.CheckCircle, Color.LimeGreen);
                        Message("Auto backup enabled", "", MessageType.Information, MessageMode.Status);
                        break;
                    case AutoBackupStatus.Waiting:
                        buttonActionAuto.Text = "Stop AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Waiting";
                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.Clock, Color.Orange);
                        Message("Auto backup will start after the first checkpoint", "", MessageType.Warning, MessageMode.User);
                        break;
                }
            }
            else
            {
                switch (_watcherManager.AutoBackupStatus)
                {
                    case AutoBackupStatus.Disabled:
                        buttonActionAuto.Text = "Start AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Disabled";
                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.TimesCircle, Color.Red);
                        Message("Auto backup disabled", "", MessageType.Information, MessageMode.Status);
                        break;
                    case AutoBackupStatus.Enabled:
                        buttonActionAuto.Text = "Stop AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Enabled";
                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.CheckCircle, Color.LimeGreen);
                        Message("Auto backup fail to stop", "", MessageType.Information, MessageMode.Status);
                        _autoBackupEnabled = true;
                        break;
                    case AutoBackupStatus.Waiting:
                        buttonActionAuto.Text = "Stop AutoBackup";
                        toolStripAutoBackup.Text = "AutoBackup: Waiting";
                        SetToolStripHotKeyStyle(toolStripAutoBackup, IconChar.Clock, Color.Orange);
                        Message("Auto backup fail to stop", "", MessageType.Warning, MessageMode.User);
                        break;
                }
            }
        }

        private void SetButtonStyle(Button button, IconChar iconChar, Color color)
        {
            button.Image = iconChar.ToBitmap(color, 32);
            button.ImageAlign = ContentAlignment.MiddleRight;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        private void SetComparer()
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.Name, " ", _sortKey, " ", _sortOrder);

            if (_sortKey == null)
            {
                _sortKey = "SavedAt";
            }

            _comparer = GetSnapshotComparer(_sortKey, _sortOrder);
        }

        private void setKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var hotKeyStatus = _settingsManager.HotKeysActive;
            _settingsManager.HotKeysActive = false;
            try
            {
                FormSettingsHotKeys formSettingsHotKeys = new FormSettingsHotKeys(_settingsManager.HotKeyToActions);
                if (formSettingsHotKeys.ShowDialog(this) == DialogResult.OK)
                {
                    InitHotKeys();
                }
            }
            finally
            {
                _settingsManager.HotKeysActive = hotKeyStatus;
            }
        }

        private void SetPictureBoxStyle(PictureBox pictureBox, IconChar iconChar, Color color)
        {
            pictureBox.Image = iconChar.ToBitmap(color, 32);
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void SetProgressBarVisibility(bool visible)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { SetProgressBarVisibility(visible); }));
            }
            else
            {
                toolStripProgressBar.Visible = visible;
            }
        }

        private void SetSnapshotInfo(EngineSnapshot snapshot)
        {
            _selectedSnapshot = snapshot;
            if (_selectedSnapshot != null)
            {
                SetSnapshotDetails(listViewDetails, _selectedSnapshot);

                textBoxNotes.Enabled = true;
                textBoxNotes.Text = _selectedSnapshot.Notes;

                pictureBoxScreenshot.ImageLocation = null;
                if (_selectedSnapshot.HasScreenshot && !string.IsNullOrEmpty(snapshot.ScreenshotFilename))
                {
                    string path = Path.Combine(_luaManager.ActiveEngine.SnapshotsFolder, snapshot.RelativePath,
                        snapshot.ScreenshotFilename);
                    if (File.Exists(path))
                    {
                        pictureBoxScreenshot.ImageLocation = path;
                        pictureBoxScreenshot.Visible = true;
                        labelScreenshotInfo.Visible = false;
                    }
                    else
                    {
                        pictureBoxScreenshot.Visible = false;
                        labelScreenshotInfo.Visible = true;
                    }
                }
                else
                {
                    pictureBoxScreenshot.Visible = false;
                    labelScreenshotInfo.Visible = true;
                }
            }
            else
            {
                listViewDetails.Enabled = false;
                listViewDetails.BeginUpdate();
                try
                {
                    listViewDetails.Items.Clear();
                }
                finally
                {
                    listViewDetails.EndUpdate();
                }

                textBoxNotes.Text = "";
                textBoxNotes.Enabled = false;

                pictureBoxScreenshot.ImageLocation = null;
                pictureBoxScreenshot.Visible = false;
                labelScreenshotInfo.Visible = true;
            }
        }

        private void SetToolStripHotKeyStyle(ToolStripStatusLabel toolStripStatusLabel, IconChar iconChar, Color color)
        {
            toolStripStatusLabel.Image = iconChar.ToBitmap(color, 20);
            toolStripStatusLabel.ImageAlign = ContentAlignment.BottomLeft;
            toolStripStatusLabel.TextImageRelation = TextImageRelation.TextBeforeImage;
        }

        private void SetToolStripStatusStyle(ToolStripStatusLabel toolStripStatusLabel, IconChar iconChar, Color color)
        {
            toolStripStatusLabel.Image = iconChar.ToBitmap(color, 20);
            toolStripStatusLabel.ImageAlign = ContentAlignment.BottomLeft;
            toolStripStatusLabel.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        private void ShowNotificationBalloon(string title, string text, ToolTipIcon icon)
        {
            if (!notifyIcon.Visible)
            {
                return;
            }

            if (!_settingsManager.ShowTrayNotification)
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipIcon = icon;
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(NotifyIconBalloonTimeout);
        }

        private void showNukedSnapshotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormNukedSnapshots formNukedSnapshots = new FormNukedSnapshots(this, _luaManager);
            formNukedSnapshots.ShowDialog(this);
        }

        private void ShowProfilePage()
        {
            if (!tabControlMain.Controls.Contains(tabPageProfile))
            {
                tabControlMain.Controls.Add(tabPageProfile);
            }
        }

        private void ShowReadMePage(string content)
        {
            richTextBoxUsage.Text = "";
            if (!string.IsNullOrEmpty(content))
            {
                if (!tabControlMain.Controls.Contains(tabPageUsage))
                {
                    tabControlMain.Controls.Add(tabPageUsage);
                }

                // plain text for now
                richTextBoxUsage.Text = content;
            }
        }

        private void showTrayNotificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.ShowTrayNotification = !_settingsManager.ShowTrayNotification;
        }

        private bool SnapshotNuke(EngineSnapshot snapshot)
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            try
            {
                string targetPath = Path.Combine(_luaManager.ActiveEngine.SnapshotsFolder, snapshot.RelativePath);
                DirectoryInfo directoryInfo = new DirectoryInfo(targetPath);

                if (directoryInfo.Parent == null)
                {
                    return false;
                }

                if (snapshot.Compressed)
                {
                    string archiveName = directoryInfo.Name + ".zip";
                    string sourcePath = Path.Combine(directoryInfo.Parent.FullName, archiveName);
                    // ReSharper disable once RedundantAssignment
                    directoryInfo = null;

                    // remove zip file
                    if (File.Exists(sourcePath))
                    {
                        File.Delete(sourcePath);
                    }
                }
                else
                {
                    if (directoryInfo.Exists)
                    {
                        directoryInfo.Delete(true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);

                return false;
            }
        }

        private void snapToScreenEdgesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.SnapToScreenEdges = !_settingsManager.SnapToScreenEdges;
        }

        private void SortSnapshots()
        {
            if (_comparer != null)
            {
                _luaManager.ActiveEngine.Snapshots.Sort(_comparer);
            }
        }

        private void soundNotificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.AutoBackupSoundNotification = !_settingsManager.AutoBackupSoundNotification;
            soundNotificationToolStripMenuItem.Checked = _settingsManager.AutoBackupSoundNotification;
        }

        private void soundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.HotKeysSound = !_settingsManager.HotKeysSound;
            soundToolStripMenuItem.Checked = _settingsManager.HotKeysSound;
        }

        private void startAutoBackupAfterOpeningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _activeProfileFile.StartAutoBackupAfterOpening = !_activeProfileFile.StartAutoBackupAfterOpening;
            startAutoBackupAfterOpeningToolStripMenuItem.Checked = _activeProfileFile.StartAutoBackupAfterOpening;
        }

        private void startMinimizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.StartMinimized = !_settingsManager.StartMinimized;
        }

        private void startWithWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.StartWithWindows = !_settingsManager.StartWithWindows;
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settingsManager.NotificationMode = MessageMode.Status;
            messageBoxToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.MessageBox;
            statusBarToolStripMenuItem.Checked = _settingsManager.NotificationMode == MessageMode.Status;
        }

        private void tabControlSaves_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControlSaves.SelectedTab == tabPageActiveSaves)
            {
                buttonActionBackup.Enabled = true;
                buttonActionRestore.Text = "Restore Selected";
                buttonActionArchive.Text = "Archive Selected";
                buttonActionDelete.Text = "Delete Selected";
            }
            else if (tabControlSaves.SelectedTab == tabPageArchivedSaves)
            {
                buttonActionBackup.Enabled = false;
                buttonActionRestore.Text = "Active Selected";
                buttonActionArchive.Text = "Archive Selected";
                buttonActionDelete.Text = "Delete Selected";
            }
            else if (tabControlSaves.SelectedTab == tabPageDeletedSaves)
            {
                buttonActionBackup.Enabled = false;
                buttonActionRestore.Text = "Active Selected";
                buttonActionArchive.Text = "Archive Selected";
                buttonActionDelete.Text = "Nuke Selected";
            }

            RefreshSnapshotLists();
        }

        private void textBoxNotes_Leave(object sender, EventArgs e)
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.Name, " ", _selectedSnapshot);

            if (_selectedSnapshot == null)
            {
                return;
            }

            if (_selectedSnapshot.Notes == textBoxNotes.Text)
            {
                return;
            }

            _selectedSnapshot.Notes = textBoxNotes.Text;
            SaveProfile();
            RefreshSnapshotLists();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool UnzipSnapshot(EngineSnapshot snapshot)
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (!snapshot.Compressed)
            {
                return true;
            }

            try
            {
                string targetPath = Path.Combine(_luaManager.ActiveEngine.SnapshotsFolder, snapshot.RelativePath);
                DirectoryInfo directoryInfo = new DirectoryInfo(targetPath);

                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                if (directoryInfo.Parent == null)
                {
                    return false;
                }

                string archiveName = directoryInfo.Name + ".zip";
                string sourcePath = Path.Combine(directoryInfo.Parent.FullName, archiveName);

                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }

                ZipFile.ExtractToDirectory(sourcePath, targetPath);

                snapshot.Compressed = false;

                try
                {
                    // remove zip file
                    File.Delete(sourcePath);
                }
                catch (Exception exception)
                {
                    Logger.LogException(exception);
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }

            return true;
        }

        private void UpdateProgressBar(int value, int max)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { UpdateProgressBar(value, max); }));
            }
            else
            {
                toolStripProgressBar.Maximum = max;
                toolStripProgressBar.Value = value;
            }
        }

        private void UpdateSortOrder(string key)
        {
            if (_sortKey != key)
            {
                _sortKey = key;
                _sortOrder = SortOrder.Ascending;
            }
            else
            {
                if (_sortOrder == SortOrder.Ascending)
                {
                    _sortOrder = SortOrder.Descending;
                }
                else
                {
                    _sortOrder = SortOrder.Ascending;
                }
            }
        }

        private bool ZipSnapshot(EngineSnapshot snapshot)
        {
            Logger.Information(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ".", MethodBase.GetCurrentMethod()?.Name);

            if (snapshot.Compressed)
            {
                return true;
            }

            try
            {
                string sourcePath = Path.Combine(_luaManager.ActiveEngine.SnapshotsFolder, snapshot.RelativePath);
                DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);

                if (directoryInfo.Parent == null)
                {
                    return false;
                }

                string archiveName = directoryInfo.Name + ".zip";
                string targetPath = Path.Combine(directoryInfo.Parent.FullName, archiveName);
                // ReSharper disable once RedundantAssignment
                directoryInfo = null;

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                ZipFile.CreateFromDirectory(
                    sourcePath, targetPath,
                    CompressionLevel.Optimal, false);

                snapshot.Compressed = true;

                try
                {
                    // remove zipped folder
                    Thread.Sleep(250);
                    Directory.Delete(sourcePath, true);
                }
                catch (Exception exception)
                {
                    if (exception is IOException)
                    {
                        Logger.Log("ZipSnapshot: Deletion of source folder failed, trying delayed delete.",
                            LogLevel.Warning);
                        Thread.Sleep(5000);
                        Directory.Delete(sourcePath, true);
                    }
                    else
                    {
                        Logger.LogException(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }

            return true;
        }

        #endregion
    }
}