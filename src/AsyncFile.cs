using System;
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

        public static void ResetCounts()
        {
            ready = 0;
            sent = 0;
        }

        public static void Copy(string source, string dest)
        {
            /*
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
                Interlocked.Increment(ref jobs);

                var sourceInfo = new PathInfo(source);
                var kind = NativeIO.GetTargetKind(sourceInfo);
                switch (kind) {
                    case FileOrDirectory.Nothing:
                        enumComplete = true;
                        break;

                    case FileOrDirectory.File:
                        lock (_lock)
                        {
                            source = Path.GetDirectoryName(source);
                            fileSubpaths.Enqueue(NativeIO.ReadFileDetails(sourceInfo));
                            enumComplete = true;
                        }
                        break;

                    case FileOrDirectory.Directory:
                        var list = NativeIO.EnumerateFiles(source, ResultType.FilesOnly, "*", SearchOption.AllDirectories).ToList();
                        foreach (var file in list)
                            lock (_lock)
                            {
                                fileSubpaths.Enqueue(file);
                                Interlocked.Increment(ref ready);
                            }
                        enumComplete = true;
                        break;
                }
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