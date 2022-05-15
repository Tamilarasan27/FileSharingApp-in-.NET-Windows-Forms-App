using System;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace FilesharingHOST
{
   public class PacketWriter
    {
        MemoryStream ms;
        public byte[] createDataPackets(int datalength, byte[] data)
        {
             ms = new MemoryStream();

            byte[] datalenbuff = BitConverter.GetBytes(datalength);
            byte[] intial = new byte[1];
            byte[] seperator = new byte[1];
            intial[0] = 2;
            seperator[0] = 4;

            ms.Write(intial, 0, intial.Length);
            ms.Write(datalenbuff, 0, datalenbuff.Length);
            ms.Write(seperator, 0, seperator.Length);
            ms.Write(data, 0, data.Length);
            return ms.ToArray();
        }
    }
    public class PackerReader
    {
        TransferQueue recqueue;
        public string path { get; set; }
        public TransferQueue readDataPackets(byte[] readBuffer)
        {
            try
            {
                lock (readBuffer)
                {
                   
                    byte[] cmdBuff = new byte[4];
                    Buffer.BlockCopy(readBuffer, 0, cmdBuff, 0, 4);
                    int cmd = BitConverter.ToInt32(cmdBuff, 0);
                    switch (cmd)
                    {
                        case 151:
                            {

                                byte[] filelenBuff = new byte[8];
                                Buffer.BlockCopy(readBuffer, 4, filelenBuff, 0, filelenBuff.Length);
                                //
                                long filelength = BitConverter.ToInt64(filelenBuff, 0);

                                byte[] fileNameBuff = new byte[readBuffer.Length - 12];
                                Buffer.BlockCopy(readBuffer, 12, fileNameBuff, 0, fileNameBuff.Length);
                                //
                                string filename = Encoding.UTF8.GetString(fileNameBuff);

                                if (filename != null)
                                {
                                    recqueue = new TransferQueue();
                                    recqueue = recqueue.CreateDownloadQueues(filename, filelength);
                                }
                               
                                break;
                            }
                        case 152:
                            {
                                byte[] indexBuff = new byte[8];
                                Buffer.BlockCopy(readBuffer, 4, indexBuff, 0, indexBuff.Length);
                                recqueue.buffer = new byte[readBuffer.Length - 12];
                                Buffer.BlockCopy(readBuffer, 12, recqueue.buffer, 0, recqueue.buffer.Length);
                                recqueue.index = BitConverter.ToInt64(indexBuff, 0);
                                cmd = 0;
                                break;
                            }
                        default:
                            {
                               
                                cmd = 0;
                                break;
                            }
                            
                    }
                    return recqueue;

                }

                
            }
            
            catch (ArgumentNullException err)
            {
                Debug.WriteLine(err);
                recqueue.status = "Falied";
                return null;
            }
            
        }
    }
}
