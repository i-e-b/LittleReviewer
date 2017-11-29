using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using LittleReviewer.DynamicType;

namespace LittleReviewer
{
    public partial class MainForm : Form
    {
        private readonly List<string> AffectedProducts = new List<string>();
        private string BaseDirectory;
        private string CopyReason = "";

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

            TryFindCodeReviewsFolder();

            /*
            // Test stuff:
            //JourneyStatusGrid.SelectedObject = new PropertyObject { Car = DdTest.Four, Home = "Is where the heart is"};

            // -- API Desire --
            var props = DynamicPropertyObject.NewObject();
            props.AddProperty(key: "Property1", displayName: "Property One", description: "This was generated", initialValue: "init value", standardValues: new[] { "Option 1", "Option 2" });

            JourneyStatusGrid.SelectedObject = props;
            JourneyStatusGrid.Refresh();
            // -- end of api desire --
            
            // test:
            var currentValue = ((PropertyTarget) JourneyStatusGrid.SelectedObject)["Property1"];
            MessageBox.Show("Current value: "+currentValue);
            // end test
            */
        }

        /// <summary>
        /// If there is a folder at C:\CodeReview..., try to use that and immediately load
        /// </summary>
        private void TryFindCodeReviewsFolder()
        {
            var reviewFolder = NativeIO.EnumerateFiles("C:\\", ResultType.DirectoriesOnly, "CodeReview*", SearchOption.TopDirectoryOnly, SuppressExceptions.SuppressAllExceptions)
                .FirstOrDefault();

            if (reviewFolder == null) return; // not set up yet, or non-standard folder
            SetupProject(reviewFolder.FullName);
        }

        /// <summary>
        /// This is the main point where we read in the available targets and versions
        /// </summary>
        private void SetupProject(string selectedPath)
        {
            if (new PathInfo(selectedPath).IsRoot)
            {
                Status_SelectedRoot();
                return;
            }

            SetStatus("Working...");
            CopyProgress.Minimum = 0;
            CopyProgress.Maximum = 100;
            CopyProgress.Value = 0;

            BaseDirectory = selectedPath;

            var prs = NativeIO.EnumerateFiles(Paths.PullRequestRoot, ResultType.DirectoriesOnly).Select(f=>f.Name).ToList();

            WriteBranchesToPropertiesGrid(prs);
            State_ReadyToReview();
        }

