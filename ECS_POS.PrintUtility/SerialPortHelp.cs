using System;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;

namespace ECS_POS.PrintUtility
{
    public class SerialPortHelp
    {
        public event PortDataReceivedEventHandle Received;
        public event SerialErrorReceivedEventHandler Error;
        public SerialPort port;
        public bool ReceiveEventFlag = false;  //接收事件是否有效 false表示有效

        public SerialPortHelp(string sPortName, int baudrate, Parity parity)
        {
            port = new SerialPort(sPortName, baudrate, parity, 8, StopBits.One);
            port.RtsEnable = true;
            port.ReadTimeout = 3000;
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            port.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorEvent);
        }

        ~SerialPortHelp()
        {
            Close();
        }

        public void Open()
        {
            if (!port.IsOpen)
            {
                port.Open();
            }
        }

        public void Close()
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }

        public void SetEncoding(string codename)
        {
            port.Encoding = Encoding.GetEncoding(codename);
        }

        public void SetEncoding(Encoding encoding)
        {
            port.Encoding = encoding;
        }
        //数据发送
        public void SendData(byte[] data)
        {
            if (port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
        }
        public void SendData(byte[] data, int offset, int count)
        {
            if (port.IsOpen)
            {
                port.Write(data, offset, count);
            }
        }

        //数据发送
        public void SendData(string Text)
        {
            if (port.IsOpen)
            {
                port.WriteLine(Text);
            }
        }

        //发送命令
        public int SendCommand(byte[] SendData, ref  byte[] ReceiveData, int Overtime)
        {

            if (port.IsOpen)
            {
                ReceiveEventFlag = true;        //关闭接收事件
                port.DiscardInBuffer();         //清空接收缓冲区                 
                port.Write(SendData, 0, SendData.Length);
                int num = 0, ret = 0;
                while (num++ < Overtime)
                {
                    if (port.BytesToRead >= ReceiveData.Length) break;
                    System.Threading.Thread.Sleep(1);
                }
                if (port.BytesToRead >= ReceiveData.Length)
                    ret = port.Read(ReceiveData, 0, ReceiveData.Length);
                ReceiveEventFlag = false;       //打开事件
                return ret;
            }
            return -1;
        }

        public void ErrorEvent(object sender, SerialErrorReceivedEventArgs e)
        {
            if (Error != null) Error(sender, e);
        }

        //数据接收
        public void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //禁止接收事件时直接退出
            if (ReceiveEventFlag) return;

            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);
            if (Received != null) Received(sender, new PortDataReciveEventArgs(data));
        }

        public bool IsOpen()
        {
            return port.IsOpen;
        }
    }

    public delegate void PortDataReceivedEventHandle(object sender, PortDataReciveEventArgs e);

    public class PortDataReciveEventArgs : EventArgs
    {
        public PortDataReciveEventArgs()
        {
            this.data = null;
        }

        public PortDataReciveEventArgs(byte[] data)
        {
            this.data = data;
        }

        private byte[] data;

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
