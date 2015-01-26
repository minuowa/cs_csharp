using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using StriveEngine;
using System.Net;
using System.Diagnostics;
using System.Configuration;
using NNOldManNet;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace StriveEngine.SimpleDemoServer
{
/*
 * 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
 *
 * ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
 */
public partial class Form_Server : Form
{
    private Server mNet;
    private Dictionary<Process, Socket> mWorkProcess;
    public Form_Server()
    {
        InitializeComponent();
        InitData();
    }
    ~Form_Server()
    {
        mNet.close();
    }
    private void configNet()
    {
        if (mNet == null)
            mNet = new Server(int.Parse(this.textBox_port.Text));
        mNet.mOnLogInfo += showInfo;
        mNet.mOnReceiveMsg += onClientMsg;
        mNet.mOnConnected += onConnected;
        bool res = mNet.restart();

        this.button1.Enabled = !res;
        this.textBox_port.ReadOnly = true;
        this.button2.Enabled = res;
    }
    private void button1_Click ( object sender, EventArgs e )
    {
        try
        {
            configNet();
        }
        catch ( Exception ee )
        {
            MessageBox.Show ( ee.Message );
        }
    }

    void onConnected ( Socket client, bool sucess )
    {
        if ( this.toolStrip1.InvokeRequired )
        {
            Server.OnConnected myCompare = new Server.OnConnected ( onConnected ); //代理实例化
            this.toolStrip1.Invoke ( myCompare, client, sucess );
        }
        else
        {
            if ( sucess )
            {
                this.comboBox1.Items.Add ( client.RemoteEndPoint.ToString() );
            }
            else
            {
                foreach ( KeyValuePair<Process, Socket> p in mWorkProcess )
                {
                    if ( p.Value == client )
                    {
                        p.Key.OutputDataReceived -= onDataReceivedEventHandler;
                        p.Key.ErrorDataReceived -= onDataReceivedEventHandler;
                        p.Key.Exited -= process_Exited;
                        if (!p.Key.HasExited)
                            p.Key.Kill();
                        mWorkProcess.Remove ( p.Key );
                        break;
                    }
                }
                this.comboBox1.Items.Remove ( client.RemoteEndPoint.ToString() );
                string msg = string.Format ( "{0} 下线", client.RemoteEndPoint.ToString() );
                showInfo ( msg );
            }
        }
    }


    private void showInfo ( string msg )
    {
        if ( this.toolStrip1.InvokeRequired )
        {
            DebugInfo myCompare = new DebugInfo ( showInfo ); //代理实例化
            this.toolStrip1.Invoke ( myCompare, msg );
        }
        else
        {
            this.toolStripLabel_event.Text = msg;
        }
    }

    private void onClientMsg ( Socket client, byte[] msg )
    {
        if ( this.listView1.InvokeRequired )
        {
            Server.ReceiveMsg myCompare = new Server.ReceiveMsg ( onClientMsg ); //代理实例化
            this.listView1.Invoke ( myCompare, client, msg );
        }
        else
        {
            string smsg = Encoding.UTF8.GetString ( msg );
            doWork ( client, smsg );
            ListViewItem item = new ListViewItem ( new string[] { DateTime.Now.ToString(), client.RemoteEndPoint.ToString(), smsg } );
            this.listView1.Items.Insert ( 0, item );
        }
    }
    public void onDataReceivedEventHandler ( object sender, DataReceivedEventArgs e )
    {
        Process process = ( Process ) sender;
        if ( mWorkProcess.ContainsKey ( process ) )
        {
            Socket client = mWorkProcess[process];
            if ( this.mNet.isClientOnline ( client ) && e.Data != null )
            {
                this.mNet.sendMsg ( client, e.Data );
            }
        }
    }
    private void doWork ( Socket client, string msg )
    {
        if ( msg.Length == 0 )
            return;
        try
        {
            Process process = new Process();

            process.StartInfo.FileName = msg;
            process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值
            process.StartInfo.RedirectStandardOutput = true;  //重定向输出流
            process.StartInfo.RedirectStandardError = true;  //重定向错误流
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += onDataReceivedEventHandler;
            process.ErrorDataReceived += onDataReceivedEventHandler;
            process.Exited += process_Exited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var moduleList = process.Modules;
            foreach (System.Diagnostics.ProcessModule module in moduleList)
                Console.WriteLine(string.Format("{0}\n  URL:{1}\n  Version:{2}",
                    module.ModuleName, module.FileName, module.FileVersionInfo.FileVersion));

            mWorkProcess.Add ( process, client );
        }
        catch ( System.Exception ex )
        {
            showInfo(ex.Message);
        }

    }
    void sendMsgToClient ( Socket client, string msg )
    {
        if (this.mNet.isClientOnline ( client ) )
        {
            this.mNet.sendMsg ( client, msg );
        }
    }
    void process_Exited ( object sender, EventArgs e )
    {
        Process process = ( Process ) sender;
        if ( mWorkProcess.ContainsKey ( process ) )
        {
            Socket client = mWorkProcess[process];
            string msg = string.Format ( "{0} :Exit code ( {1} )", process.StartInfo.FileName
                                         , process.ExitCode
                                       );
            sendMsgToClient ( client, msg );
            mWorkProcess.Remove ( process );
        }
    }
    private void ShowConnectionCount ( int clientCount )
    {
        this.toolStripLabel_clientCount.Text = "在线数量： " + clientCount.ToString();
    }



    private void button2_Click ( object sender, EventArgs e )
    {
        try
        {
            Socket client = mNet.getClientByIpAddress ( this.comboBox1.SelectedItem.ToString() );
            if ( client == null )
            {
                MessageBox.Show ( "没有选中任何在线客户端！" );
                return;
            }

            if (!this.mNet.isClientOnline(client))
            {
                MessageBox.Show ( "目标客户端不在线！" );
                return;
            }

            this.mNet.sendMsg ( client, this.textBox_msg.Text );
        }
        catch ( Exception ee )
        {
            MessageBox.Show ( ee.Message );
        }
    }

    private void button3_Click ( object sender, EventArgs e )
    {
        //doWork(TODO,"1234");
        return;
    }

    private void InitData()
    {
        mWorkProcess = new Dictionary<Process, Socket>();
    }

    private void Form1_Load ( object sender, EventArgs e )
    {
    }

    private void Form1_FormClosed ( object sender, FormClosedEventArgs e )
    {
        if ( mNet != null )
        {
            mNet.close();
            mNet = null;
            foreach ( KeyValuePair<Process, Socket> p in mWorkProcess )
            {
                if (!p.Key.HasExited)
                {
                    p.Key.OutputDataReceived -= onDataReceivedEventHandler;
                    p.Key.ErrorDataReceived -= onDataReceivedEventHandler;
                    p.Key.Exited -= process_Exited;
                    p.Key.Kill();
                }
            }
        }
    }
}
}
