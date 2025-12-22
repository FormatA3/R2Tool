
namespace R2SaveEditor
{
    partial class TextureEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureEditorForm));
            this.assetListbox = new System.Windows.Forms.ListBox();
            this.openAssetLookupButton = new System.Windows.Forms.Button();
            this.extractButton = new System.Windows.Forms.Button();
            this.extractAllButton = new System.Windows.Forms.Button();
            this.replaceButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.currentFolderLabel = new System.Windows.Forms.Label();
            this.previewPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // assetListbox
            // 
            this.assetListbox.FormattingEnabled = true;
            this.assetListbox.Location = new System.Drawing.Point(8, 41);
            this.assetListbox.Name = "assetListbox";
            this.assetListbox.Size = new System.Drawing.Size(168, 485);
            this.assetListbox.TabIndex = 0;
            this.assetListbox.SelectedIndexChanged += new System.EventHandler(this.assetListbox_SelectedIndexChanged);
            // 
            // openAssetLookupButton
            // 
            this.openAssetLookupButton.Location = new System.Drawing.Point(8, 12);
            this.openAssetLookupButton.Name = "openAssetLookupButton";
            this.openAssetLookupButton.Size = new System.Drawing.Size(143, 23);
            this.openAssetLookupButton.TabIndex = 1;
            this.openAssetLookupButton.Text = "Open AssetLookup Folder";
            this.openAssetLookupButton.UseVisualStyleBackColor = true;
            this.openAssetLookupButton.Click += new System.EventHandler(this.openAssetLookupButton_Click);
            // 
            // extractButton
            // 
            this.extractButton.Location = new System.Drawing.Point(182, 12);
            this.extractButton.Name = "extractButton";
            this.extractButton.Size = new System.Drawing.Size(93, 23);
            this.extractButton.TabIndex = 2;
            this.extractButton.Text = "Extract";
            this.extractButton.UseVisualStyleBackColor = true;
            this.extractButton.Click += new System.EventHandler(this.extractButton_Click);
            // 
            // extractAllButton
            // 
            this.extractAllButton.Location = new System.Drawing.Point(281, 12);
            this.extractAllButton.Name = "extractAllButton";
            this.extractAllButton.Size = new System.Drawing.Size(93, 23);
            this.extractAllButton.TabIndex = 3;
            this.extractAllButton.Text = "Extract All";
            this.extractAllButton.UseVisualStyleBackColor = true;
            this.extractAllButton.Click += new System.EventHandler(this.extractAllButton_Click);
            // 
            // replaceButton
            // 
            this.replaceButton.Location = new System.Drawing.Point(390, 12);
            this.replaceButton.Name = "replaceButton";
            this.replaceButton.Size = new System.Drawing.Size(93, 23);
            this.replaceButton.TabIndex = 5;
            this.replaceButton.Text = "Replace";
            this.replaceButton.UseVisualStyleBackColor = true;
            this.replaceButton.Click += new System.EventHandler(this.replaceButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(182, 500);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(267, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "Highmips.dat must be in the same directory as\r\nassetlookup.dat";
            // 
            // currentFolderLabel
            // 
            this.currentFolderLabel.AutoSize = true;
            this.currentFolderLabel.Location = new System.Drawing.Point(5, 529);
            this.currentFolderLabel.Name = "currentFolderLabel";
            this.currentFolderLabel.Size = new System.Drawing.Size(102, 13);
            this.currentFolderLabel.TabIndex = 7;
            this.currentFolderLabel.Text = "Current folder: None";
            // 
            // previewPictureBox
            // 
            this.previewPictureBox.Location = new System.Drawing.Point(182, 41);
            this.previewPictureBox.Name = "previewPictureBox";
            this.previewPictureBox.Size = new System.Drawing.Size(301, 260);
            this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.previewPictureBox.TabIndex = 4;
            this.previewPictureBox.TabStop = false;
            // 
            // TextureEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(495, 549);
            this.Controls.Add(this.currentFolderLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.replaceButton);
            this.Controls.Add(this.previewPictureBox);
            this.Controls.Add(this.extractAllButton);
            this.Controls.Add(this.extractButton);
            this.Controls.Add(this.openAssetLookupButton);
            this.Controls.Add(this.assetListbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TextureEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "R2 - Texture Editor";
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox assetListbox;
        private System.Windows.Forms.Button openAssetLookupButton;
        private System.Windows.Forms.Button extractButton;
        private System.Windows.Forms.Button extractAllButton;
        private System.Windows.Forms.PictureBox previewPictureBox;
        private System.Windows.Forms.Button replaceButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label currentFolderLabel;
    }
}