namespace LittleReviewer
{
    public class Paths
    {
        public static string PullRequestRoot = @"\\web020.testing.gocompare.local\wwwroot\Reviews\PullRequests\refs\pull";
        public static string PrContainer = @"merge";
        public static string MastersRoot = @"\\web020.testing.gocompare.local\wwwroot\Reviews\Masters";

        public static PathInfo Masters {
            get { return new PathInfo(MastersRoot); }
        }
    }
}
