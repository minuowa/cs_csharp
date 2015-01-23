using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using StriveEngine;
using StriveEngine.Core;
using StriveEngine.Tcp.Server;
using System.Net;
using System.Diagnostics;
using System.Configuration;
namespace StriveEngine.SimpleDemoServer
{
    /*
     * 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
     * 
     * ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
     */
    public partial class Form1 : Form
    {
        private ITcpServerEngine mNet;
        private Dictionary<Process, IPEndPoint> mWorkProcess;
        public Form1()
        {
            InitializeComponent();
            InitData();
        }
        ~Form1()
        {
            mNet.CloseAllClient();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //初始化并启动服务端引擎（TCP、文本协议）
                this.mNet = NetworkEngineFactory.CreateTextTcpServerEngine(int.Parse(this.textBox_port.Text), new DefaultTextContractHelper("\0"));//DefaultTextContractHelper是StriveEngine内置的ITextContractHelper实现。使用UTF-8对EndToken进行编码。 
                this.mNet.ClientCountChanged += new CbDelegate<int>(tcpServerEngine_ClientCountChanged);
                this.mNet.ClientConnected += new CbDelegate<System.Net.IPEndPoint>(tcpServerEngine_ClientConnected);
                this.mNet.ClientDisconnected += new CbDelegate<System.Net.IPEndPoint>(tcpServerEngine_ClientDisconnected);
                this.mNet.MessageReceived += new CbDelegate<IPEndPoint, byte[]>(tcpServerEngine_MessageReceived);
                this.mNet.Initialize();

                this.button1.Enabled = false;
                this.textBox_port.ReadOnly = true;
                this.button2.Enabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        void tcpServerEngine_MessageReceived(IPEndPoint client, byte[] bMsg)
        {
            string msg = System.Text.Encoding.UTF8.GetString(bMsg); //消息使用UTF-8编码
            msg = msg.Substring(0, msg.Length - 1); //将结束标记"\0"剔除
            this.ShowClientMsg(client, msg);
        }

        void tcpServerEngine_ClientDisconnected(System.Net.IPEndPoint ipe)
        {
            string msg = string.Format("{0} 下线", ipe);
            this.ShowEvent(msg);
        }

        void tcpServerEngine_ClientConnected(System.Net.IPEndPoint ipe)
        {
            string msg = string.Format("{0} 上线", ipe);
            this.ShowEvent(msg);
        }

        void tcpServerEngine_ClientCountChanged(int count)
        {
            this.ShowConnectionCount(count);
        }

        private void ShowEvent(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<string>(this.ShowEvent), msg);
            }
            else
            {
                this.toolStripLabel_event.Text = msg;
            }
        }

        private void ShowClientMsg(IPEndPoint client, string msg)
        {
            if (this.InvokeRequired)
            {
                doWork(client, msg);
                this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, msg);
            }
            else
            {
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), client.ToString(), msg });
                this.listView1.Items.Insert(0, item);
            }
        }
        public void onDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            Process process = (Process)sender;
            if (mWorkProcess.ContainsKey(process))
            {
                IPEndPoint client = mWorkProcess[process];
                if (this.mNet.IsClientOnline(client) && e.Data != null)
                {
                    string msg = e.Data + "\0";// "\0" 表示一个消息的结尾
                    byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                    this.mNet.SendMessageToClient(client, bMsg);
                }
            }
        }
        private void doWork(IPEndPoint client, string msg)
        {
            if (msg.Length == 0)
                return;
           Process process = new Process();

           process.StartInfo.FileName = msg;

           process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序 

           process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 

            //startInfo.RedirectStandardInput = true;  // 重定向输入流 

           process.StartInfo.RedirectStandardOutput = true;  //重定向输出流 

           process.StartInfo.RedirectStandardError = true;  //重定向错误流 

           process.OutputDataReceived += onDataReceivedEventHandler;
           process.ErrorDataReceived += onDataReceivedEventHandler;
           process.Exited += process_Exited;
           process.Start();
           process.BeginOutputReadLine();
           process.BeginErrorReadLine();

           mWorkProcess.Add(process, client);
           //NetProcess netProcess = new NetProcess();
           //netProcess.mProcess = process;
           //netProcess.mClient = client;
        }
        void sendMsgToClient(IPEndPoint client, string msg)
        {
            if (client!=null &&this.mNet.IsClientOnline(client))
            {
                string smsg = msg + "\0";// "\0" 表示一个消息的结尾
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(smsg);//消息使用UTF-8编码
                this.mNet.SendMessageToClient(client, bMsg);
            }
        }
        void process_Exited(object sender, EventArgs e)
        {
            Process process = (Process)sender;
            if (mWorkProcess.ContainsKey(process))
            {
                IPEndPoint client = mWorkProcess[process];
                string msg = string.Format("{0} :Exit code ( {1} )", process.StartInfo.FileName
                    , process.ExitCode
                    );
                sendMsgToClient(client, msg);
            }
        }
        private void ShowConnectionCount(int clientCount)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<int>(this.ShowConnectionCount), clientCount);
            }
            else
            {
                this.toolStripLabel_clientCount.Text = "在线数量： " + clientCount.ToString();
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            List<IPEndPoint> list = this.mNet.GetClientList();
            this.comboBox1.DataSource = list;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                IPEndPoint client = (IPEndPoint)this.comboBox1.SelectedItem;
                if (client == null)
                {
                    MessageBox.Show("没有选中任何在线客户端！");
                    return;
                }

                if (!this.mNet.IsClientOnline(client))
                {
                    MessageBox.Show("目标客户端不在线！");
                    return;
                }

                string msg = this.textBox_msg.Text + "\0";// "\0" 表示一个消息的结尾
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                this.mNet.SendMessageToClient(client, bMsg);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //doWork(TODO,"1234");
            return;
        }

        private void InitData()
        {
            mWorkProcess = new Dictionary<Process, IPEndPoint>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mNet.CloseAllClient();
            mNet.DisposeAsyn();
            foreach (KeyValuePair<Process, IPEndPoint> p in mWorkProcess)
            {
                p.Key.Kill();
            }
        }

    }
}
