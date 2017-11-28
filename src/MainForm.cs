using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using LittleReviewer.DynamicType;

namespace LittleReviewer
{
    public partial class MainForm : Form
    {
        private const string PickPullRequestMessage = "- pick a pull request ID -";
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

            // Test stuff:
            JourneyStatusGrid.SelectedObject = new PropertyObject { Car = DdTest.Four, Home = "Is where the heart is"};

            // Adding default values to Van...
            DynTypeDescriptor.InstallTypeDescriptor(JourneyStatusGrid.SelectedObject);
            var td = DynTypeDescriptor.GetTypeDescriptor(JourneyStatusGrid.SelectedObject);
            if (td == null) throw new Exception("Could not load type descriptor. Have you installed it?");

            var pd = td.GetProperties().Find("Van", true) as DynPropertyDescriptor;
            if (pd == null) throw new Exception("Target property not found");

            pd.Attributes.Add(new TypeConverterAttribute(typeof(DynStandardValueConverter)), true);

            var sv = new DynStandardValue("path 1");
            sv.DisplayName = "Display path 1";
            pd.StandardValues.Add(sv);
            
            sv = new DynStandardValue("path 2");
            sv.DisplayName = "Display path 2";
            pd.StandardValues.Add(sv);


            // Adding a whole new section...
            var pd2 = new DynPropertyDescriptor(JourneyStatusGrid.SelectedObject.GetType(), "Dynamic", typeof(string), "dynamic value"
                ,new BrowsableAttribute(true)
                ,new DisplayNameAttribute("Dynamically generated")
                ,new DescriptionAttribute("This was generated at run time")
                //,new DefaultValueAttribute("")
                );
            
            pd2.Attributes.Add(new TypeConverterAttribute(typeof(DynStandardValueConverter)), true);
            td.GetProperties().Add(pd2);

            sv = new DynStandardValue("dynalt1");
            sv.DisplayName = "Display path 1";
            pd2.StandardValues.Add(sv);
            
            sv = new DynStandardValue("dynalt2");
            sv.DisplayName = "Display path 2";
            pd2.StandardValues.Add(sv);


            JourneyStatusGrid.Refresh();
        }

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
            // test:
            Console.WriteLine(JourneyStatusGrid.SelectedObject);
            // end test
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
            if (BranchSelectMenu.SelectedIndex < 1)
            {
                SetStatus("Pick a pull request to delete");
                return;
            }

            // This should delete the pull request folder
            SetStatus("Deleting Pull Request directory...");
            //NativeIO.DeleteDirectory("", recursive:true);
            var prDirectory = Path.Combine(Paths.PullRequestRoot, BranchSelectMenu.Text);
            Directory.Delete(prDirectory, true);
            SetupProject(BaseDirectory);
        }

        private void StartReviewButton_Click(object sender, System.EventArgs e)
        {
            if (BranchSelectMenu.SelectedIndex < 1)
            {
                SetStatus("Pick a pull request to review");
                return;
            }

            DisableControls();
            SetStatus("Working...");
            CopyReason = "Updating masters and copying Pull Request";
            CopyMastersAndPR(BranchSelectMenu.Text);
        }

        private void EndReviewButton_Click(object sender, System.EventArgs e)
        {
            // The plan: whichever product we copied across, restore that from masters
            // at the moment, we do the lazy thing of copying everything back
            DisableControls();
            SetStatus("Working...");
            CopyReason = "Resetting to most recent masters";
            CopyMastersToLocal();
            SetupProject(BaseDirectory);
        }

        private void MainForm_Load(object sender, System.EventArgs e) { }

        private void BranchSelectMenu_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CleanupReviewButton.Enabled = BranchSelectMenu.SelectedIndex > 0;
        }
        
        private void ProgressTimer_Tick(object sender, System.EventArgs e)
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
    }

    public class PropertyObject
    {
        public DdTest Car { get; set; }
        public string CommonStatic { get; set; }
        public string Home { get; set; }
        public string Legacy { get; set; }
        public string MyGoCompare { get; set; }
        public string Van { get; set; }
        public string WebUI { get; set; }
    }

    public enum DdTest
    {
        [Description("One")] One,
        [Description("Two")] Two,
        [Description("Three")] Three,
        [Description("Four")] Four,
        [Description("Can I have a little more")] AllTogetherNow
    }
}
