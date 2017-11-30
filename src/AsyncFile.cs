using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SharpCompress.Archives;
using SharpCompress.Readers;

namespace LittleReviewer
{
    public static class AsyncFile
    {
        public static bool UnpackArchiveFiles { get; set; }

        public static int FilesQueued {
            get { return ready; }
        }

        public static int FilesCopied {
            get { return sent; }
        }

        public static int JobsWaiting {
            get { return jobs; }
        }

        private static int ready;
        private static int sent;
        private static int jobs;
        private static readonly List<string> failedSources = new List<string>();

        public static void ResetCounts()
        {
            ready = 0;
            sent = 0;
            failedSources.Clear();
        }

        /// <summary>
        /// Return any sources that could not be found
        /// </summary>
        public static ICollection<string> FailedSources(){
            return failedSources.ToArray();
        }

        public static void Copy(string source, string dest, string displayName)
        {
            /*
               - Flag for when scanning is done
               - Queue of files to be copied

               - spin up a new thread 
                   (a) to recurse over the source. It will add files to the queue then flip the flag when done.
                   (b) that will copy the files until both queue is empty and flag is flipped
            */


            new Thread(() => // read files
            {
                var _lock = new object();
                var fileSubpaths = new Queue<FileDetail>();
                Interlocked.Increment(ref jobs);

                var sourceInfo = new PathInfo(source);
                var kind = NativeIO.GetTargetKind(sourceInfo);
                switch (kind) {
                    case FileOrDirectory.Nothing:
                        // Nothing found. Increment the counts so we don't freeze up.
                        Interlocked.Increment(ref sent);
                        Interlocked.Increment(ref ready);
                        failedSources.Add(displayName); // store the missing thing
                        break;

                    case FileOrDirectory.File:
                        lock (_lock)
                        {
                            source = Path.GetDirectoryName(source);
                            fileSubpaths.Enqueue(NativeIO.ReadFileDetails(sourceInfo));
                        }
                        break;

                    case FileOrDirectory.Directory:
                        var list = NativeIO.EnumerateFiles(source, ResultType.FilesOnly, "*", SearchOption.AllDirectories);
                        foreach (var file in list)
                            lock (_lock)
                            {
                                fileSubpaths.Enqueue(file);
                                Interlocked.Increment(ref ready);
                            }
                        break;
                }

                bool hasItems = true;
                while (hasItems)
                {
                    FileDetail file;
                    lock (_lock)
                    {
                        if (fileSubpaths.Count < 1)
                        {
                            hasItems = false;
                            continue;
                        }
                        file = fileSubpaths.Dequeue();
                    }

                    var target = file.PathInfo.Reroot(source, dest);
                    NativeIO.CreateDirectory(target.Parent, true);
                    NativeIO.CopyFile(file.PathInfo, target, true);

                    if (UnpackArchiveFiles && file.Name == "Archive.7z") { // it's a special archive
                        // unpack...
                        var archive = ArchiveFactory.Open(file.FullName);
                        var entries = archive.Entries.ToList();
                        ready += entries.Count;
                        foreach (var entry in entries)
                        {
                            if (entry.IsDirectory) continue;
                            entry.WriteToDirectory(target.Parent.FullName, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            Interlocked.Increment(ref sent);
                        }
                    }

                    Interlocked.Increment(ref sent);
                }
                Interlocked.Decrement(ref jobs);
            }).Start();
        }
    }
}