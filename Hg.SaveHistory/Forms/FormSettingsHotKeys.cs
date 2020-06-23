using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Hg.SaveHistory.Controls;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;

namespace Hg.SaveHistory.Forms
{
    public partial class FormSettingsHotKeys : Form
    {
        #region Fields & Properties

        private readonly List<HotKeyToAction> _hotKeyToActions;
        private readonly List<Keys> _keys = new List<Keys>();

        #endregion

        #region Members

        public FormSettingsHotKeys(List<HotKeyToAction> hotKeyToActions)
        {
            InitializeComponent();

            _hotKeyToActions = hotKeyToActions;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _keys.Clear();

            foreach (HotKeyControl hotKeyControl in FormHelper.FindControls<HotKeyControl>(this))
            {
                if (!_keys.Contains(hotKeyControl.Key))
                {
                    _keys.Add(hotKeyControl.Key);
                }
                else
                {
                    MessageBox.Show(
                        $@"Duplicate hot keys are not allowed, {hotKeyControl.Key} found twice.",
                        @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            foreach (HotKeyToAction hotKeyToAction in _hotKeyToActions)
            {
                switch (hotKeyToAction.Action)
                {
                    case HotKeyAction.CategoryPrevious:
                        hkCategoryPrevious.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.CategoryNext:
                        hkcCategoryNext.ToHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SnapshotFirst:
                        hkcSnapshotFirst.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotLast:
                        hkcSnapshotLast.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotPrevious:
                        hkcSnapshotPrevious.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotNext:
                        hkcSnapshotNext.ToHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SnapshotRestore:
                        hkcSnapshotRestore.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotBackup:
                        hkcSnapshotBackup.ToHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotDelete:
                        hkcSnapshotDelete.ToHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SettingSwitchAutoBackup:
                        hkcSwitchAutoBackup.ToHotKeyToAction(hotKeyToAction);
                        break;
                }
            }
        }

        private void FormSettingsHotKeys_Load(object sender, EventArgs e)
        {
            // Load hot keys into control
            foreach (HotKeyToAction hotKeyToAction in _hotKeyToActions)
            {
                switch (hotKeyToAction.Action)
                {
                    case HotKeyAction.CategoryPrevious:
                        hkCategoryPrevious.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.CategoryNext:
                        hkcCategoryNext.FromHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SnapshotFirst:
                        hkcSnapshotFirst.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotLast:
                        hkcSnapshotLast.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotPrevious:
                        hkcSnapshotPrevious.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotNext:
                        hkcSnapshotNext.FromHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SnapshotRestore:
                        hkcSnapshotRestore.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotBackup:
                        hkcSnapshotBackup.FromHotKeyToAction(hotKeyToAction);
                        break;
                    case HotKeyAction.SnapshotDelete:
                        hkcSnapshotDelete.FromHotKeyToAction(hotKeyToAction);
                        break;

                    case HotKeyAction.SettingSwitchAutoBackup:
                        hkcSwitchAutoBackup.FromHotKeyToAction(hotKeyToAction);
                        break;
                }
            }
        }

        #endregion
    }
}