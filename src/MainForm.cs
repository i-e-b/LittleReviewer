using System.Windows.Forms;

namespace LittleReviewer
{
    public partial class MainForm : Form
    {
        private const string PickABranchMessage = "- pick a branch -";

        private readonly Git git;
        private string BaseDirectory;

        public MainForm()
        {
            InitializeComponent();
            var gitPath = Git.FindGitExe();

            if (gitPath == null)
            {
                State_CantFindGit();
                return;
            }

            git = new Git(gitPath);
            SetStatus("Using git at " + gitPath + ".");
        }

        private void SetupProject(string selectedPath)
        {
            SetStatus("Working...");
            var ok = git.TryReadRepo(selectedPath, out BaseDirectory);
            if (!ok)
            {
                State_NotARepo(selectedPath);
                return;
            }


            var currentBranch = git.GetBranchName(BaseDirectory);
            if (currentBranch != "master")
            {
                State_NotInMaster();
                return;
            }

            string mergeName;
            var inMerge = git.HasMergeInProgress(BaseDirectory, out mergeName);
            if (inMerge)
            {
                State_InMerge(mergeName);
                return;
            }

            git.FetchAll(BaseDirectory);
            var branchesAvailable = git.GetUnmergedBranches(BaseDirectory);
            if (branchesAvailable.Count < 1)
            {
                State_NothingToMerge();
                return;
            }

            BranchSelectMenu.Items.Clear();
            BranchSelectMenu.Items.Add(PickABranchMessage);
            // ReSharper disable once CoVariantArrayConversion
            BranchSelectMenu.Items.AddRange(branchesAvailable.ToArray());
            BranchSelectMenu.SelectedIndex = 0;
            State_ReadyToReview();

        }

        private void State_ReadyToReview()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            StartReviewButton.Enabled = true;
            BranchSelectMenu.Enabled = true;
            SetStatus("Project at " + BaseDirectory +" is ready");
        }

        private void State_InMerge(string mergeName)
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            EndReviewButton.Enabled = true;
            SetStatus("A review has been started in this project: "+mergeName+"\r\nUse Visual Studio to rebuild before testing.");
        }

        private void State_NothingToMerge()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            SetStatus("There are no unmerged branches to review.");
        }

        private void State_CantFindGit()
        {
            DisableControls();
            SetStatus("No 'git.exe' was found. Please check it is installed and on the path");
        }

        private void State_NotInMaster()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            ResetStateButton.Enabled = true;
            SetStatus("Project at '" + BaseDirectory + "' is not ready for reviews. Press 'Reset State' to undo changes. Changes will be lost.");
        }

        private void State_NotARepo(string selectedPath)
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            SetStatus("Directory '" + selectedPath + "' is not a git repo.");
        }



        private void SetStatus(string msg)
        {
            StatusLabel.Text = msg;
            Refresh();
        }


        private void DisableControls()
        {
            LoadProjectButton.Enabled = false;
            ResetStateButton.Enabled = false;
            StartReviewButton.Enabled = false;
            EndReviewButton.Enabled = false;
            BranchSelectMenu.Enabled = false;
        }

        private void LoadProjectButton_Click(object sender, System.EventArgs e)
        {
            var result = BrowseForProjectDlog.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    SetupProject(BrowseForProjectDlog.SelectedPath);
                    break;
            }
        }

        private void ResetStateButton_Click(object sender, System.EventArgs e)
        {
            // reset head, checkout master, fetch, reset to origin
            DisableControls();
            SetStatus(git.ResetRepoToOriginMaster(BaseDirectory));
            SetupProject(BaseDirectory);
        }

        private void StartReviewButton_Click(object sender, System.EventArgs e)
        {
            if (BranchSelectMenu.SelectedIndex < 1)
            {
                SetStatus("Pick a branch to review");
                return;
            }

            // merge branch into wc
            DisableControls();
            SetStatus("Working...");
            SetStatus(git.MergeBranchIntoWorkingCopy(BaseDirectory, BranchSelectMenu.Text));
            SetupProject(BaseDirectory);
        }

        private void EndReviewButton_Click(object sender, System.EventArgs e)
        {
            // checkout master, fetch, reset to origin
            DisableControls();
            SetStatus("Working...");
            SetStatus(git.ResetRepoToOriginMaster(BaseDirectory));
            SetupProject(BaseDirectory);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {

        }
    }
}
