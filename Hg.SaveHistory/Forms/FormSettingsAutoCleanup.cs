using System;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Forms
{
    public partial class FormSettingsAutoCleanup : Form
    {
        #region Fields & Properties

        private readonly SettingsAutoCleanupBackup _settingsAutoCleanupBackupArchive;
        private readonly SettingsAutoCleanupBackup _settingsAutoCleanupBackupDelete;
        private readonly SettingsAutoCleanupBackup _settingsAutoCleanupBackupNuke;

        #endregion

        #region Members

        public FormSettingsAutoCleanup(
            SettingsAutoCleanupBackup settingsAutoCleanupBackupArchive,
            SettingsAutoCleanupBackup settingsAutoCleanupBackupDelete,
            SettingsAutoCleanupBackup settingsAutoCleanupBackupNuke)
        {
            InitializeComponent();

            _settingsAutoCleanupBackupArchive = settingsAutoCleanupBackupArchive;
            _settingsAutoCleanupBackupDelete = settingsAutoCleanupBackupDelete;
            _settingsAutoCleanupBackupNuke = settingsAutoCleanupBackupNuke;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            autoCleanupControlArchive.ToSettings(_settingsAutoCleanupBackupArchive);
            autoCleanupControlDelete.ToSettings(_settingsAutoCleanupBackupDelete);
            autoCleanupControlNuke.ToSettings(_settingsAutoCleanupBackupNuke);
        }

        private void FormSettingsAutoCleanup_Load(object sender, EventArgs e)
        {
            autoCleanupControlArchive.FromSettings(_settingsAutoCleanupBackupArchive);
            autoCleanupControlDelete.FromSettings(_settingsAutoCleanupBackupDelete);
            autoCleanupControlNuke.FromSettings(_settingsAutoCleanupBackupNuke);
        }

        #endregion
    }
}