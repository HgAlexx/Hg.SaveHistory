namespace Hg.SaveHistory.Forms
{
    partial class FormException
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxDetail = new System.Windows.Forms.TextBox();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonGithub = new System.Windows.Forms.Button();
            this.comboBoxErrors = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxErrorCount = new System.Windows.Forms.TextBox();
            this.buttonSaveErrorsToFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxDetail
            // 
            this.textBoxDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDetail.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxDetail.Location = new System.Drawing.Point(12, 128);
            this.textBoxDetail.MaxLength = 999999;
            this.textBoxDetail.Multiline = true;
            this.textBoxDetail.Name = "textBoxDetail";
            this.textBoxDetail.ReadOnly = true;
            this.textBoxDetail.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetail.Size = new System.Drawing.Size(533, 282);
            this.textBoxDetail.TabIndex = 0;
            this.textBoxDetail.WordWrap = false;
            // 
            // buttonContinue
            // 
            this.buttonContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonContinue.Location = new System.Drawing.Point(401, 12);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(144, 23);
            this.buttonContinue.TabIndex = 1;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.ButtonContinue_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(401, 70);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(144, 23);
            this.buttonExit.TabIndex = 2;
            this.buttonExit.Text = "Exit Application";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.ButtonExit_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(383, 81);
            this.label1.TabIndex = 3;
            this.label1.Text = "Unexpected errors occurred :(\r\nPlease report error details to the author:\r\n- Open" +
    " an issue on Github\r\n- Or send a DM to mZHg#7908 on discord";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonGithub
            // 
            this.buttonGithub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGithub.Location = new System.Drawing.Point(401, 41);
            this.buttonGithub.Name = "buttonGithub";
            this.buttonGithub.Size = new System.Drawing.Size(144, 23);
            this.buttonGithub.TabIndex = 4;
            this.buttonGithub.Text = "Open Github Issue";
            this.buttonGithub.UseVisualStyleBackColor = true;
            this.buttonGithub.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // comboBoxErrors
            // 
            this.comboBoxErrors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxErrors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxErrors.FormattingEnabled = true;
            this.comboBoxErrors.Location = new System.Drawing.Point(159, 101);
            this.comboBoxErrors.Name = "comboBoxErrors";
            this.comboBoxErrors.Size = new System.Drawing.Size(236, 21);
            this.comboBoxErrors.TabIndex = 5;
            this.comboBoxErrors.SelectedIndexChanged += new System.EventHandler(this.ComboBoxErrors_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Error count";
            // 
            // textBoxErrorCount
            // 
            this.textBoxErrorCount.Location = new System.Drawing.Point(74, 102);
            this.textBoxErrorCount.Name = "textBoxErrorCount";
            this.textBoxErrorCount.ReadOnly = true;
            this.textBoxErrorCount.Size = new System.Drawing.Size(79, 20);
            this.textBoxErrorCount.TabIndex = 7;
            // 
            // buttonSaveErrorsToFile
            // 
            this.buttonSaveErrorsToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveErrorsToFile.Location = new System.Drawing.Point(401, 99);
            this.buttonSaveErrorsToFile.Name = "buttonSaveErrorsToFile";
            this.buttonSaveErrorsToFile.Size = new System.Drawing.Size(144, 23);
            this.buttonSaveErrorsToFile.TabIndex = 8;
            this.buttonSaveErrorsToFile.Text = "Save errors to file";
            this.buttonSaveErrorsToFile.UseVisualStyleBackColor = true;
            this.buttonSaveErrorsToFile.Click += new System.EventHandler(this.ButtonSaveErrorsToFile_Click);
            // 
            // FormException
            // 
            this.ClientSize = new System.Drawing.Size(557, 422);
            this.Controls.Add(this.buttonSaveErrorsToFile);
            this.Controls.Add(this.textBoxErrorCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxErrors);
            this.Controls.Add(this.buttonGithub);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.textBoxDetail);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.Name = "FormException";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error report";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxDetail;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonGithub;
        private System.Windows.Forms.ComboBox comboBoxErrors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxErrorCount;
        private System.Windows.Forms.Button buttonSaveErrorsToFile;
    }
}