        private void State_ReadyToReview()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            StartReviewButton.Enabled = true;
            RefreshListButton.Enabled = true;
            SetStatus("Project at " + BaseDirectory +" is ready");
        }

        private void Status_SelectedRoot()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            SetStatus("Don't use the drive root for reviewing. Please create a folder to contain review artifacts.");
        }

        private void State_CantFindShare()
        {
            DisableControls();
            SetStatus("Could not access the build output. Expecting it at '" + Paths.MastersRoot + "'.\r\nPlease check with the dev team.");
        }

        private void StopIIS()
        {
            SetStatus("Stopping IIS");

            string serviceName = "W3SVC"; //W3SVC refers to IIS service
            ServiceController service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped); // Wait till the service started and is running
            }
            SetStatus("IIS Stopped");
        }

        private void StartIIS()
        {
            SetStatus("Starting IIS");

            string serviceName = "W3SVC"; //W3SVC refers to IIS service
            ServiceController service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running); // Wait till the service started and is running
            }
            SetStatus("IIS started");
        }

        private void CopyMastersAndPR(string folderName)
        {
            StopIIS();
            var prDirectory = Path.Combine(Paths.PullRequestRoot, folderName, Paths.PrContainer);
            // The PR drop folder is partly set by TFS. It should be configured to follow:
            //  \\server\share\PullRequests\refs\pull\{ID OF PULL REQUEST}\merge\{FOLDER IN MASTERS TO OVERWRITE}\...contents...

            AffectedProducts.Clear();
            var toCopy = NativeIO.EnumerateFiles(prDirectory, ResultType.DirectoriesOnly).ToList();

            AsyncFile.Copy(Paths.MastersRoot, BaseDirectory, () => {
                foreach (var fileDetail in toCopy)
                {
                    var target = Path.Combine(BaseDirectory, fileDetail.Name);
                    AsyncFile.Copy(fileDetail.FullName, target, null);
                    AffectedProducts.Add(fileDetail.Name);
                }
            });
        }

        private void CopyMastersToLocal()
        {
            StopIIS();
            AsyncFile.Copy(Paths.MastersRoot, BaseDirectory, null);
        }

        private void SetStatus(string msg)
        {
            StatusLabel.Text = msg;
            Refresh();
        }
        
        private void WriteBranchesToPropertiesGrid(List<string> prIds)
        {
            // Read PRs
            var options = new Map<string, string>();
            foreach (var prId in prIds)
            {
                var prDirectory = Path.Combine(Paths.PullRequestRoot, prId, Paths.PrContainer);
                var journeys = NativeIO.EnumerateFiles(prDirectory, ResultType.DirectoriesOnly).Select(f => f.Name).ToList();
                foreach (var journey in journeys)
                {
                    options.Add(journey, prId);
                }
            }

            // Read masters
            var masters = NativeIO.EnumerateFiles(Paths.MastersRoot, ResultType.DirectoriesOnly).Select(f => f.Name).ToList();
            foreach (var journey in masters)
            {
                options.Add(journey, "master");
            }

            // Render to list
            var props = DynamicPropertyObject.NewObject();
            foreach (var journey in options.Keys()) {
                props.AddProperty(key: journey, displayName: journey, description: BuildFileDateDescription(journey),
                    initialValue: "this should have the recorded last state, or blank", standardValues: options.Get(journey));
            }


            JourneyStatusGrid.SelectedObject = props;
            JourneyStatusGrid.Refresh();

            // TODO: This whole thing has to change. Read each PR's journey and add it to that journey's drop downs
        }

        private string BuildFileDateDescription(string journey)
        {
            var sb = new StringBuilder();
            var local = GetApproximateLocalTime(journey);
            var remote = GetApproximateMasterTime(journey);

            if (local == null) sb.Append("Local version is empty");
            else sb.Append("Local version updated      " + local.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            sb.Append("\r\n");
            
            if (remote == null) sb.Append("Remote version is empty");
            else sb.Append("Remote version updated  " + remote.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            return sb.ToString();
        }

        private DateTime? GetApproximateMasterTime(string journey)
        {
            return GetFolderMedianTime(Path.Combine(Paths.MastersRoot, journey));
        }

        private DateTime? GetApproximateLocalTime(string journey)
        {
            return GetFolderMedianTime(Path.Combine(BaseDirectory, journey));
        }

        /// <summary>
        /// Get a median time for file creation at the first level of a directory
        /// </summary>
        private DateTime? GetFolderMedianTime(string path)
        {
            var allDates = NativeIO.EnumerateFiles(path, ResultType.FilesOnly, "*", SearchOption.TopDirectoryOnly, SuppressExceptions.SuppressAllExceptions)
                .Select(f=> f.CreationDate).OrderBy(d=>d)
                .ToArray();

            if (allDates.Length < 1) { // no top level files, try a sample of deep files
                allDates = NativeIO.EnumerateFiles(path, ResultType.FilesOnly, "*", SearchOption.AllDirectories, SuppressExceptions.SuppressAllExceptions)
                    .Take(10)
                    .Select(f=> f.CreationDate).OrderBy(d=>d)
                    .ToArray();
    
                if (allDates.Length < 1) return null; // no files at all
            }
            var idx = allDates.Length / 2;

            return allDates[idx];
        }

        private void DisableControls()
        {
            LoadProjectButton.Enabled = false;
            StartReviewButton.Enabled = false;
            RefreshListButton.Enabled = false;
        }

        private void LoadProjectButton_Click(object sender, EventArgs e)
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

        private void StartReviewButton_Click(object sender, EventArgs e)
        {
            /*if (BranchSelectMenu.SelectedIndex < 1)
            {
                SetStatus("Pick a pull request to review");
                return;
            }

            DisableControls();
            SetStatus("Working...");
            CopyReason = "Updating masters and copying Pull Request";
            CopyMastersAndPR(BranchSelectMenu.Text);*/
            // TODO: This needs to be re-worked. Sync all selected journeys
        }

        private void MainForm_Load(object sender, EventArgs e) { }
        
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (AsyncFile.FilesQueued < 1)
            {
                if (AsyncFile.FilesCopied > 0)
                {
                    var list = string.Join("\r\n", AffectedProducts);
                    SetStatus("Copy complete\r\n"+list);
                    AsyncFile.ResetCounts();
                    StartIIS();
                    State_ReadyToReview();
                }
                CopyProgress.Value = 0;
                return;
            }

            CopyProgress.Value = Math.Min(100,(int)((AsyncFile.FilesCopied / (double) AsyncFile.FilesQueued) * 100));
            SetStatus(CopyReason + ": " + AsyncFile.FilesCopied + " of " + AsyncFile.FilesQueued);
        }

        private void SettingButton_Click(object sender, EventArgs e)
        {
            new PathsForm().ShowDialog();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Help");
        }

        private void RefreshListButton_Click(object sender, EventArgs e)
        {
            SetupProject(BaseDirectory);
        }
    }
}
