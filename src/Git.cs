using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LittleReviewer
{
    public class Git
    {
        private readonly string _gitExePath;

        public const string TopLevel = "rev-parse --show-toplevel";
        public const string MergeToWorkingCopy = "merge \"{0}\" --no-ff --no-commit -m \"{1}\""; // {0} -> branch name, {1} -> Merge message (used to display code review state)
        public const string Fetch = "fetch --all";
        public const string CheckoutMaster = "checkout master";
        public const string ResetMasterToOrigin = "reset --hard origin/master";
        public const string ResetCurrent = "reset --hard HEAD";
        public const string CurrentBranch = "rev-parse --abbrev-ref HEAD";
        public const string BranchesNotMerged = "branch -r --no-merged master";
        public const string ListUncommittedChanges = "diff-index --name-only HEAD";

        public Git(string gitExePath)
        {
            _gitExePath = gitExePath;
        }

        
        public static string FindGitExe()
        {
            return ExecuteAndReadLine("where", "git.exe");
        }

        private static string ExecuteAndReadLine(string exe, string args, string dir = null)
        {
            using (var p = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = dir ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
            }))
            {
                if (p == null) return null;
                if (!p.WaitForExit(30000)) return null;
                if (p.ExitCode != 0) return null;

                return p.StandardOutput.ReadLine();
            }
        }

        private static string ExecuteAndReadStatus(string exe, string args, string dir = null)
        {
            using (var p = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = dir ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true, RedirectStandardError = true,
                CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
            }))
            {
                if (p == null) return null;
                if (!p.WaitForExit(30000)) return null;
                if (p.ExitCode != 0) return null;

                return p.StandardError.ReadLine() + p.StandardOutput.ReadLine();
            }
        }

        private static List<string> ExecuteAndReadAllLines(string exe, string args, string dir, bool filterExitCode = true)
        {
            using (var p = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = dir ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true, RedirectStandardError = true,
                CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
            }))
            {
                if (p == null) return null;
                if (!p.WaitForExit(30000)) return null;
                if (filterExitCode && p.ExitCode != 0) return null;

                var rawStdout = p.StandardOutput.ReadToEnd();
                var rawStderr = p.StandardError.ReadToEnd();
                string src = (rawStderr.Length > rawStdout.Length) ? (rawStderr) : (rawStdout);

                return src.Split('\r','\n').Where(l=> ! string.IsNullOrWhiteSpace(l)).Select(l=>l.Trim()).ToList();
            }
        }

        public bool TryReadRepo(string repo, out string baseDirectory)
        {
            baseDirectory = ExecuteAndReadLine(_gitExePath, TopLevel, repo);
            return baseDirectory != null;
        }

        public string GetBranchName(string repo)
        {
            return ExecuteAndReadLine(_gitExePath, CurrentBranch, repo);
        }

        public string ResetRepoToOriginMaster(string repo)
        {
            ExecuteAndReadLine(_gitExePath, ResetCurrent, repo);
            ExecuteAndReadLine(_gitExePath, Fetch, repo);
            ExecuteAndReadLine(_gitExePath, CheckoutMaster, repo);
            return ExecuteAndReadLine(_gitExePath, ResetMasterToOrigin, repo);
        }

        public List<string> GetUnmergedBranches(string repo)
        {
            return ExecuteAndReadAllLines(_gitExePath, BranchesNotMerged, repo);
        }

        public void FetchAll(string repo)
        {
            ExecuteAndReadLine(_gitExePath, Fetch, repo);
        }

        public bool HasMergeInProgress(string repo, out string mergeName)
        {
            mergeName = null;

            if (!File.Exists(Path.Combine(repo, ".git\\MERGE_HEAD"))) return false;

            mergeName = File.ReadAllText(Path.Combine(repo, ".git\\MERGE_MSG")).Trim();
            return true;
        }

        public List<string> GetUncommittedChangedFiles(string repo)
        {
            return ExecuteAndReadAllLines(_gitExePath, ListUncommittedChanges, repo, filterExitCode: false);
        }

        public bool TryMergeBranchIntoWorkingCopy(string repo, string branchName, out string status)
        {
            var cmd = string.Format(MergeToWorkingCopy, branchName, "Code review for " + branchName.Replace("origin/",""));
            status = ExecuteAndReadStatus(_gitExePath, cmd, repo);
            return status != null;
        }
    }
}