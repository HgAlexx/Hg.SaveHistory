namespace Hg.SaveHistory.Controls
{
    partial class AutoCleanupControl
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
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownTotalSize = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownSize = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownCount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMinutes = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownHours = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownDays = new System.Windows.Forms.NumericUpDown();
            this.checkBoxByTotalSize = new System.Windows.Forms.CheckBox();
            this.checkBoxBySize = new System.Windows.Forms.CheckBox();
            this.checkBoxByCount = new System.Windows.Forms.CheckBox();
            this.checkBoxByAge = new System.Windows.Forms.CheckBox();
            this.checkBoxPerCategory = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTotalSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDays)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(9, 3);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "Enabled";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            this.checkBoxEnabled.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.numericUpDownTotalSize);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numericUpDownSize);
            this.groupBox1.Controls.Add(this.numericUpDownCount);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDownMinutes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericUpDownHours);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDownDays);
            this.groupBox1.Controls.Add(this.checkBoxByTotalSize);
            this.groupBox1.Controls.Add(this.checkBoxBySize);
            this.groupBox1.Controls.Add(this.checkBoxByCount);
            this.groupBox1.Controls.Add(this.checkBoxByAge);
            this.groupBox1.Location = new System.Drawing.Point(3, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(475, 114);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cleanup conditions";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(331, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Mo";
            // 
            // numericUpDownTotalSize
            // 
            this.numericUpDownTotalSize.Location = new System.Drawing.Point(178, 87);
            this.numericUpDownTotalSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownTotalSize.Name = "numericUpDownTotalSize";
            this.numericUpDownTotalSize.Size = new System.Drawing.Size(147, 20);
            this.numericUpDownTotalSize.TabIndex = 13;
            this.numericUpDownTotalSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(331, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "ko";
            // 
            // numericUpDownSize
            // 
            this.numericUpDownSize.Location = new System.Drawing.Point(178, 64);
            this.numericUpDownSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownSize.Name = "numericUpDownSize";
            this.numericUpDownSize.Size = new System.Drawing.Size(147, 20);
            this.numericUpDownSize.TabIndex = 11;
            this.numericUpDownSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // numericUpDownCount
            // 
            this.numericUpDownCount.Location = new System.Drawing.Point(178, 41);
            this.numericUpDownCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownCount.Name = "numericUpDownCount";
            this.numericUpDownCount.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownCount.TabIndex = 10;
            this.numericUpDownCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(423, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "minutes";
            // 
            // numericUpDownMinutes
            // 
            this.numericUpDownMinutes.Location = new System.Drawing.Point(366, 18);
            this.numericUpDownMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownMinutes.Name = "numericUpDownMinutes";
            this.numericUpDownMinutes.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownMinutes.TabIndex = 8;
            this.numericUpDownMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(327, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "hours";
            // 
            // numericUpDownHours
            // 
            this.numericUpDownHours.Location = new System.Drawing.Point(270, 18);
            this.numericUpDownHours.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.numericUpDownHours.Name = "numericUpDownHours";
            this.numericUpDownHours.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownHours.TabIndex = 6;
            this.numericUpDownHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(235, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "days";
            // 
            // numericUpDownDays
            // 
            this.numericUpDownDays.Location = new System.Drawing.Point(178, 18);
            this.numericUpDownDays.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownDays.Name = "numericUpDownDays";
            this.numericUpDownDays.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDays.TabIndex = 4;
            this.numericUpDownDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxByTotalSize
            // 
            this.checkBoxByTotalSize.AutoSize = true;
            this.checkBoxByTotalSize.Location = new System.Drawing.Point(6, 88);
            this.checkBoxByTotalSize.Name = "checkBoxByTotalSize";
            this.checkBoxByTotalSize.Size = new System.Drawing.Size(82, 17);
            this.checkBoxByTotalSize.TabIndex = 3;
            this.checkBoxByTotalSize.Text = "By total size";
            this.checkBoxByTotalSize.UseVisualStyleBackColor = true;
            this.checkBoxByTotalSize.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxBySize
            // 
            this.checkBoxBySize.AutoSize = true;
            this.checkBoxBySize.Location = new System.Drawing.Point(6, 65);
            this.checkBoxBySize.Name = "checkBoxBySize";
            this.checkBoxBySize.Size = new System.Drawing.Size(106, 17);
            this.checkBoxBySize.TabIndex = 2;
            this.checkBoxBySize.Text = "By individual size";
            this.checkBoxBySize.UseVisualStyleBackColor = true;
            this.checkBoxBySize.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxByCount
            // 
            this.checkBoxByCount.AutoSize = true;
            this.checkBoxByCount.Location = new System.Drawing.Point(6, 42);
            this.checkBoxByCount.Name = "checkBoxByCount";
            this.checkBoxByCount.Size = new System.Drawing.Size(68, 17);
            this.checkBoxByCount.TabIndex = 1;
            this.checkBoxByCount.Text = "By count";
            this.checkBoxByCount.UseVisualStyleBackColor = true;
            this.checkBoxByCount.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxByAge
            // 
            this.checkBoxByAge.AutoSize = true;
            this.checkBoxByAge.Location = new System.Drawing.Point(6, 19);
            this.checkBoxByAge.Name = "checkBoxByAge";
            this.checkBoxByAge.Size = new System.Drawing.Size(59, 17);
            this.checkBoxByAge.TabIndex = 0;
            this.checkBoxByAge.Text = "By age";
            this.checkBoxByAge.UseVisualStyleBackColor = true;
            this.checkBoxByAge.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxPerCategory
            // 
            this.checkBoxPerCategory.AutoSize = true;
            this.checkBoxPerCategory.Location = new System.Drawing.Point(181, 3);
            this.checkBoxPerCategory.Name = "checkBoxPerCategory";
            this.checkBoxPerCategory.Size = new System.Drawing.Size(166, 17);
            this.checkBoxPerCategory.TabIndex = 2;
            this.checkBoxPerCategory.Text = "Apply conditions per Category";
            this.checkBoxPerCategory.UseVisualStyleBackColor = true;
            // 
            // AutoCleanupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxPerCategory);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxEnabled);
            this.Name = "AutoCleanupControl";
            this.Size = new System.Drawing.Size(481, 143);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTotalSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDays)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownTotalSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownSize;
        private System.Windows.Forms.NumericUpDown numericUpDownCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownMinutes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownHours;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownDays;
        private System.Windows.Forms.CheckBox checkBoxByTotalSize;
        private System.Windows.Forms.CheckBox checkBoxBySize;
        private System.Windows.Forms.CheckBox checkBoxByCount;
        private System.Windows.Forms.CheckBox checkBoxByAge;
        private System.Windows.Forms.CheckBox checkBoxPerCategory;
    }
}
