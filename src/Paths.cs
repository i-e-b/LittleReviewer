namespace LittleReviewer
{
    public class Paths
    {
        public const string PullRequestRoot = @"\\172.16.0.34\NuGet$\PullRequests\refs\pull";
        public const string PrContainer = @"merge";
        public const string MastersRoot = @"\\172.16.0.34\NuGet$\Masters";

        public static PathInfo Masters {
            get { return new PathInfo(MastersRoot); }
        }
    }
}
