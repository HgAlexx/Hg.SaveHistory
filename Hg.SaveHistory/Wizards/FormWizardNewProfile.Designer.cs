using System.Windows.Forms;

namespace Hg.SaveHistory.Wizards
{
    partial class FormWizardNewProfile
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWizardNewProfile));
            this.stepWizardControl = new AeroWizard.StepWizardControl();
            this.wizardPageWelcome = new AeroWizard.WizardPage();
            this.labelWelcome = new System.Windows.Forms.Label();
            this.wizardPageEngine = new AeroWizard.WizardPage();
            this.tableLayoutPanelEngine = new System.Windows.Forms.TableLayoutPanel();
            this.radioButtonEngineOfficial = new System.Windows.Forms.RadioButton();
            this.radioButtonEngineOther = new System.Windows.Forms.RadioButton();
            this.comboBoxEngineOfficial = new System.Windows.Forms.ComboBox();
            this.comboBoxEngineOther = new System.Windows.Forms.ComboBox();
            this.labelEngineDescription = new System.Windows.Forms.Label();
            this.labelWarning = new System.Windows.Forms.Label();
            this.wizardPageSetup = new AeroWizard.WizardPage();
            this.panelSetup = new System.Windows.Forms.Panel();
            this.wizardPageSave = new AeroWizard.WizardPage();
            this.panelSave = new System.Windows.Forms.Panel();
            this.richTextBoxSummary = new System.Windows.Forms.RichTextBox();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.buttonSaveBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSaveFolder = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.stepWizardControl)).BeginInit();
            this.wizardPageWelcome.SuspendLayout();
            this.wizardPageEngine.SuspendLayout();
            this.tableLayoutPanelEngine.SuspendLayout();
            this.wizardPageSetup.SuspendLayout();
            this.wizardPageSave.SuspendLayout();
            this.panelSave.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // stepWizardControl
            // 
            this.stepWizardControl.BackColor = System.Drawing.Color.White;
            this.stepWizardControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stepWizardControl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stepWizardControl.Location = new System.Drawing.Point(0, 0);
            this.stepWizardControl.Name = "stepWizardControl";
            this.stepWizardControl.Pages.Add(this.wizardPageWelcome);
            this.stepWizardControl.Pages.Add(this.wizardPageEngine);
            this.stepWizardControl.Pages.Add(this.wizardPageSetup);
            this.stepWizardControl.Pages.Add(this.wizardPageSave);
            this.stepWizardControl.ShowProgressInTaskbarIcon = true;
            this.stepWizardControl.Size = new System.Drawing.Size(744, 470);
            this.stepWizardControl.StepListFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.stepWizardControl.SuppressParentFormCaptionSync = true;
            this.stepWizardControl.SuppressParentFormIconSync = true;
            this.stepWizardControl.TabIndex = 0;
            this.stepWizardControl.Title = "Backup Profile Creation Wizard";
            this.stepWizardControl.TitleIcon = ((System.Drawing.Icon)(resources.GetObject("stepWizardControl.TitleIcon")));
            this.stepWizardControl.Finished += new System.EventHandler(this.stepWizardControl_Finished);
            // 
            // wizardPageWelcome
            // 
            this.wizardPageWelcome.Controls.Add(this.labelWelcome);
            this.wizardPageWelcome.Name = "wizardPageWelcome";
            this.wizardPageWelcome.NextPage = this.wizardPageEngine;
            this.wizardPageWelcome.Size = new System.Drawing.Size(546, 316);
            this.stepWizardControl.SetStepText(this.wizardPageWelcome, "Welcome");
            this.wizardPageWelcome.TabIndex = 2;
            this.wizardPageWelcome.Text = "Welcome to the Backup Profile Creation Wizard";
            // 
            // labelWelcome
            // 
            this.labelWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelWelcome.Location = new System.Drawing.Point(0, 0);
            this.labelWelcome.Name = "labelWelcome";
            this.labelWelcome.Size = new System.Drawing.Size(546, 316);
            this.labelWelcome.TabIndex = 1;
            this.labelWelcome.Text = "Welcome !\r\n\r\n\r\nThis wizard will assist you in the creation of a backup profile.\r\n" +
    "\r\nClick Next to continue, or Cancel to exit.";
            // 
            // wizardPageEngine
            // 
            this.wizardPageEngine.AllowBack = false;
            this.wizardPageEngine.Controls.Add(this.tableLayoutPanelEngine);
            this.wizardPageEngine.Name = "wizardPageEngine";
            this.wizardPageEngine.NextPage = this.wizardPageSetup;
            this.wizardPageEngine.Size = new System.Drawing.Size(546, 316);
            this.stepWizardControl.SetStepText(this.wizardPageEngine, "Choose an engine");
            this.wizardPageEngine.TabIndex = 3;
            this.wizardPageEngine.Text = "Choose a Backup Engine";
            this.wizardPageEngine.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.wizardPageEngine_Commit);
            // 
            // tableLayoutPanelEngine
            // 
            this.tableLayoutPanelEngine.ColumnCount = 2;
            this.tableLayoutPanelEngine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelEngine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelEngine.Controls.Add(this.radioButtonEngineOfficial, 0, 0);
            this.tableLayoutPanelEngine.Controls.Add(this.radioButtonEngineOther, 1, 0);
            this.tableLayoutPanelEngine.Controls.Add(this.comboBoxEngineOfficial, 0, 1);
            this.tableLayoutPanelEngine.Controls.Add(this.comboBoxEngineOther, 1, 1);
            this.tableLayoutPanelEngine.Controls.Add(this.labelEngineDescription, 0, 3);
            this.tableLayoutPanelEngine.Controls.Add(this.labelWarning, 0, 2);
            this.tableLayoutPanelEngine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelEngine.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelEngine.Name = "tableLayoutPanelEngine";
            this.tableLayoutPanelEngine.RowCount = 3;
            this.tableLayoutPanelEngine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelEngine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelEngine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelEngine.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelEngine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelEngine.Size = new System.Drawing.Size(546, 316);
            this.tableLayoutPanelEngine.TabIndex = 1;
            // 
            // radioButtonEngineOfficial
            // 
            this.radioButtonEngineOfficial.AutoSize = true;
            this.radioButtonEngineOfficial.Checked = true;
            this.radioButtonEngineOfficial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonEngineOfficial.Location = new System.Drawing.Point(3, 3);
            this.radioButtonEngineOfficial.Name = "radioButtonEngineOfficial";
            this.radioButtonEngineOfficial.Size = new System.Drawing.Size(267, 24);
            this.radioButtonEngineOfficial.TabIndex = 0;
            this.radioButtonEngineOfficial.TabStop = true;
            this.radioButtonEngineOfficial.Text = "Official Engine";
            this.radioButtonEngineOfficial.UseVisualStyleBackColor = true;
            this.radioButtonEngineOfficial.CheckedChanged += new System.EventHandler(this.radioButtonEngineOfficial_CheckedChanged);
            // 
            // radioButtonEngineOther
            // 
            this.radioButtonEngineOther.AutoSize = true;
            this.radioButtonEngineOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonEngineOther.Location = new System.Drawing.Point(276, 3);
            this.radioButtonEngineOther.Name = "radioButtonEngineOther";
            this.radioButtonEngineOther.Size = new System.Drawing.Size(267, 24);
            this.radioButtonEngineOther.TabIndex = 1;
            this.radioButtonEngineOther.Text = "Third Party Engine";
            this.radioButtonEngineOther.UseVisualStyleBackColor = true;
            this.radioButtonEngineOther.CheckedChanged += new System.EventHandler(this.radioButtonEngineOther_CheckedChanged);
            // 
            // comboBoxEngineOfficial
            // 
            this.comboBoxEngineOfficial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxEngineOfficial.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEngineOfficial.FormattingEnabled = true;
            this.comboBoxEngineOfficial.Location = new System.Drawing.Point(3, 33);
            this.comboBoxEngineOfficial.Name = "comboBoxEngineOfficial";
            this.comboBoxEngineOfficial.Size = new System.Drawing.Size(267, 23);
            this.comboBoxEngineOfficial.TabIndex = 2;
            this.comboBoxEngineOfficial.SelectionChangeCommitted += new System.EventHandler(this.comboBoxEngineOfficial_SelectionChangeCommitted);
            // 
            // comboBoxEngineOther
            // 
            this.comboBoxEngineOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxEngineOther.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEngineOther.Enabled = false;
            this.comboBoxEngineOther.FormattingEnabled = true;
            this.comboBoxEngineOther.Location = new System.Drawing.Point(276, 33);
            this.comboBoxEngineOther.Name = "comboBoxEngineOther";
            this.comboBoxEngineOther.Size = new System.Drawing.Size(267, 23);
            this.comboBoxEngineOther.TabIndex = 3;
            this.comboBoxEngineOther.SelectionChangeCommitted += new System.EventHandler(this.comboBoxEngineOther_SelectionChangeCommitted);
            // 
            // labelEngineDescription
            // 
            this.tableLayoutPanelEngine.SetColumnSpan(this.labelEngineDescription, 2);
            this.labelEngineDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelEngineDescription.Location = new System.Drawing.Point(3, 90);
            this.labelEngineDescription.Name = "labelEngineDescription";
            this.labelEngineDescription.Size = new System.Drawing.Size(540, 237);
            this.labelEngineDescription.TabIndex = 4;
            // 
            // labelWarning
            // 
            this.tableLayoutPanelEngine.SetColumnSpan(this.labelWarning, 2);
            this.labelWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelWarning.ForeColor = System.Drawing.Color.Red;
            this.labelWarning.Location = new System.Drawing.Point(3, 60);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(540, 30);
            this.labelWarning.TabIndex = 5;
            // 
            // wizardPageSetup
            // 
            this.wizardPageSetup.Controls.Add(this.panelSetup);
            this.wizardPageSetup.Name = "wizardPageSetup";
            this.wizardPageSetup.NextPage = this.wizardPageSave;
            this.wizardPageSetup.Size = new System.Drawing.Size(546, 316);
            this.stepWizardControl.SetStepText(this.wizardPageSetup, "Setup");
            this.wizardPageSetup.TabIndex = 4;
            this.wizardPageSetup.Text = "Setup the Engine";
            this.wizardPageSetup.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.wizardPageSetup_Commit);
            this.wizardPageSetup.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.wizardPageSetup_Initialize);
            // 
            // panelSetup
            // 
            this.panelSetup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSetup.Location = new System.Drawing.Point(0, 0);
            this.panelSetup.Name = "panelSetup";
            this.panelSetup.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.panelSetup.Size = new System.Drawing.Size(546, 316);
            this.panelSetup.TabIndex = 1;
            // 
            // wizardPageSave
            // 
            this.wizardPageSave.Controls.Add(this.panelSave);
            this.wizardPageSave.IsFinishPage = true;
            this.wizardPageSave.Name = "wizardPageSave";
            this.wizardPageSave.Size = new System.Drawing.Size(546, 316);
            this.stepWizardControl.SetStepText(this.wizardPageSave, "Save && Finish");
            this.wizardPageSave.TabIndex = 5;
            this.wizardPageSave.Text = "Save the Backup Profile";
            this.wizardPageSave.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.wizardPageSave_Commit);
            this.wizardPageSave.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.wizardPageSave_Initialize);
            // 
            // panelSave
            // 
            this.panelSave.Controls.Add(this.richTextBoxSummary);
            this.panelSave.Controls.Add(this.groupBoxInfo);
            this.panelSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSave.Location = new System.Drawing.Point(0, 0);
            this.panelSave.Name = "panelSave";
            this.panelSave.Size = new System.Drawing.Size(546, 316);
            this.panelSave.TabIndex = 1;
            // 
            // richTextBoxSummary
            // 
            this.richTextBoxSummary.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxSummary.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxSummary.Name = "richTextBoxSummary";
            this.richTextBoxSummary.ReadOnly = true;
            this.richTextBoxSummary.Size = new System.Drawing.Size(546, 201);
            this.richTextBoxSummary.TabIndex = 4;
            this.richTextBoxSummary.Text = "";
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.buttonSaveBrowse);
            this.groupBoxInfo.Controls.Add(this.label2);
            this.groupBoxInfo.Controls.Add(this.textBoxSaveFolder);
            this.groupBoxInfo.Controls.Add(this.textBoxName);
            this.groupBoxInfo.Controls.Add(this.label1);
            this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxInfo.Location = new System.Drawing.Point(0, 201);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(546, 115);
            this.groupBoxInfo.TabIndex = 3;
            this.groupBoxInfo.TabStop = false;
            // 
            // buttonSaveBrowse
            // 
            this.buttonSaveBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveBrowse.Location = new System.Drawing.Point(506, 81);
            this.buttonSaveBrowse.Name = "buttonSaveBrowse";
            this.buttonSaveBrowse.Size = new System.Drawing.Size(35, 23);
            this.buttonSaveBrowse.TabIndex = 5;
            this.buttonSaveBrowse.Text = "...";
            this.buttonSaveBrowse.UseVisualStyleBackColor = true;
            this.buttonSaveBrowse.Click += new System.EventHandler(this.buttonSaveBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.label2.Location = new System.Drawing.Point(6, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(293, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Choose where the profile and backups will be saved";
            // 
            // textBoxSaveFolder
            // 
            this.textBoxSaveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSaveFolder.Location = new System.Drawing.Point(6, 81);
            this.textBoxSaveFolder.Name = "textBoxSaveFolder";
            this.textBoxSaveFolder.Size = new System.Drawing.Size(494, 23);
            this.textBoxSaveFolder.TabIndex = 3;
            this.textBoxSaveFolder.TextChanged += new System.EventHandler(this.textBoxSaveFolder_TextChanged);
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(6, 37);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(534, 23);
            this.textBoxName.TabIndex = 2;
            this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(215, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose a name for the backup profile";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "shp";
            this.saveFileDialog.Title = "Save Backup Profile";
            // 
            // toolTipHelp
            // 
            this.toolTipHelp.AutoPopDelay = 20000;
            this.toolTipHelp.InitialDelay = 100;
            this.toolTipHelp.ReshowDelay = 10;
            this.toolTipHelp.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipHelp.ToolTipTitle = "Help";
            // 
            // FormWizardNewProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 470);
            this.Controls.Add(this.stepWizardControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormWizardNewProfile";
            this.Text = "Backup Profile Creation Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.stepWizardControl)).EndInit();
            this.wizardPageWelcome.ResumeLayout(false);
            this.wizardPageEngine.ResumeLayout(false);
            this.tableLayoutPanelEngine.ResumeLayout(false);
            this.tableLayoutPanelEngine.PerformLayout();
            this.wizardPageSetup.ResumeLayout(false);
            this.wizardPageSave.ResumeLayout(false);
            this.panelSave.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.StepWizardControl stepWizardControl;
        private AeroWizard.WizardPage wizardPageWelcome;
        private AeroWizard.WizardPage wizardPageEngine;
        private AeroWizard.WizardPage wizardPageSetup;
        private System.Windows.Forms.Label labelWelcome;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelEngine;
        private System.Windows.Forms.RadioButton radioButtonEngineOfficial;
        private System.Windows.Forms.RadioButton radioButtonEngineOther;
        private System.Windows.Forms.ComboBox comboBoxEngineOfficial;
        private System.Windows.Forms.ComboBox comboBoxEngineOther;
        private System.Windows.Forms.Label labelEngineDescription;
        private AeroWizard.WizardPage wizardPageSave;
        private Label labelWarning;
        private Panel panelSetup;
        private GroupBox groupBoxInfo;
        private TextBox textBoxName;
        private Label label1;
        private Panel panelSave;
        private SaveFileDialog saveFileDialog;
        private Button buttonSaveBrowse;
        private Label label2;
        private TextBox textBoxSaveFolder;
        private FolderBrowserDialog folderBrowserDialog;
        public ToolTip toolTipHelp;
        private RichTextBox richTextBoxSummary;
    }
}