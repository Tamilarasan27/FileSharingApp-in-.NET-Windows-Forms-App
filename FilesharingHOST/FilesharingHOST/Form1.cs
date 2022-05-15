using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;

namespace FilesharingHOST
{
    public partial class Form1 : Form
    {
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        FileSender filesender;
        FileRec filerec;
        TransferQueue sendqueue = new TransferQueue();
        TransferQueue recqueue = new TransferQueue();
       // Thread sendthread;
       // public  Dictionary<string, TransferQueue> my_dict = new Dictionary<string, TransferQueue>();
        string[] data = new string[6];
        byte recbyte;
        int itemNo;
        public Form1()
        {
            InitializeComponent();
            itemNo = 1;
            recbyte = 1;
            
        }

        [Obsolete]
        private  void btn_START_Click(object sender, EventArgs e)
        {
            btn_START.Enabled = false;
            btn_Connect.Enabled = false;
            listener = new TcpListener(8888);
            listener.Start();
            client = listener.AcceptTcpClient();
            MessageBox.Show("Client Connected");
            ns = client.GetStream();
            filerec = new FileRec(@"Transfer\");
            timer2.Start();
        }

        private void btn_Browse_Click(object sender, EventArgs e)
        {
           
        }
        private void btn_SEND_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "openFileDialog1")
            btn_SEND.Enabled = false;
            filesender = new FileSender(ns);
            sendqueue = sendqueue.CreateUploadQueue(openFileDialog1.FileName);
            filesender.Fileinfo(sendqueue.filelength, sendqueue.filename);
            sendqueue.id += itemNo;
            addItems(sendqueue);
            itemNo++;
            timer1.Start();
           

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
             SendFile();   
        }
       

        private void addItems(TransferQueue queue)
        {
            data[0] = Convert.ToString(queue.id);
            data[1] = queue.filename;
            data[2] = Convert.ToString(queue.type);
            data[3] = Convert.ToString(queue.percentage);
            data[4] = Convert.ToString(queue.Speed);
            data[5] = queue.status;
            ListViewItem item = new ListViewItem(data);
            listView1.Items.Add(item);
        }
        private void updateItems(TransferQueue queue)
        {
            listView1.Items[queue.id - 1].SubItems[3].Text = Convert.ToString(queue.percentage) + "%";
            listView1.Items[queue.id - 1].SubItems[4].Text = Convert.ToString(queue.Speed);
            listView1.Items[queue.id - 1].SubItems[5].Text = queue.status;
            
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            btn_Connect.Enabled = false;
            btn_START.Enabled = false;
            label1.Text = "Client";
            client = new TcpClient();
            client.Connect("127.0.0.1", 8888);
            ns = client.GetStream();
            filerec = new FileRec(@"Transfer\");
            timer2.Start();
        }
        
        private void SendFile()
        {
            sendqueue.index += filesender.FileReader(sendqueue);
            sendqueue.percentage = (int)Math.Round((decimal)(sendqueue.transfered * 100) / sendqueue.filelength);
            sendqueue.Speed = Convert.ToString((sendqueue.transfered / 1024) / 1024) + "MB" + " of " + Convert.ToString((sendqueue.filelength / 1024) / 1024) + "MB";
            sendqueue.transfered = sendqueue.index;
            updateItems(sendqueue);
            if (sendqueue.transfered >= sendqueue.filelength)
            {
               
                sendqueue.status = "Completed";
                updateItems(sendqueue);
                sendqueue.fs.Close();
                timer1.Stop();
                btn_SEND.Enabled = true;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            RecFile();
        }
        private void RecFile()
        {
            if (ns.DataAvailable && recbyte == 1)
            {
                byte[] buffer = new byte[1];
                var queue = filerec.RecData(buffer, true, ns);
                if (queue != null && queue.filename != null)
                {
                    Debug.WriteLine("rec1");
                    recqueue = queue;
                    recqueue.id = itemNo;
                    recqueue.status = "Running";
                    addItems(recqueue);
                    itemNo++;
                    recbyte = 2;
                }


            }
            else if (recbyte == 2)
            {
                var queue = filerec.RecData(recqueue.buffer, recqueue.isRunning, ns);

                if (queue != null)
                {
                    recqueue = queue;
                    updateItems(recqueue);
                    if (recqueue.isRunning == false)
                    {
                      
                        Debug.WriteLine("Rec Stop");
                        recbyte = 1;
                    }
                }


            
        }
    }

    } 
}

