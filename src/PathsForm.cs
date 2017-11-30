using System.Windows.Forms;

namespace LittleReviewer
{
    public partial class PathsForm : Form
    {
        public PathsForm()
        {
            InitializeComponent();
            PullRequestRootTextBox.Text = Paths.PullRequestRoot;
            MastersRootTextBox.Text = Paths.MastersRoot;
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            Paths.PullRequestRoot = PullRequestRootTextBox.Text;
            Paths.MastersRoot = MastersRootTextBox.Text;
            Close();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
