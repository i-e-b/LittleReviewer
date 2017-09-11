namespace LittleReviewer
{
    partial class MainForm
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
            this.StatusLabel = new System.Windows.Forms.Label();
            this.LoadProjectButton = new System.Windows.Forms.Button();
            this.BrowseForProjectDlog = new System.Windows.Forms.FolderBrowserDialog();
            this.BranchSelectMenu = new System.Windows.Forms.ComboBox();
            this.StartReviewButton = new System.Windows.Forms.Button();
            this.EndReviewButton = new System.Windows.Forms.Button();
            this.CleanupReviewButton = new System.Windows.Forms.Button();
            this.CopyProgress = new System.Windows.Forms.ProgressBar();
            this.ProgressTimer = new System.Windows.Forms.Timer(this.components);
            this.SettingButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Location = new System.Drawing.Point(13, 38);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(445, 76);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Checking shared folders...";
            // 
            // LoadProjectButton
            // 
            this.LoadProjectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadProjectButton.Location = new System.Drawing.Point(16, 121);
            this.LoadProjectButton.Name = "LoadProjectButton";
            this.LoadProjectButton.Size = new System.Drawing.Size(202, 23);
            this.LoadProjectButton.TabIndex = 1;
            this.LoadProjectButton.Text = "Select Local Review Folder...";
            this.LoadProjectButton.UseVisualStyleBackColor = true;
            this.LoadProjectButton.Click += new System.EventHandler(this.LoadProjectButton_Click);
            // 
            // BrowseForProjectDlog
            // 
            this.BrowseForProjectDlog.Description = "Please select the folder where the project is checked out";
            this.BrowseForProjectDlog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.BrowseForProjectDlog.ShowNewFolderButton = false;
            // 
            // BranchSelectMenu
            // 
            this.BranchSelectMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchSelectMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BranchSelectMenu.Enabled = false;
            this.BranchSelectMenu.FormattingEnabled = true;
            this.BranchSelectMenu.Location = new System.Drawing.Point(158, 157);
            this.BranchSelectMenu.Name = "BranchSelectMenu";
            this.BranchSelectMenu.Size = new System.Drawing.Size(300, 21);
            this.BranchSelectMenu.TabIndex = 2;
            this.BranchSelectMenu.SelectedIndexChanged += new System.EventHandler(this.BranchSelectMenu_SelectedIndexChanged);
            // 
            // StartReviewButton
            // 
            this.StartReviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StartReviewButton.Enabled = false;
            this.StartReviewButton.Location = new System.Drawing.Point(16, 156);
            this.StartReviewButton.Name = "StartReviewButton";
            this.StartReviewButton.Size = new System.Drawing.Size(136, 23);
            this.StartReviewButton.TabIndex = 3;
            this.StartReviewButton.Text = "Start Review";
            this.StartReviewButton.UseVisualStyleBackColor = true;
            this.StartReviewButton.Click += new System.EventHandler(this.StartReviewButton_Click);
            // 
            // EndReviewButton
            // 
            this.EndReviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EndReviewButton.Enabled = false;
            this.EndReviewButton.Location = new System.Drawing.Point(16, 185);
            this.EndReviewButton.Name = "EndReviewButton";
            this.EndReviewButton.Size = new System.Drawing.Size(136, 23);
            this.EndReviewButton.TabIndex = 4;
            this.EndReviewButton.Text = "End Review";
            this.EndReviewButton.UseVisualStyleBackColor = true;
            this.EndReviewButton.Click += new System.EventHandler(this.EndReviewButton_Click);
            // 
            // CleanupReviewButton
            // 
            this.CleanupReviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CleanupReviewButton.Enabled = false;
            this.CleanupReviewButton.Location = new System.Drawing.Point(322, 121);
            this.CleanupReviewButton.Name = "CleanupReviewButton";
            this.CleanupReviewButton.Size = new System.Drawing.Size(136, 23);
            this.CleanupReviewButton.TabIndex = 5;
            this.CleanupReviewButton.Text = "Clean-up Review";
            this.CleanupReviewButton.UseVisualStyleBackColor = true;
            this.CleanupReviewButton.Click += new System.EventHandler(this.CleanupReviewButton_Click);
            // 
            // CopyProgress
            // 
            this.CopyProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyProgress.Location = new System.Drawing.Point(16, 12);
            this.CopyProgress.Name = "CopyProgress";
            this.CopyProgress.Size = new System.Drawing.Size(430, 23);
            this.CopyProgress.TabIndex = 6;
            // 
            // ProgressTimer
            // 
            this.ProgressTimer.Enabled = true;
            this.ProgressTimer.Interval = 1000;
            this.ProgressTimer.Tick += new System.EventHandler(this.ProgressTimer_Tick);
            // 
            // SettingButton
            // 
            this.SettingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SettingButton.Location = new System.Drawing.Point(428, 185);
            this.SettingButton.Name = "SettingButton";
            this.SettingButton.Size = new System.Drawing.Size(30, 24);
            this.SettingButton.TabIndex = 7;
            this.SettingButton.Text = ". . .";
            this.SettingButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.SettingButton.UseVisualStyleBackColor = true;
            this.SettingButton.Click += new System.EventHandler(this.SettingButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 221);
            this.Controls.Add(this.SettingButton);
            this.Controls.Add(this.CopyProgress);
            this.Controls.Add(this.CleanupReviewButton);
            this.Controls.Add(this.EndReviewButton);
            this.Controls.Add(this.StartReviewButton);
            this.Controls.Add(this.BranchSelectMenu);
            this.Controls.Add(this.LoadProjectButton);
            this.Controls.Add(this.StatusLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(486, 207);
            this.Name = "MainForm";
            this.Text = "Review Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button LoadProjectButton;
        private System.Windows.Forms.FolderBrowserDialog BrowseForProjectDlog;
        private System.Windows.Forms.ComboBox BranchSelectMenu;
        private System.Windows.Forms.Button StartReviewButton;
        private System.Windows.Forms.Button EndReviewButton;
        private System.Windows.Forms.Button CleanupReviewButton;
        private System.Windows.Forms.ProgressBar CopyProgress;
        private System.Windows.Forms.Timer ProgressTimer;
        private System.Windows.Forms.Button SettingButton;
    }
}

