using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LittleReviewer
{
    public partial class MainForm : Form
    {
        private const string PickPullRequestMessage = "- pick a pull request ID -";

        private string BaseDirectory;

        public MainForm()
        {
            InitializeComponent();

            if (NativeIO.Exists(Paths.Masters))
            {
                SetStatus("Using masters at " + Paths.MastersRoot + ".");
            }
            else
            {
                State_CantFindShare();
            }
        }

        private void SetupProject(string selectedPath)
        {
            SetStatus("Working...");
            CopyProgress.Minimum = 0;
            CopyProgress.Maximum = 100;
            CopyProgress.Value = 0;

            BaseDirectory = selectedPath;

            var prs = NativeIO.EnumerateFiles(Paths.PullRequestRoot, ResultType.DirectoriesOnly).Select(f=>f.Name).ToList();
            if (prs.Count < 1)
            {
                State_NothingToMerge();
                return;
            }

            WriteBranchesToDropDown(prs);
            State_ReadyToReview();
        }

        private void State_ReadyToReview()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            StartReviewButton.Enabled = true;
            EndReviewButton.Enabled = true;
            BranchSelectMenu.Enabled = true;
            SetStatus("Project at " + BaseDirectory +" is ready");
        }

        private void State_NothingToMerge()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            SetStatus("There are no pull request builds ready to review.");
        }

        private void State_CantFindShare()
        {
            DisableControls();
            SetStatus("Could not access the build output. Expecting it at '" + Paths.MastersRoot + "'.\r\nPlease check with the dev team.");
        }

        private void SetStatus(string msg)
        {
            StatusLabel.Text = msg;
            Refresh();
        }
        

        private void WriteBranchesToDropDown(List<string> branchesAvailable)
        {
            BranchSelectMenu.Items.Clear();
            BranchSelectMenu.Items.Add(PickPullRequestMessage);
            // ReSharper disable once CoVariantArrayConversion
            BranchSelectMenu.Items.AddRange(branchesAvailable.ToArray());
            BranchSelectMenu.SelectedIndex = 0;
        }

        private void DisableControls()
        {
            LoadProjectButton.Enabled = false;
            CleanupReviewButton.Enabled = false;
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

        private void CleanupReviewButton_Click(object sender, System.EventArgs e)
        {
            // This should delete the pull request folder
            SetStatus("NOT YET IMPLEMENTED");
            SetupProject(BaseDirectory);
        }

        private void StartReviewButton_Click(object sender, System.EventArgs e)
        {
            if (BranchSelectMenu.SelectedIndex < 1)
            {
                SetStatus("Pick a branch to review");
                return;
            }
            /*

            // merge branch into wc
            DisableControls();
            SetStatus("Working...");

            string status;
            var ok = git.TryMergeBranchIntoWorkingCopy(BaseDirectory, BranchSelectMenu.Text, out status);

            if (ok)
            {
                SetStatus(status);
                SetupProject(BaseDirectory);
            }
            else
            {
                git.HasMergeInProgress(BaseDirectory, out status);
                State_MergeConflicts(status);
            }*/

            DisableControls();
            SetStatus("Working...");
            CopyMastersToLocal();
            CopyPR(BranchSelectMenu.Text);
        }

        private void CopyPR(string folderName)
        {

        }

        private void CopyMastersToLocal()
        {
            AsyncCopy(source: Paths.MastersRoot, dest: BaseDirectory);
        }


        private void EndReviewButton_Click(object sender, System.EventArgs e)
        {
            // The plan: whichever product we copied across, restore that from masters
            // at the moment, we do the lazy thing of copying everything back
            DisableControls();
            SetStatus("Working...");
            CopyMastersToLocal();
            SetupProject(BaseDirectory);
        }

        private void MainForm_Load(object sender, System.EventArgs e) { }

        private void BranchSelectMenu_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CleanupReviewButton.Enabled = BranchSelectMenu.SelectedIndex > 0;
        }
        
        private volatile int ready = 0;
        private volatile int sent = 0;

        private void AsyncCopy(string source, string dest)
        {
            /* Plan:
               - Flag for when scanning is done
               - Queue of files to be copied

               - spin up a new thread (a) to recurse over the source. It will add files to the queue then flip the flag when done.
               - spin up a new thread (b) that will copy the files until both queue is empty and flag is flipped
            */

            var _lock = new object();
            var fileSubpaths = new Queue<FileDetail>();
            var enumComplete = false;

            new Thread(() => // read files
            {
                var list = NativeIO.EnumerateFiles(source, ResultType.FilesOnly, "*", SearchOption.AllDirectories);
                foreach (var file in list)
                    lock (_lock)
                    {
                        fileSubpaths.Enqueue(file);
                        ready++;
                    }
                enumComplete = true;
            }).Start();

            
            new Thread(() => // write files
            {
                bool hasItems = true;
                while (!enumComplete || hasItems)
                {
                    FileDetail file;
                    lock (_lock)
                    {
                        if (fileSubpaths.Count < 1)
                        {
                            hasItems = false;
                            continue;
                        }
                        hasItems = true;
                        file = fileSubpaths.Dequeue();
                    }

                    var target = file.PathInfo.Reroot(source, dest);
                    NativeIO.CreateDirectory(target.Parent, true);
                    NativeIO.CopyFile(file.PathInfo, target, true);
                    sent++;
                }
                ready = 0; // mark the process done
            }).Start();
        }

        private void ProgressTimer_Tick(object sender, System.EventArgs e)
        {
            if (ready < 1)
            {
                if (sent > 0)
                {
                    SetStatus("Copy complete");
                    sent = 0;
                }
                State_ReadyToReview();
                CopyProgress.Value = 0;
                return;
            }

            CopyProgress.Value = (int)((sent / (double) ready) * 100);
            SetStatus(sent + " of " + ready);
        }
    }
}
