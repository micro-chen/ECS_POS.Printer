using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;

namespace ECS_POS.PrintUtility
{
    /**/
    /// <summary>
    /// 串口类，IPort是抽像的端口类
    /// </summary>
    public class ParallelPort : IDisposable
    {

        // inpout32相关
        #region inpout32相关
        [DllImport("inpout32.dll", EntryPoint = "Out32")]
        public static extern void Output(uint adress, int value);

        [DllImport("inpout32.dll", EntryPoint = "Inp32")]
        public static extern int Input(uint adress);
        [StructLayout(LayoutKind.Sequential)]
        private struct OVERLAPPED
        {
            int Internal;
            int InternalHigh;
            int Offset;
            int OffSetHigh;
            int hEvent;
        }
        #endregion

        //win32 API
        #region win32 API
        [DllImport("kernel32.dll ")]
        private static extern int CreateFile(
          string lpFileName,
          uint dwDesiredAccess,
          int dwShareMode,
          int lpSecurityAttributes,
          int dwCreationDisposition,
          int dwFlagsAndAttributes,
          int hTemplateFile
          );
        [DllImport("kernel32.dll ")]
        private static extern bool WriteFile(
          int hFile,
          byte[] lpBuffer,
          int nNumberOfBytesToWrite,
          ref  int lpNumberOfBytesWritten,
          ref  OVERLAPPED lpOverlapped
          );
        [DllImport("kernel32.dll ")]
        private static extern bool CloseHandle(
          int hObject
          );
        [DllImport("kernel32.dll ")]
        private static extern bool ReadFile(
            int hFile,
            out byte[] lpBuffer,
            int nNumberOfBytesToRead,
            ref int lpNumberOfBytesRead,
            ref OVERLAPPED lpOverlapped
          );

        private int iHandle;

        private bool _isWork;

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        #endregion
        //IPort 成员
        #region IPort 成员


        private bool _IsOpen;
        public bool IsOpen
        {
            get
            {
                return _IsOpen;
            }
            private set
            {
                _IsOpen = value;
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public bool WriteData(byte[] Data)
        {
            //for (int i = 0; i < Data.Length; i++)
            //    Output(BasePort, Data); 这里原来也想用inpout32实现，但是从字节到int转换比较麻烦，试了几次没达到效果
            //return true;

            if (iHandle != -1)
            {
                OVERLAPPED x = new OVERLAPPED();
                int i = 0;
                WriteFile(iHandle, Data, Data.Length,
                  ref  i, ref  x);
                return true;
            }
            else
            {
                //throw new Exception("不能连接到打印机! ");
                return false;
            }
        }


        /**/
        /// <summary>
        /// 读状态
        /// 用inpout32
        /// </summary>
        /// <param name="Len"></param>
        /// <returns></returns>
        public byte[] ReadData(int Len)
        {
            byte[] result;//= new byte[Len];
            result = new byte[Len];
            for (int i = 0; i < result.Length; i++)
                result[i] = (byte)Input(BasePort + 1);
            return result;
        }


        /**/
        /// <summary>
        /// 打开端口
        /// </summary>
        public void Open()
        {
            iHandle = CreateFile(Name, 0x40000000, 0, 0, 3, 0, 0);
            if (iHandle != -1)
            {
                this.IsOpen = true;
            }
            else
            {
                this.IsOpen = false;
            }

            this.IsOpen = true;
            _isWork = true;
            //开一个线程检测状态口状态
            new System.Threading.Thread(new System.Threading.ThreadStart(ReadState)).Start();
        }


        /**/
        /// <summary>
        /// 关闭端口
        /// </summary>
        public void Close()
        {
            this.IsOpen = !CloseHandle(iHandle);
            _isWork = false;
        }

        /**/
        /// <summary>
        /// 端口基址
        /// </summary>
        private uint BasePort;
        internal ParallelPort(String portName)
        {
            ///用wql查询串口基址
            ///用wql查询串口基址
            ManagementObjectSearcher search2 =
                    new ManagementObjectSearcher(
                        "ASSOCIATORS OF {Win32_ParallelPort.DeviceID='" + this.Name + "'}");
            //本来最佳的wql是ASSOCIATORS OF {Win32_ParallelPort.DeviceID='" + this.Name  + "'} WHERE ASSCICLASS = Win32_PortResource
            //但是不知道为什么不返回结果，所以就做一个简单的遍历吧
            foreach (ManagementObject i in search2.Get())
            {

                if (i.ClassPath.ClassName == "Win32_PortResource")
                {
                    //得到串口基址 大多数是0x378H
                    this.BasePort = System.Convert.ToUInt32(i.Properties["StartingAddress"].Value.ToString());

                    break;
                }


            }
            if (BasePort == 0)
                throw new Exception("不是有效端口");
            IsOpen = false;
        }

        #endregion


        #region IPort Members

        //public event PortStateChanged StateChanged;

        //public event PortDataReceived DataReceive;

        //public System.Windows.Forms.Form Parent
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        private int a;

        /**/
        /// <summary>
        /// 检测线程，当状态改变时，引发事件
        /// </summary>
        private void ReadState()
        {

            a = 0;
            int lastRead = a;
            while (_isWork)
            {
                lastRead = a;
                a = Input(BasePort + 1);
                if (a != lastRead)
                {
                    //if (this.StateChanged != null)
                    //{
                    //    PortChangedEvntAvrgs e = new PortChangedEvntAvrgs();
                    //    e.PortStatusByte = a;
                    //    //this.StateChanged(this, e);
                    //}

                }
                System.Threading.Thread.Sleep(500);

            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Close();
        }

        #endregion

        #region IPort Members


        public void Update()
        {
            a = 0;
        }

        #endregion
    }
}
