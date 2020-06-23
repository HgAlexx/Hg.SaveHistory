//
// File imported from my old Hg.Common project
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Forms
{
    public partial class FormException : Form
    {
        #region Fields & Properties

        public List<Error> ErrorDetails { get; } = new List<Error>();

        #endregion

        #region Members

        public FormException()
        {
            InitializeComponent();
        }

        public void LoadCombobox()
        {
            comboBoxErrors.Items.Clear();

            foreach (var error in ErrorDetails)
            {
                comboBoxErrors.Items.Add(error.Title);
            }

            comboBoxErrors.SelectedIndex = comboBoxErrors.Items.Count - 1;
            ComboBoxErrors_SelectedIndexChanged(null, null);

            textBoxErrorCount.Text = ErrorDetails.Count.ToString();
        }

        private void ButtonContinue_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ButtonSaveErrorsToFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                string content = "";
                foreach (Error errorDetail in ErrorDetails)
                {
                    content += errorDetail.Title + Environment.NewLine;
                    content += errorDetail.Content + Environment.NewLine;
                    content += "----------------------------------------" + Environment.NewLine;
                }

                string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "Hg.SaveHistory.Errors.log");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.WriteAllText(filePath, content);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/HgAlexx/Hg.SaveHistory/issues/new");
        }

        private void ComboBoxErrors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxErrors.SelectedIndex >= 0)
            {
                var error = ErrorDetails[comboBoxErrors.SelectedIndex];
                textBoxDetail.Text = error.Content;
            }
        }

        #endregion
    }
}