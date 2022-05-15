using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesharingHOST
{
    public partial class LockForm : Form
    {
        public LockForm()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "12345678")
            {
                Form1 form = new Form1();
                form.Show();
                
            }
            else
            {
                MessageBox.Show("Please Enter Vaild Password...");
            }
            
        }
    }
}
