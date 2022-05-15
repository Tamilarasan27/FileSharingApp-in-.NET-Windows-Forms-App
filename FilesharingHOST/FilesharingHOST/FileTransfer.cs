using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace FilesharingHOST
{

   public class FileSender:PacketWriter
    {
        NetworkStream ns;
        public FileSender(NetworkStream ns)
        {
            this.ns = ns;
        }
        public void Fileinfo(long fileLength,string filename)
        {
            MemoryStream ms = new MemoryStream();
            byte[] cmdBuff = BitConverter.GetBytes(151);
            ms.Write(cmdBuff, 0, cmdBuff.Length);
            ms.Write(BitConverter.GetBytes(fileLength), 0, BitConverter.GetBytes(fileLength).Length);
            ms.Write(Encoding.UTF8.GetBytes(filename), 0, Encoding.UTF8.GetBytes(filename).Length);
            Filesend(createDataPackets(ms.ToArray().Length, ms.ToArray()));
            ms.Close();
        }
        public long FileReader(TransferQueue queue)
        {
            MemoryStream ms = new MemoryStream();
            if (queue.index < queue.filelength && queue.isRunning && queue.filelength > queue.buffer.Length)
            {
                
                if (queue.transfered+queue.buffer.Length >= queue.filelength)
                {
                    int size = Convert.ToInt32(queue.filelength - queue.transfered);
                    queue.buffer = new byte[size];
                }
                
                    
                    queue.fs.Seek(queue.index, SeekOrigin.Begin);
                    queue.fs.Read(queue.buffer, 0, queue.buffer.Length);

                    byte[] cmdBuff = BitConverter.GetBytes(152);
                    byte[] indexBuff = BitConverter.GetBytes(queue.index);
                    ms.Write(cmdBuff, 0, cmdBuff.Length);
                    ms.Write(indexBuff, 0, indexBuff.Length);
                    ms.Write(queue.buffer, 0, queue.buffer.Length);
                    Filesend(createDataPackets(ms.ToArray().Length, ms.ToArray()));

                ms.Close();
                return queue.buffer.Length;
            }
            else if (queue.filelength < queue.buffer.Length && queue.isRunning)
            {
                queue.buffer = new byte[queue.filelength];
                queue.fs.Seek(queue.index, SeekOrigin.Begin);
                queue.fs.Read(queue.buffer, 0, queue.buffer.Length);

                byte[] cmdBuff = BitConverter.GetBytes(152);
                byte[] indexBuff = BitConverter.GetBytes(queue.index);
                ms.Write(cmdBuff, 0, cmdBuff.Length);
                ms.Write(indexBuff, 0, indexBuff.Length);
                ms.Write(queue.buffer, 0, queue.buffer.Length);
                Filesend(createDataPackets(ms.ToArray().Length, ms.ToArray()));
                queue.isRunning = false;
                ms.Close();
                return queue.buffer.Length;
            }
            else
            {
                queue.isRunning = false;
                ms.Close();
                return 0;
            }
        }
        public void Filesend(byte[] sendBuff)
        {
            try
            {
                lock (sendBuff)
                {
                    ns.Write(sendBuff, 0, sendBuff.Length);
                    ns.Flush();
                }

            }
            catch(Exception err)
            {
                Debug.WriteLine(err);
            }


        }
        
    }
    public class FileRec:PackerReader
    {
        TransferQueue Recqueue = new TransferQueue();
        public FileRec(string filepath)
        {
            path = filepath; 
        }
        public TransferQueue RecData(byte[] buffer, bool isRunning,NetworkStream ns)
        {

                if (isRunning == true && ns.ReadByte() == 2)
                {
                    int b = 0;
                    byte[] bufferlenbuff = new byte[4];
                    int i = 0;
                    while ((b = ns.ReadByte()) != 4)
                    {
                        bufferlenbuff[i] = (byte)b;
                        i++;
                    }
                    int bufferLen = BitConverter.ToInt32(bufferlenbuff, 0);
                    buffer = new byte[bufferLen];
                    int byte_read = 0;
                    int byte_offset = 0;
                    while (byte_offset < bufferLen)
                    {
                        byte_read = ns.Read(buffer, byte_offset, bufferLen - byte_offset);
                        byte_offset += byte_read;
                    }
                Recqueue = readDataPackets(buffer);
                if(Recqueue != null)
                {
                    FileWriter(Recqueue.buffer,Recqueue.index);
                }
                

                return Recqueue;
            }
            else
            {
                Debug.WriteLine("ELSE");
                return null;
            }
            

        }
       
        private void FileWriter(byte[] buff, long index)
        {
            try
            {
                if (Recqueue.transfered < Recqueue.filelength)
                {
                    lock (buff)
                    {
                        Recqueue.index = index;
                        Recqueue.fs.Position = index;
                        Recqueue.fs.Write(buff, 0, buff.Length);
                        Recqueue.transfered += buff.Length;
                        Recqueue.percentage = (int)Math.Round(((decimal)(Recqueue.transfered * 100) / Recqueue.filelength));
                        Recqueue.Speed = Convert.ToString((Recqueue.transfered / 1024) / 1024) + "MB" + " of " + Convert.ToString((Recqueue.filelength / 1024) / 1024) + "MB";
                    }

                }
                if(Recqueue.transfered >= Recqueue.filelength)
                {
                    Recqueue.isRunning = false;
                    Recqueue.status = "Completed";
                    Recqueue.fs.Close();
                }
            }
            catch
            {
                Recqueue.status = "Falied";
            }
        }
    }
}
