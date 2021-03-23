using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AeroWizard;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Controls;
using Hg.SaveHistory.Managers;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Wizards
{
    public partial class FormWizardNewProfile : Form
    {
        #region Fields & Properties

        private LuaManager _luaManager;
        private string _profileFileFullName;
        private EngineScript _selectedEngineScript;
        public string ProfileFileFullName => _profileFileFullName;

        #endregion

        #region Members

        public FormWizardNewProfile(EngineScriptManager engineScriptManager)
        {
            InitializeComponent();

            // Load official engines
            foreach (var backupEngine in engineScriptManager.BackupEngines.Where(backupEngine => backupEngine.Official))
            {
                comboBoxEngineOfficial.Items.Add(backupEngine);
            }

            // Load third party engines
            foreach (var backupEngine in engineScriptManager.BackupEngines.Where(backupEngine => !backupEngine.Official))
            {
                comboBoxEngineOfficial.Items.Add(backupEngine);
            }

            wizardPageEngine.AllowNext = false;
            wizardPageSetup.AllowNext = false;
        }

        private void buttonSaveBrowse_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxSaveFolder.Text) && Directory.Exists(textBoxSaveFolder.Text))
            {
                folderBrowserDialog.SelectedPath = textBoxSaveFolder.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (Directory.Exists(folderBrowserDialog.SelectedPath))
                {
                    textBoxSaveFolder.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void comboBoxEngineOfficial_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxEngineOfficial.SelectedItem is EngineScript backupEngine)
            {
                _selectedEngineScript = backupEngine;
                labelEngineDescription.Text = backupEngine.Description;

                if (backupEngine.IsAltered(true))
                {
                    labelWarning.Text = "WARNING: The files of this engine have been altered! Use at your own risk!";
                }
                else
                {
                    labelWarning.Text = "";
                }

                wizardPageEngine.AllowNext = true;
            }
        }

        private void comboBoxEngineOther_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxEngineOther.SelectedItem is EngineScript backupEngine)
            {
                _selectedEngineScript = backupEngine;
                labelEngineDescription.Text = backupEngine.Description;
                labelWarning.Text = "WARNING: Third party engine script might be unsafe, use at your own risk!";

                wizardPageEngine.AllowNext = true;
            }
        }

        private void PageSaveOnInputChanges()
        {
            Color errorColor = Color.LightCoral;
            string filename = textBoxName.Text;
            bool valid = true;

            textBoxName.BackColor = Color.White;
            toolTipHelp.SetToolTip(textBoxName, "");

            textBoxSaveFolder.BackColor = Color.White;
            toolTipHelp.SetToolTip(textBoxSaveFolder, "");

            if (!string.IsNullOrEmpty(filename))
            {
                if (Path.GetInvalidFileNameChars().Any(c => filename.Contains(c)))
                {
                    valid = false;
                }

                if (!valid)
                {
                    textBoxName.BackColor = errorColor;
                    toolTipHelp.SetToolTip(textBoxName, "Invalid name: the name must be a valid filename");
                }
            }
            else
            {
                valid = false;
                toolTipHelp.SetToolTip(textBoxName, "Invalid name: the name is mandatory");
            }

            if (string.IsNullOrEmpty(textBoxSaveFolder.Text))
            {
                valid = false;
                toolTipHelp.SetToolTip(textBoxSaveFolder, "Invalid folder: the folder is mandatory");
            }
            else if (!string.IsNullOrEmpty(textBoxSaveFolder.Text) && !Directory.Exists(textBoxSaveFolder.Text))
            {
                valid = false;
                textBoxSaveFolder.BackColor = errorColor;
                toolTipHelp.SetToolTip(textBoxSaveFolder, "Invalid folder: the folder must exist");
            }

            wizardPageSave.AllowNext = valid;
        }

        private void radioButtonEngineOfficial_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEngineOfficial.Checked)
            {
                comboBoxEngineOfficial.SelectedItem = null;
                comboBoxEngineOther.SelectedItem = null;

                comboBoxEngineOfficial.Enabled = true;
                comboBoxEngineOther.Enabled = false;

                labelEngineDescription.Text = "";
                _selectedEngineScript = null;
            }

            wizardPageEngine.AllowNext = false;
        }

        private void radioButtonEngineOther_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEngineOther.Checked)
            {
                comboBoxEngineOfficial.SelectedItem = null;
                comboBoxEngineOther.SelectedItem = null;

                comboBoxEngineOfficial.Enabled = false;
                comboBoxEngineOther.Enabled = true;

                labelEngineDescription.Text = "";
                _selectedEngineScript = null;
            }

            wizardPageEngine.AllowNext = false;
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            wizardPageSetup.AllowNext = false;
            if (_luaManager.ActiveEngine.OnSetupValidate != null)
            {
                if (_luaManager.ActiveEngine.OnSetupValidate.Call().First() is bool result)
                {
                    wizardPageSetup.AllowNext = result;
                }
            }
        }

        private void stepWizardControl_Finished(object sender, EventArgs e)
        {
            // TODO: ?
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            PageSaveOnInputChanges();
        }

        private void textBoxSaveFolder_TextChanged(object sender, EventArgs e)
        {
            PageSaveOnInputChanges();
        }

        private void wizardPageEngine_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (_selectedEngineScript != null)
            {
                if (_selectedEngineScript.Official && _selectedEngineScript.IsAltered())
                {
                    if (MessageBox.Show("Are you sure you want to use this altered engine ?", "Confirmation", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (!_selectedEngineScript.Official)
                {
                    if (MessageBox.Show("Are you sure you want to use this third party engine ?", "Confirmation",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                _luaManager = new LuaManager();

                if (!_luaManager.LoadEngine(_selectedEngineScript))
                {
                    MessageBox.Show("Unable to load this Engine!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    _luaManager = null;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void wizardPageSave_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            try
            {
                var profileFile = new ProfileFile {EngineScriptName = _selectedEngineScript.Name, Name = textBoxName.Text};

                _luaManager.SaveSettings(profileFile);

                // Save file
                string filePath = Path.Combine(textBoxSaveFolder.Text, textBoxName.Text);
                filePath = Path.ChangeExtension(filePath, "shp");

                if (File.Exists(filePath))
                {
                    if (MessageBox.Show(@"The file already exists, do you want to override it?", @"Confirmation", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                _profileFileFullName = filePath;
                profileFile.FilePath = filePath;
                ProfileFile.Save(profileFile);
            }
            catch
            {
                e.Cancel = true;
            }
        }

        private void wizardPageSave_Initialize(object sender, WizardPageInitEventArgs e)
        {
            wizardPageSave.AllowNext = false;

            // build summary
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-= Backup Profile Setup Summary =-");
            sb.AppendLine("");
            sb.AppendLine("Settings:");

            foreach (var setting in _luaManager.ActiveEngine.Settings.Where(s => s.Kind == EngineSettingKind.Setup).OrderBy(s => s.Index))
            {
                if (setting is EngineSettingCombobox settingCombobox)
                {
                    string s = "";
                    if (settingCombobox.Values.ContainsKey(settingCombobox.Value))
                    {
                        s = settingCombobox.Values[settingCombobox.Value];
                    }

                    sb.AppendLine($"- {settingCombobox.Caption}: {s}");
                }

                if (setting is EngineSettingFolderBrowser settingFolder)
                {
                    sb.AppendLine($"- {settingFolder.Caption}: {settingFolder.Value}");
                }

                if (setting is EngineSettingCheckbox settingCheckbox)
                {
                    if (settingCheckbox.Value)
                    {
                        sb.AppendLine($"- {settingCheckbox.Caption}: Checked");
                    }
                    else
                    {
                        sb.AppendLine($"- {settingCheckbox.Caption}: Unchecked");
                    }
                }

                if (setting is EngineSettingTextbox settingTextbox)
                {
                    sb.AppendLine($"- {settingTextbox.Caption}: {settingTextbox.Value}");
                }
            }

            if (_luaManager.ActiveEngine.ReadMe?.Call().First() is string content)
            {
                sb.AppendLine();
                sb.AppendLine("-= README =-");
                foreach (string line in content.Split('\n'))
                {
                    sb.AppendLine(line);
                }
            }

            richTextBoxSummary.Text = sb.ToString();

            if (_luaManager.ActiveEngine.OnSetupSuggestProfileName != null)
            {
                if (_luaManager.ActiveEngine.OnSetupSuggestProfileName.Call().First() is string name)
                {
                    textBoxName.Text = name;
                }
                else
                {
                    textBoxName.Text = _luaManager.ActiveEngine.Name + @" Profile";
                }
            }

            PageSaveOnInputChanges();
        }

        private void wizardPageSetup_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            // TODO: ?
        }

        private void wizardPageSetup_Initialize(object sender, WizardPageInitEventArgs e)
        {
            panelSetup.Controls.Clear();

            foreach (var setting in _luaManager.ActiveEngine.Settings.Where(s => s.Kind == EngineSettingKind.Setup).OrderBy(s => -s.Index))
            {
                // TODO: missing settings types

                if (setting is EngineSettingCombobox settingCombobox)
                {
                    var control = new EngineSettingComboboxControl
                    {
                        groupBoxCaption = {Text = settingCombobox.Caption}, labelDescription = {Text = settingCombobox.Description}
                    };
                    control.toolTipHelp.SetToolTip(control.comboBoxValues, settingCombobox.HelpTooltip);
                    control.SetComboboxValues(settingCombobox.Values);
                    control.SetValue(settingCombobox.Value);
                    control.ValueChanged += () =>
                    {
                        settingCombobox.Value = control.Value;
                        SettingsChanged(null, null);
                    };

                    panelSetup.Controls.Add(control);
                    control.Dock = DockStyle.Top;
                }

                if (setting is EngineSettingFolderBrowser settingFolder)
                {
                    var control = new EngineSettingFolderBrowserControl
                    {
                        groupBoxCaption = {Text = settingFolder.Caption}, labelDescription = {Text = settingFolder.Description}
                    };
                    control.toolTipHelp.SetToolTip(control.textBoxFolderPath, settingFolder.HelpTooltip);
                    control.buttonAutoDetect.Enabled = settingFolder.CanAutoDetect;
                    control.buttonAutoDetect.Click += (o, args) =>
                    {
                        if (settingFolder.OnAutoDetect?.Call().First() is string result)
                        {
                            control.textBoxFolderPath.Text = result;
                        }
                    };
                    control.ValueChanged += () =>
                    {
                        settingFolder.Value = control.Value;
                        SettingsChanged(null, null);
                    };
                    panelSetup.Controls.Add(control);
                    control.Dock = DockStyle.Top;
                }
            }
        }

        #endregion
    }
}