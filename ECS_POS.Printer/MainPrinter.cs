using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ECS_POS.Printer
{
    public partial class frmPrinter : Form
    {
        PrintUtility.SerialPortHelp PortHelpCom = null;

        public frmPrinter()
        {
            PortHelpCom = new PrintUtility.SerialPortHelp("COM1", 9600, System.IO.Ports.Parity.None);
            PortHelpCom.Received += new PrintUtility.PortDataReceivedEventHandle(PortHelp_Received);
            PortHelpCom.Error += new System.IO.Ports.SerialErrorReceivedEventHandler(PortHelp_Error);
            PortHelpCom.Open();
            InitializeComponent();
        }

        void PortHelp_Received(object sender, PrintUtility.PortDataReciveEventArgs e)
        { 
            
        }

        void PortHelp_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        { 
            
        }

        private void btnprint_Click(object sender, EventArgs e)
        {
            PortHelpCom.SetEncoding("GBK");
            string a =
@"---------------------------------------
|           威海常温威送中心          |
|                调度单               |
---------------------------------------
排队号：
车号:
发车日期:
配送路径：
载具：_________
";
            PortHelpCom.SendData(txtContent.Text);
            byte[] blst = System.Text.Encoding.GetEncoding("GBK").GetBytes("GS V 65 1");
            byte[] rec = new byte[1024];

            //切纸指令   GS m n  M为41时全切，M为42时半切，n为 1到255至于有啥用，还不知道
            //new byte[] { 0x1D, 0x56, 0x42, 0x01 }

            PortHelpCom.SendData(new byte[] { 0x1D, 0x56, 0x42, 0x01 });
        }
    }
}
