
namespace R2SaveEditor
{
    partial class R2Form
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

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R2Form));
            this.nudCampaignXp = new System.Windows.Forms.NumericUpDown();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.check_BlackWraith = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nudCompXp = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nudMedicHead = new System.Windows.Forms.NumericUpDown();
            this.nudSoldierHead = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nudSpecHead = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.unpackButton = new System.Windows.Forms.Button();
            this.repackButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.openTextureEditorButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.extractAssetsButton = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.compileLUAButton = new System.Windows.Forms.Button();
            this.logsTextBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.logsCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudCampaignXp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCompXp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMedicHead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoldierHead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpecHead)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // nudCampaignXp
            // 
            this.nudCampaignXp.Location = new System.Drawing.Point(89, 46);
            this.nudCampaignXp.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudCampaignXp.Name = "nudCampaignXp";
            this.nudCampaignXp.Size = new System.Drawing.Size(120, 20);
            this.nudCampaignXp.TabIndex = 0;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.SelectedPath = "C:\\Users\\Utilisateur\\Downloads\\RPCS3\\dev_hdd0\\home\\00000001\\savedata\\BCES00226_SA" +
    "VE_0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Campaign XP :";
            // 
            // check_BlackWraith
            // 
            this.check_BlackWraith.AutoSize = true;
            this.check_BlackWraith.Location = new System.Drawing.Point(66, 203);
            this.check_BlackWraith.Name = "check_BlackWraith";
            this.check_BlackWraith.Size = new System.Drawing.Size(87, 17);
            this.check_BlackWraith.TabIndex = 3;
            this.check_BlackWraith.Text = "Black Wraith";
            this.check_BlackWraith.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Comp XP :";
            // 
            // nudCompXp
            // 
            this.nudCompXp.Location = new System.Drawing.Point(89, 72);
            this.nudCompXp.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudCompXp.Name = "nudCompXp";
            this.nudCompXp.Size = new System.Drawing.Size(120, 20);
            this.nudCompXp.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Medic Head:";
            // 
            // nudMedicHead
            // 
            this.nudMedicHead.Location = new System.Drawing.Point(89, 113);
            this.nudMedicHead.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMedicHead.Name = "nudMedicHead";
            this.nudMedicHead.Size = new System.Drawing.Size(48, 20);
            this.nudMedicHead.TabIndex = 8;
            // 
            // nudSoldierHead
            // 
            this.nudSoldierHead.Location = new System.Drawing.Point(89, 139);
            this.nudSoldierHead.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSoldierHead.Name = "nudSoldierHead";
            this.nudSoldierHead.Size = new System.Drawing.Size(48, 20);
            this.nudSoldierHead.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Soldier Head:";
            // 
            // nudSpecHead
            // 
            this.nudSpecHead.Location = new System.Drawing.Point(89, 165);
            this.nudSpecHead.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.nudSpecHead.Name = "nudSpecHead";
            this.nudSpecHead.Size = new System.Drawing.Size(48, 20);
            this.nudSpecHead.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 167);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Spec Head:";
            // 
            // unpackButton
            // 
            this.unpackButton.Location = new System.Drawing.Point(15, 19);
            this.unpackButton.Name = "unpackButton";
            this.unpackButton.Size = new System.Drawing.Size(98, 23);
            this.unpackButton.TabIndex = 13;
            this.unpackButton.Text = "Unpack PSARC";
            this.unpackButton.UseVisualStyleBackColor = true;
            this.unpackButton.Click += new System.EventHandler(this.unpackButton_Click);
            // 
            // repackButton
            // 
            this.repackButton.Location = new System.Drawing.Point(15, 48);
            this.repackButton.Name = "repackButton";
            this.repackButton.Size = new System.Drawing.Size(98, 23);
            this.repackButton.TabIndex = 14;
            this.repackButton.Text = "Repack PSARC";
            this.repackButton.UseVisualStyleBackColor = true;
            this.repackButton.Click += new System.EventHandler(this.repackButton_Click);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(28, 17);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 15;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(110, 17);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 16;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.logsCheckBox);
            this.groupBox1.Controls.Add(this.openButton);
            this.groupBox1.Controls.Add(this.saveButton);
            this.groupBox1.Controls.Add(this.nudCampaignXp);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.check_BlackWraith);
            this.groupBox1.Controls.Add(this.nudCompXp);
            this.groupBox1.Controls.Add(this.nudSpecHead);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.nudSoldierHead);
            this.groupBox1.Controls.Add(this.nudMedicHead);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(11, 68);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(218, 226);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Save Editor";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.unpackButton);
            this.groupBox2.Controls.Add(this.repackButton);
            this.groupBox2.Location = new System.Drawing.Point(235, 68);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(128, 83);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PSARC";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // openTextureEditorButton
            // 
            this.openTextureEditorButton.Location = new System.Drawing.Point(15, 48);
            this.openTextureEditorButton.Name = "openTextureEditorButton";
            this.openTextureEditorButton.Size = new System.Drawing.Size(98, 23);
            this.openTextureEditorButton.TabIndex = 24;
            this.openTextureEditorButton.Text = "Texture Editor";
            this.openTextureEditorButton.UseVisualStyleBackColor = true;
            this.openTextureEditorButton.Click += new System.EventHandler(this.openTextureEditorButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.extractAssetsButton);
            this.groupBox3.Controls.Add(this.openTextureEditorButton);
            this.groupBox3.Location = new System.Drawing.Point(235, 157);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(128, 83);
            this.groupBox3.TabIndex = 25;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Assets";
            // 
            // extractAssetsButton
            // 
            this.extractAssetsButton.Location = new System.Drawing.Point(15, 19);
            this.extractAssetsButton.Name = "extractAssetsButton";
            this.extractAssetsButton.Size = new System.Drawing.Size(98, 23);
            this.extractAssetsButton.TabIndex = 27;
            this.extractAssetsButton.Text = "Extract Assets";
            this.extractAssetsButton.UseVisualStyleBackColor = true;
            this.extractAssetsButton.Click += new System.EventHandler(this.extractAssetsButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.compileLUAButton);
            this.groupBox4.Location = new System.Drawing.Point(235, 246);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(128, 48);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Lua";
            // 
            // compileLUAButton
            // 
            this.compileLUAButton.BackColor = System.Drawing.Color.Transparent;
            this.compileLUAButton.Location = new System.Drawing.Point(14, 17);
            this.compileLUAButton.Name = "compileLUAButton";
            this.compileLUAButton.Size = new System.Drawing.Size(98, 23);
            this.compileLUAButton.TabIndex = 25;
            this.compileLUAButton.Text = "Compile LUA";
            this.compileLUAButton.UseVisualStyleBackColor = false;
            this.compileLUAButton.Click += new System.EventHandler(this.compileLUAButton_Click);
            // 
            // logsTextBox
            // 
            this.logsTextBox.Location = new System.Drawing.Point(16, 305);
            this.logsTextBox.Multiline = true;
            this.logsTextBox.Name = "logsTextBox";
            this.logsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logsTextBox.Size = new System.Drawing.Size(352, 196);
            this.logsTextBox.TabIndex = 28;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::R2Tool.Properties.Resources.Resistance_2_Logo;
            this.pictureBox1.Location = new System.Drawing.Point(11, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(352, 61);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 23;
            this.pictureBox1.TabStop = false;
            // 
            // logsCheckBox
            // 
            this.logsCheckBox.AutoSize = true;
            this.logsCheckBox.Location = new System.Drawing.Point(6, 204);
            this.logsCheckBox.Name = "logsCheckBox";
            this.logsCheckBox.Size = new System.Drawing.Size(15, 14);
            this.logsCheckBox.TabIndex = 17;
            this.logsCheckBox.UseVisualStyleBackColor = true;
            this.logsCheckBox.CheckedChanged += new System.EventHandler(this.logsCheckBox_CheckedChanged);
            // 
            // R2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(374, 301);
            this.Controls.Add(this.logsTextBox);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "R2Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "R2 - ToolKit";
            this.Load += new System.EventHandler(this.R2Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudCampaignXp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCompXp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMedicHead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoldierHead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpecHead)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudCampaignXp;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox check_BlackWraith;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudCompXp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudMedicHead;
        private System.Windows.Forms.NumericUpDown nudSoldierHead;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudSpecHead;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button unpackButton;
        private System.Windows.Forms.Button repackButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button openTextureEditorButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button compileLUAButton;
        private System.Windows.Forms.Button extractAssetsButton;
        private System.Windows.Forms.TextBox logsTextBox;
        private System.Windows.Forms.CheckBox logsCheckBox;
    }
}

