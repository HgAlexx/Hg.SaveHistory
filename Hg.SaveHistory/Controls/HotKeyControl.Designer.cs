namespace Hg.SaveHistory.Controls
{
    partial class HotKeyControl
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
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxShift = new System.Windows.Forms.CheckBox();
            this.checkBoxAlt = new System.Windows.Forms.CheckBox();
            this.checkBoxControl = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxKey = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(3, 22);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "Enabled";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            this.checkBoxEnabled.CheckedChanged += new System.EventHandler(this.checkBoxEnable_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxShift);
            this.groupBox1.Controls.Add(this.checkBoxAlt);
            this.groupBox1.Controls.Add(this.checkBoxControl);
            this.groupBox1.Location = new System.Drawing.Point(68, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(166, 48);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Modifiers";
            // 
            // checkBoxShift
            // 
            this.checkBoxShift.AutoSize = true;
            this.checkBoxShift.Location = new System.Drawing.Point(115, 19);
            this.checkBoxShift.Name = "checkBoxShift";
            this.checkBoxShift.Size = new System.Drawing.Size(47, 17);
            this.checkBoxShift.TabIndex = 2;
            this.checkBoxShift.Text = "Shift";
            this.checkBoxShift.UseVisualStyleBackColor = true;
            // 
            // checkBoxAlt
            // 
            this.checkBoxAlt.AutoSize = true;
            this.checkBoxAlt.Location = new System.Drawing.Point(71, 19);
            this.checkBoxAlt.Name = "checkBoxAlt";
            this.checkBoxAlt.Size = new System.Drawing.Size(38, 17);
            this.checkBoxAlt.TabIndex = 1;
            this.checkBoxAlt.Text = "Alt";
            this.checkBoxAlt.UseVisualStyleBackColor = true;
            // 
            // checkBoxControl
            // 
            this.checkBoxControl.AutoSize = true;
            this.checkBoxControl.Location = new System.Drawing.Point(6, 19);
            this.checkBoxControl.Name = "checkBoxControl";
            this.checkBoxControl.Size = new System.Drawing.Size(59, 17);
            this.checkBoxControl.TabIndex = 0;
            this.checkBoxControl.Text = "Control";
            this.checkBoxControl.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(240, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Key";
            // 
            // textBoxKey
            // 
            this.textBoxKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxKey.Location = new System.Drawing.Point(240, 30);
            this.textBoxKey.Name = "textBoxKey";
            this.textBoxKey.Size = new System.Drawing.Size(100, 20);
            this.textBoxKey.TabIndex = 3;
            this.textBoxKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxKey_KeyDown);
            this.textBoxKey.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKey_KeyUp);
            this.textBoxKey.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBoxKey_PreviewKeyDown);
            // 
            // HotKeyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxEnabled);
            this.MaximumSize = new System.Drawing.Size(0, 55);
            this.MinimumSize = new System.Drawing.Size(347, 55);
            this.Name = "HotKeyControl";
            this.Size = new System.Drawing.Size(347, 55);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox checkBoxEnabled;
        public System.Windows.Forms.CheckBox checkBoxShift;
        public System.Windows.Forms.CheckBox checkBoxAlt;
        public System.Windows.Forms.CheckBox checkBoxControl;
        public System.Windows.Forms.TextBox textBoxKey;
    }
}
