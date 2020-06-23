namespace Hg.SaveHistory.Controls
{
    partial class ProfileLinkItem
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
            this.pictureBoxPin = new System.Windows.Forms.PictureBox();
            this.linkLabelItem = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabelPath = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPin)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxPin
            // 
            this.pictureBoxPin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxPin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxPin.Location = new System.Drawing.Point(163, -1);
            this.pictureBoxPin.Name = "pictureBoxPin";
            this.pictureBoxPin.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.pictureBoxPin.Size = new System.Drawing.Size(36, 36);
            this.pictureBoxPin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxPin.TabIndex = 4;
            this.pictureBoxPin.TabStop = false;
            // 
            // linkLabelItem
            // 
            this.linkLabelItem.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.linkLabelItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelItem.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelItem.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelItem.LinkColor = System.Drawing.Color.Black;
            this.linkLabelItem.Location = new System.Drawing.Point(3, 1);
            this.linkLabelItem.Name = "linkLabelItem";
            this.linkLabelItem.Size = new System.Drawing.Size(158, 16);
            this.linkLabelItem.TabIndex = 5;
            this.linkLabelItem.TabStop = true;
            this.linkLabelItem.Text = "profile_item";
            this.linkLabelItem.VisitedLinkColor = System.Drawing.Color.RoyalBlue;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.linkLabelPath);
            this.panel1.Controls.Add(this.linkLabelItem);
            this.panel1.Controls.Add(this.pictureBoxPin);
            this.panel1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 36);
            this.panel1.TabIndex = 6;
            // 
            // linkLabelPath
            // 
            this.linkLabelPath.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.linkLabelPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelPath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelPath.LinkColor = System.Drawing.Color.RoyalBlue;
            this.linkLabelPath.Location = new System.Drawing.Point(2, 19);
            this.linkLabelPath.Name = "linkLabelPath";
            this.linkLabelPath.Size = new System.Drawing.Size(159, 14);
            this.linkLabelPath.TabIndex = 6;
            this.linkLabelPath.TabStop = true;
            this.linkLabelPath.Text = "profile_item";
            this.linkLabelPath.VisitedLinkColor = System.Drawing.Color.RoyalBlue;
            // 
            // ProfileLinkItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(0, 36);
            this.MinimumSize = new System.Drawing.Size(200, 36);
            this.Name = "ProfileLinkItem";
            this.Size = new System.Drawing.Size(200, 36);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPin)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pictureBoxPin;
        public System.Windows.Forms.LinkLabel linkLabelItem;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.LinkLabel linkLabelPath;
    }
}
