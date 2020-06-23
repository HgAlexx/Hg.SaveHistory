using System;
using System.IO;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Controls
{
    public partial class EngineSettingFolderBrowserControl : UserControl
    {
        #region Fields & Properties

        public event SettingControlEventHandler ValueChanged;

        public string Value => textBoxFolderPath.Text;

        #endregion

        #region Members

        public EngineSettingFolderBrowserControl()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxFolderPath.Text))
            {
                folderBrowserDialog.SelectedPath = textBoxFolderPath.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (Directory.Exists(folderBrowserDialog.SelectedPath))
                {
                    textBoxFolderPath.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void textBoxFolderPath_TextChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke();
        }

        #endregion
    }
}