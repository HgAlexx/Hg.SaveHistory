namespace Hg.SaveHistory.Controls
{
    partial class EngineSettingComboboxControl
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
            this.comboBoxValues = new System.Windows.Forms.ComboBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxCaption.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCaption
            // 
            this.groupBoxCaption.Controls.Add(this.comboBoxValues);
            this.groupBoxCaption.Controls.Add(this.labelDescription);
            this.groupBoxCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCaption.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCaption.Name = "groupBoxCaption";
            this.groupBoxCaption.Size = new System.Drawing.Size(300, 70);
            this.groupBoxCaption.TabIndex = 1;
            this.groupBoxCaption.TabStop = false;
            this.groupBoxCaption.Text = "Caption";
            // 
            // comboBoxValues
            // 
            this.comboBoxValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxValues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxValues.FormattingEnabled = true;
            this.comboBoxValues.Location = new System.Drawing.Point(6, 42);
            this.comboBoxValues.Name = "comboBoxValues";
            this.comboBoxValues.Size = new System.Drawing.Size(288, 21);
            this.comboBoxValues.TabIndex = 1;
            this.comboBoxValues.SelectedIndexChanged += new System.EventHandler(this.comboBoxValues_SelectedIndexChanged);
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
            // EngineSettingComboboxControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxCaption);
            this.MaximumSize = new System.Drawing.Size(0, 70);
            this.MinimumSize = new System.Drawing.Size(300, 70);
            this.Name = "EngineSettingComboboxControl";
            this.Size = new System.Drawing.Size(300, 70);
            this.groupBoxCaption.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox groupBoxCaption;
        public System.Windows.Forms.ComboBox comboBoxValues;
        public System.Windows.Forms.Label labelDescription;
        public System.Windows.Forms.ToolTip toolTipHelp;
    }
}
