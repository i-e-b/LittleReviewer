﻿using System;
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
        private string BaseDirectory;
        private string CopyReason = "";
        private const char RecordSeparator = '\x1E';

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
            SetStatus("Project at " + BaseDirectory +" is ready");
            State_ReadyToReview();
        }

        private void State_ReadyToReview()
        {
            DisableControls();
            LoadProjectButton.Enabled = true;
            StartReviewButton.Enabled = true;
            RefreshListButton.Enabled = true;
            VpnModeCheckbox.Enabled = true;
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

        private void DisableControls()
        {
            LoadProjectButton.Enabled = false;
            StartReviewButton.Enabled = false;
            RefreshListButton.Enabled = false;
            VpnModeCheckbox.Enabled = false;
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

        /// <summary>
        /// Given a dictionary of src=>dst, copy files async
        /// </summary>
        private void CopyFiles(List<StateTransition> copiesToMake)
        {
            if (copiesToMake == null || copiesToMake.Count < 1) {
                SetStatus("Nothing to synchronise");
                State_ReadyToReview();
                return;
            }

            StopIIS(); // IIS is restarted in `ProgressTimer_Tick` when all copies have completed

            AsyncFile.UnpackArchiveFiles = VpnModeCheckbox.Checked; // if true, Async file will try to unpack anything called 'Archive.7z'

            foreach (var change in copiesToMake)
            {
                var displayName = change.JourneyName+"->"+change.RequestedState;
                if (VpnModeCheckbox.Checked && NativeIO.Exists(new PathInfo(Path.Combine(change.RemotePath, "Archive.7z")))) {
                    // copy just the archive, then expand as a post-process.
                    AsyncFile.Copy(Path.Combine(change.RemotePath, "Archive.7z"), change.LocalPath, displayName);
                } else {
                    AsyncFile.Copy(change.RemotePath, change.LocalPath, displayName);
                }
            }
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
                options.Add(journey, "(ignore)");
            }

            // Render to list
            var states = ReadLastKnownStates();
            var props = DynamicPropertyObject.NewObject();
            foreach (var journey in options.Keys()) {
                var currentValue = GetOrDefault(states, journey, "");

                props.AddProperty(key: journey, displayName: journey, description: BuildFileDateDescription(journey),
                    initialValue: currentValue, standardValues: options.Get(journey));
            }


            JourneyStatusGrid.SelectedObject = props;
            JourneyStatusGrid.Refresh();
        }

        private string GetOrDefault(Dictionary<string, string> states, string journey, string defaultValue)
        {
            if (states == null || ! states.ContainsKey(journey)) return defaultValue;
            return states[journey];
        }

        private Dictionary<string, string> ReadLastKnownStates()
        {
            try {
                var lines = File.ReadAllLines(Path.Combine(BaseDirectory, "LastKnownStates.txt"));
                var states = new Dictionary<string,string>();
                foreach (var line in lines)
                {
                    var bits = line.Split(RecordSeparator);
                    if (bits.Length != 2) continue;
                    states.Add(bits[0], bits[1]);
                }
                return states;
            }
            catch {
                return new Dictionary<string, string>();
            }
        }
        
        /// <summary>
        /// Feed a dictionary of journey=>state
        /// </summary>
        private void WriteNewStates(List<StateTransition> updates)
        {
            var existing = ReadLastKnownStates();
            foreach (var change in updates)
            {
                if (existing.ContainsKey(change.JourneyName)) existing[change.JourneyName] = change.RequestedState;
                else existing.Add(change.JourneyName, change.RequestedState);
            }
            var sb = new StringBuilder();
            foreach (var kvp in existing)
            {
                sb.Append(kvp.Key);
                sb.Append(RecordSeparator);
                sb.AppendLine(kvp.Value);
            }
            try {
                File.WriteAllText(Path.Combine(BaseDirectory, "LastKnownStates.txt"), sb.ToString());
            }
            catch { Ignore(); }
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
                .Select(f=> f.ModifiedDate).OrderBy(d=>d)
                .ToArray();

            if (allDates.Length < 1) { // no top level files, try a sample of deep files
                allDates = NativeIO.EnumerateFiles(path, ResultType.FilesOnly, "*", SearchOption.AllDirectories, SuppressExceptions.SuppressAllExceptions)
                    .Take(10)
                    .Select(f=> f.ModifiedDate).OrderBy(d=>d)
                    .ToArray();
    
                if (allDates.Length < 1) return null; // no files at all
            }
            var idx = allDates.Length / 2;

            return allDates[idx];
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
            DisableControls();
            var props = ((PropertyTarget)JourneyStatusGrid.SelectedObject);
            var journeys = props.ListProperties();

            // The PR drop folder is partly set by TFS. It should be configured to follow:
            //  \\server\share\PullRequests\refs\pull\{ID OF PULL REQUEST}\merge\{FOLDER IN MASTERS TO OVERWRITE}\...contents...

            var changes = new List<StateTransition>();
            foreach (var journey in journeys)
            {
                var value = props[journey].ToString().ToLowerInvariant();
                var targetDir = Path.Combine(BaseDirectory, journey);

                if (value == "master") {
                    var sourceDir = Path.Combine(Paths.MastersRoot, journey);
                    changes.Add(new StateTransition{
                        JourneyName = journey,
                        LocalPath = targetDir,
                        RemotePath = sourceDir,
                        RequestedState = value
                        });
                } else if (value == "(ignore)" || string.IsNullOrWhiteSpace(value)) {
                    // do nothing
                } else {
                    var sourceDir = Path.Combine(Paths.PullRequestRoot, value, Paths.PrContainer, journey);
                    changes.Add(new StateTransition{
                        JourneyName = journey,
                        LocalPath = targetDir,
                        RemotePath = sourceDir,
                        RequestedState = value
                    });
                }
            }

            CopyReason = "Synchronising";
            CopyFiles(changes);
            WriteNewStates(changes);
        }

        private void MainForm_Load(object sender, EventArgs e) { }
        
        private static readonly object ProgressLock = new object();
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            lock (ProgressLock)
            {
                if (AsyncFile.JobsWaiting < 1)
                {
                    if (AsyncFile.FilesCopied > 0)
                    {
                        var failed = AsyncFile.FailedSources();
                        SetStatus("Copy complete");
                        AsyncFile.ResetCounts();

                        if (failed.Count > 0) {
                            var list = string.Join("    \r\n", failed);
                            MessageBox.Show("Some sources no longer exist. Consider changing them to 'master':\r\n"+list);
                        }

                        SetStatus("Copy complete");
                        AsyncFile.ResetCounts();
                        StartIIS();
                        SetupProject(BaseDirectory);
                    }
                    CopyProgress.Value = 0;
                    return;
                }

                if (AsyncFile.FilesQueued > 0)
                {
                    CopyProgress.Value = Math.Min(100, (int)((AsyncFile.FilesCopied / (double)AsyncFile.FilesQueued) * 100));
                    SetStatus(CopyReason + ": " + AsyncFile.FilesCopied + " of " + AsyncFile.FilesQueued);
                }
            }
        }

        private void SettingButton_Click(object sender, EventArgs e)
        {
            new PathsForm().ShowDialog();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            new HelpScreen().Show();
        }

        private void RefreshListButton_Click(object sender, EventArgs e)
        {
            DisableControls();
            SetupProject(BaseDirectory);
        }

        private void Ignore() { }
    }
}
