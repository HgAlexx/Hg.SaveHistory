using System;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Controls
{
    public partial class AutoCleanupControl : UserControl
    {
        #region Members

        public AutoCleanupControl()
        {
            InitializeComponent();

            checkBoxEnabled.Checked = false;
            checkBoxPerCategory.Checked = false;

            checkBoxByAge.Checked = false;
            checkBoxByCount.Checked = false;
            checkBoxBySize.Checked = false;
            checkBoxByTotalSize.Checked = false;

            SetState();
        }

        public void FromSettings(SettingsAutoCleanupBackup settings)
        {
            checkBoxEnabled.Checked = settings.Enabled;
            checkBoxPerCategory.Checked = settings.PerCategory;

            checkBoxByAge.Checked = (settings.Modes & AutoCleanupMode.ByAge) == AutoCleanupMode.ByAge;
            checkBoxByCount.Checked = (settings.Modes & AutoCleanupMode.ByCount) == AutoCleanupMode.ByCount;
            checkBoxBySize.Checked = (settings.Modes & AutoCleanupMode.BySize) == AutoCleanupMode.BySize;
            checkBoxByTotalSize.Checked = (settings.Modes & AutoCleanupMode.ByTotalSize) == AutoCleanupMode.ByTotalSize;

            SetState();

            numericUpDownDays.Value = settings.Age?.Days ?? 0;
            numericUpDownHours.Value = settings.Age?.Hours ?? 0;
            numericUpDownMinutes.Value = settings.Age?.Minutes ?? 0;

            numericUpDownCount.Value = settings.Count ?? 0;
            numericUpDownSize.Value = settings.Size / 1_024 ?? 0;
            numericUpDownTotalSize.Value = settings.TotalSize / 1_024 / 1_024 ?? 0;
        }

        public void ToSettings(SettingsAutoCleanupBackup settings)
        {
            settings.Enabled = checkBoxEnabled.Checked;
            settings.PerCategory = checkBoxPerCategory.Checked;
            settings.Modes = AutoCleanupMode.None;

            if (checkBoxByAge.Checked)
            {
                settings.Modes |= AutoCleanupMode.ByAge;
                settings.Age = new TimeSpan(
                    (int)numericUpDownDays.Value,
                    (int)numericUpDownHours.Value,
                    (int)numericUpDownMinutes.Value);
            }
            else
            {
                settings.Age = null;
            }

            if (checkBoxByCount.Checked)
            {
                settings.Modes |= AutoCleanupMode.ByCount;
                settings.Count = (int)numericUpDownCount.Value;
            }
            else
            {
                settings.Count = null;
            }

            if (checkBoxBySize.Checked)
            {
                settings.Modes |= AutoCleanupMode.BySize;
                settings.Size = (int)numericUpDownSize.Value * 1_024;
            }
            else
            {
                settings.Size = null;
            }

            if (checkBoxByTotalSize.Checked)
            {
                settings.Modes |= AutoCleanupMode.ByTotalSize;
                settings.TotalSize = (int)numericUpDownTotalSize.Value * 1_024 * 1_024;
            }
            else
            {
                settings.TotalSize = null;
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            SetState();
        }

        private void SetState()
        {
            checkBoxPerCategory.Enabled = checkBoxEnabled.Checked;

            checkBoxByAge.Enabled = checkBoxEnabled.Checked;
            checkBoxByCount.Enabled = checkBoxEnabled.Checked;
            checkBoxBySize.Enabled = checkBoxEnabled.Checked;
            checkBoxByTotalSize.Enabled = checkBoxEnabled.Checked;

            numericUpDownDays.Enabled = checkBoxEnabled.Checked && checkBoxByAge.Checked;
            numericUpDownHours.Enabled = checkBoxEnabled.Checked && checkBoxByAge.Checked;
            numericUpDownMinutes.Enabled = checkBoxEnabled.Checked && checkBoxByAge.Checked;

            numericUpDownCount.Enabled = checkBoxEnabled.Checked && checkBoxByCount.Checked;

            numericUpDownSize.Enabled = checkBoxEnabled.Checked && checkBoxBySize.Checked;

            numericUpDownTotalSize.Enabled = checkBoxEnabled.Checked && checkBoxByTotalSize.Checked;
        }

        #endregion
    }
}