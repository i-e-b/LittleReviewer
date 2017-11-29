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
            this.StartReviewButton = new System.Windows.Forms.Button();
            this.CopyProgress = new System.Windows.Forms.ProgressBar();
            this.ProgressTimer = new System.Windows.Forms.Timer(this.components);
            this.SettingButton = new System.Windows.Forms.Button();
            this.JourneyStatusGrid = new System.Windows.Forms.PropertyGrid();
            this.ShowHelpButton = new System.Windows.Forms.Button();
            this.RefreshListButton = new System.Windows.Forms.Button();
            this.VpnModeCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Location = new System.Drawing.Point(13, 38);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(577, 52);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Checking shared folders...";
            // 
            // LoadProjectButton
            // 
            this.LoadProjectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadProjectButton.Location = new System.Drawing.Point(16, 390);
            this.LoadProjectButton.Name = "LoadProjectButton";
            this.LoadProjectButton.Size = new System.Drawing.Size(184, 23);
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
            // StartReviewButton
            // 
            this.StartReviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartReviewButton.Enabled = false;
            this.StartReviewButton.Location = new System.Drawing.Point(490, 391);
            this.StartReviewButton.Name = "StartReviewButton";
            this.StartReviewButton.Size = new System.Drawing.Size(136, 23);
            this.StartReviewButton.TabIndex = 3;
            this.StartReviewButton.Text = "Synchronise";
            this.StartReviewButton.UseVisualStyleBackColor = true;
            this.StartReviewButton.Click += new System.EventHandler(this.StartReviewButton_Click);
            // 
            // CopyProgress
            // 
            this.CopyProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyProgress.Location = new System.Drawing.Point(16, 12);
            this.CopyProgress.Name = "CopyProgress";
            this.CopyProgress.Size = new System.Drawing.Size(574, 23);
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
            this.SettingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SettingButton.Location = new System.Drawing.Point(596, 11);
            this.SettingButton.Name = "SettingButton";
            this.SettingButton.Size = new System.Drawing.Size(30, 24);
            this.SettingButton.TabIndex = 7;
            this.SettingButton.Text = ". . .";
            this.SettingButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.SettingButton.UseVisualStyleBackColor = true;
            this.SettingButton.Click += new System.EventHandler(this.SettingButton_Click);
            // 
            // JourneyStatusGrid
            // 
            this.JourneyStatusGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.JourneyStatusGrid.LineColor = System.Drawing.SystemColors.ControlDark;
            this.JourneyStatusGrid.Location = new System.Drawing.Point(11, 121);
            this.JourneyStatusGrid.Margin = new System.Windows.Forms.Padding(2);
            this.JourneyStatusGrid.Name = "JourneyStatusGrid";
            this.JourneyStatusGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.JourneyStatusGrid.Size = new System.Drawing.Size(615, 263);
            this.JourneyStatusGrid.TabIndex = 8;
            this.JourneyStatusGrid.ToolbarVisible = false;
            // 
            // ShowHelpButton
            // 
            this.ShowHelpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowHelpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowHelpButton.Location = new System.Drawing.Point(596, 41);
            this.ShowHelpButton.Name = "ShowHelpButton";
            this.ShowHelpButton.Size = new System.Drawing.Size(30, 23);
            this.ShowHelpButton.TabIndex = 9;
            this.ShowHelpButton.Text = "?";
            this.ShowHelpButton.UseVisualStyleBackColor = true;
            this.ShowHelpButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // RefreshListButton
            // 
            this.RefreshListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshListButton.Enabled = false;
            this.RefreshListButton.Location = new System.Drawing.Point(490, 93);
            this.RefreshListButton.Name = "RefreshListButton";
            this.RefreshListButton.Size = new System.Drawing.Size(136, 23);
            this.RefreshListButton.TabIndex = 10;
            this.RefreshListButton.Text = "Refresh List";
            this.RefreshListButton.UseVisualStyleBackColor = true;
            this.RefreshListButton.Click += new System.EventHandler(this.RefreshListButton_Click);
            // 
            // VpnModeCheckbox
            // 
            this.VpnModeCheckbox.AutoSize = true;
            this.VpnModeCheckbox.Location = new System.Drawing.Point(16, 93);
            this.VpnModeCheckbox.Name = "VpnModeCheckbox";
            this.VpnModeCheckbox.Size = new System.Drawing.Size(155, 17);
            this.VpnModeCheckbox.TabIndex = 11;
            this.VpnModeCheckbox.Text = "Slow Network / VPN Mode";
            this.VpnModeCheckbox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 425);
            this.Controls.Add(this.VpnModeCheckbox);
            this.Controls.Add(this.RefreshListButton);
            this.Controls.Add(this.ShowHelpButton);
            this.Controls.Add(this.JourneyStatusGrid);
            this.Controls.Add(this.SettingButton);
            this.Controls.Add(this.CopyProgress);
            this.Controls.Add(this.StartReviewButton);
            this.Controls.Add(this.LoadProjectButton);
            this.Controls.Add(this.StatusLabel);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(486, 207);
            this.Name = "MainForm";
            this.Text = "Review Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button LoadProjectButton;
        private System.Windows.Forms.FolderBrowserDialog BrowseForProjectDlog;
        private System.Windows.Forms.Button StartReviewButton;
        private System.Windows.Forms.ProgressBar CopyProgress;
        private System.Windows.Forms.Timer ProgressTimer;
        private System.Windows.Forms.Button SettingButton;
        private System.Windows.Forms.PropertyGrid JourneyStatusGrid;
        private System.Windows.Forms.Button ShowHelpButton;
        private System.Windows.Forms.Button RefreshListButton;
        private System.Windows.Forms.CheckBox VpnModeCheckbox;
    }
}

