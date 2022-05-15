using System;
using System.IO;

namespace FilesharingHOST
{
    public enum QueueType : byte
    {
        Download,
        Upload
    }
    public class TransferQueue
    {
      
        public int id { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public long index { get; set; }
        public long filelength { get; set; }
        public long transfered { get; set; }
        public int percentage { get; set; }
        public string Speed { get; set; }
        public bool isRunning { get; set; }
        public QueueType type { get; set; }
        public string status { get; set; }
        public byte[] buffer { get; set; }

        public FileStream fs;

        public TransferQueue CreateUploadQueue(string path)
        {
            var queue = new TransferQueue();
            queue.path = path;
            queue.filename = Path.GetFileName(path);
            queue.index = 0;
            queue.fs = new FileStream(path, FileMode.Open);
            queue.filelength = queue.fs.Length;
            queue.buffer = new byte[81920];
            queue.transfered = 0;
            queue.percentage = 0;
            queue.isRunning = true;
            queue.type = QueueType.Upload;
            queue.status = "Running";
            return queue;

        }
        public TransferQueue CreateDownloadQueues(string filename,long filelength)
        {
            var queue = new TransferQueue();
            queue.path = @"Transfer\" + filename;
            queue.filename = filename;
            queue.index = 0;
            queue.fs = new FileStream(queue.path, FileMode.OpenOrCreate, FileAccess.Write);
            queue.filelength = filelength;
            queue.fs.SetLength(filelength);
            queue.transfered = 0;
            queue.percentage = 0;
            queue.isRunning = true;
            queue.buffer = new byte[1];
            queue.status = "Running";
            queue.type = QueueType.Download;
            return queue;
        }
    }
}
