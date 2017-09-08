using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LittleReviewer
{
    public static class AsyncFile
    {
        public static int FilesQueued {
            get { return ready; }
        }

        public static int FilesCopied {
            get { return sent; }
        }

        private static volatile int ready;
        private static volatile int sent;

        public static void ResetCounts()
        {
            ready = 0;
            sent = 0;
        }

        public static void Copy(string source, string dest, Action done)
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
                if (done == null) ready = 0;
                else done();
            }).Start();
        }
    }
}