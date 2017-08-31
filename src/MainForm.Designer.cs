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
            this.StatusLabel = new System.Windows.Forms.Label();
            this.LoadProjectButton = new System.Windows.Forms.Button();
            this.BrowseForProjectDlog = new System.Windows.Forms.FolderBrowserDialog();
            this.BranchSelectMenu = new System.Windows.Forms.ComboBox();
            this.StartReviewButton = new System.Windows.Forms.Button();
            this.EndReviewButton = new System.Windows.Forms.Button();
            this.ResetStateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Location = new System.Drawing.Point(13, 13);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(445, 52);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Checking for git";
            // 
            // LoadProjectButton
            // 
            this.LoadProjectButton.Location = new System.Drawing.Point(16, 68);
            this.LoadProjectButton.Name = "LoadProjectButton";
            this.LoadProjectButton.Size = new System.Drawing.Size(136, 23);
            this.LoadProjectButton.TabIndex = 1;
            this.LoadProjectButton.Text = "&Load Project...";
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
            this.BranchSelectMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchSelectMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BranchSelectMenu.Enabled = false;
            this.BranchSelectMenu.FormattingEnabled = true;
            this.BranchSelectMenu.Location = new System.Drawing.Point(158, 104);
            this.BranchSelectMenu.Name = "BranchSelectMenu";
            this.BranchSelectMenu.Size = new System.Drawing.Size(300, 21);
            this.BranchSelectMenu.TabIndex = 2;
            // 
            // StartReviewButton
            // 
            this.StartReviewButton.Enabled = false;
            this.StartReviewButton.Location = new System.Drawing.Point(16, 103);
            this.StartReviewButton.Name = "StartReviewButton";
            this.StartReviewButton.Size = new System.Drawing.Size(136, 23);
            this.StartReviewButton.TabIndex = 3;
            this.StartReviewButton.Text = "Start Review";
            this.StartReviewButton.UseVisualStyleBackColor = true;
            this.StartReviewButton.Click += new System.EventHandler(this.StartReviewButton_Click);
            // 
            // EndReviewButton
            // 
            this.EndReviewButton.Enabled = false;
            this.EndReviewButton.Location = new System.Drawing.Point(16, 132);
            this.EndReviewButton.Name = "EndReviewButton";
            this.EndReviewButton.Size = new System.Drawing.Size(136, 23);
            this.EndReviewButton.TabIndex = 4;
            this.EndReviewButton.Text = "End Review";
            this.EndReviewButton.UseVisualStyleBackColor = true;
            this.EndReviewButton.Click += new System.EventHandler(this.EndReviewButton_Click);
            // 
            // ResetStateButton
            // 
            this.ResetStateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetStateButton.Enabled = false;
            this.ResetStateButton.Location = new System.Drawing.Point(322, 68);
            this.ResetStateButton.Name = "ResetStateButton";
            this.ResetStateButton.Size = new System.Drawing.Size(136, 23);
            this.ResetStateButton.TabIndex = 5;
            this.ResetStateButton.Text = "Reset Changes";
            this.ResetStateButton.UseVisualStyleBackColor = true;
            this.ResetStateButton.Click += new System.EventHandler(this.ResetStateButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 168);
            this.Controls.Add(this.ResetStateButton);
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
        private System.Windows.Forms.Button ResetStateButton;
    }
}

