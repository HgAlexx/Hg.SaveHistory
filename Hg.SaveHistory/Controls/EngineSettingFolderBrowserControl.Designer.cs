namespace Hg.SaveHistory.Controls
{
    partial class EngineSettingFolderBrowserControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxCaption = new System.Windows.Forms.GroupBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonAutoDetect = new System.Windows.Forms.Button();
            this.textBoxFolderPath = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBoxCaption.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCaption
            // 
            this.groupBoxCaption.Controls.Add(this.buttonBrowse);
            this.groupBoxCaption.Controls.Add(this.buttonAutoDetect);
            this.groupBoxCaption.Controls.Add(this.textBoxFolderPath);
            this.groupBoxCaption.Controls.Add(this.labelDescription);
            this.groupBoxCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCaption.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCaption.Name = "groupBoxCaption";
            this.groupBoxCaption.Size = new System.Drawing.Size(300, 70);
            this.groupBoxCaption.TabIndex = 0;
            this.groupBoxCaption.TabStop = false;
            this.groupBoxCaption.Text = "Caption";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(138, 40);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 3;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonAutoDetect
            // 
            this.buttonAutoDetect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAutoDetect.Location = new System.Drawing.Point(219, 40);
            this.buttonAutoDetect.Name = "buttonAutoDetect";
            this.buttonAutoDetect.Size = new System.Drawing.Size(75, 23);
            this.buttonAutoDetect.TabIndex = 2;
            this.buttonAutoDetect.Text = "Auto Detect";
            this.buttonAutoDetect.UseVisualStyleBackColor = true;
            // 
            // textBoxFolderPath
            // 
            this.textBoxFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFolderPath.Location = new System.Drawing.Point(6, 42);
            this.textBoxFolderPath.Name = "textBoxFolderPath";
            this.textBoxFolderPath.ReadOnly = true;
            this.textBoxFolderPath.Size = new System.Drawing.Size(126, 20);
            this.textBoxFolderPath.TabIndex = 1;
            this.textBoxFolderPath.TextChanged += new System.EventHandler(this.textBoxFolderPath_TextChanged);
            // 
            // labelDescription
            // 
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.Location = new System.Drawing.Point(6, 16);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(288, 23);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Description";
            this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolTipHelp
            // 
            this.toolTipHelp.AutoPopDelay = 20000;
            this.toolTipHelp.InitialDelay = 100;
            this.toolTipHelp.ReshowDelay = 10;
            this.toolTipHelp.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipHelp.ToolTipTitle = "Help";
            // 
            // EngineSettingFolderBrowserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxCaption);
            this.MaximumSize = new System.Drawing.Size(0, 70);
            this.MinimumSize = new System.Drawing.Size(300, 70);
            this.Name = "EngineSettingFolderBrowserControl";
            this.Size = new System.Drawing.Size(300, 70);
            this.groupBoxCaption.ResumeLayout(false);
            this.groupBoxCaption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.GroupBox groupBoxCaption;
        public System.Windows.Forms.Button buttonBrowse;
        public System.Windows.Forms.Button buttonAutoDetect;
        public System.Windows.Forms.TextBox textBoxFolderPath;
        public System.Windows.Forms.Label labelDescription;
        public System.Windows.Forms.ToolTip toolTipHelp;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}
