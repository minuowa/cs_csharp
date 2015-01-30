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
using System.Security.Principal;
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
    private List<Process> mProcessPool;
    public static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal ( identity );
        return principal.IsInRole ( WindowsBuiltInRole.Administrator );
    }
    public Form_Server()
    {
        bool isAs = IsAdministrator();
        if ( isAs )
        {
            isAs = true;
        }
        InitializeComponent();
        InitData();
    }
    ~Form_Server()
    {
        mNet.close();
    }
    private void configNet()
    {
        if ( mNet == null )
            mNet = new Server ( int.Parse ( this.textBox_port.Text ) );
        mNet.mOnLogInfo += showInfo;
        mNet.mOnReceiveMsg += processPKG;
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
                //foreach ( Process p in mProcessPool )
                //{
                //    {
                //        p.OutputDataReceived -= onDataReceivedEventHandler;
                //        p.ErrorDataReceived -= onDataReceivedEventHandler;
                //        p.Exited -= process_Exited;
                //        if ( !p.HasExited )
                //            p.Kill();
                //        mProcessPool.Remove ( p );
                //        break;
                //    }
                //}
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
    private void processPKG ( Socket client, PKG pkg )
    {
        switch ( ( PKGID ) pkg.mType )
        {
        case PKGID.StartWork:
        {
            onStartWorkMsg ( client, pkg );
        }
        break;
        case PKGID.StopWork:
        {
            onStopWorkMsg ( client, pkg );
        }
        break;
        }
    }
    private void onStartWorkMsg ( Socket client, PKG pkg )
    {

        if ( this.listView1.InvokeRequired )
        {
            Server.ReceiveMsg myCompare = new Server.ReceiveMsg ( onStartWorkMsg ); //代理实例化
            this.listView1.Invoke ( myCompare, client, pkg );
        }
        else
        {
            doWork ( client, pkg.getDataString() );
            ListViewItem item = new ListViewItem ( new string[] { DateTime.Now.ToString(), client.RemoteEndPoint.ToString(), pkg.getDataString() } );
            this.listView1.Items.Insert ( 0, item );
        }
    }

    private void onStopWorkMsg ( Socket client, PKG pkg )
    {
        foreach ( Process p in mProcessPool )
        {
            if ( p.StartInfo.FileName == pkg.getDataString() )
            {
                if ( !p.HasExited )
                {
                    ProcessTreeNode root = new ProcessTreeNode ( p );
                    root.Kill();
                    return;
                }
            }
        }
    }
    public void onDataReceivedEventHandler ( object sender, DataReceivedEventArgs e )
    {
        if ( e.Data != null )
        {
            mNet.broadcast ( new PKG ( PKGID.NormalOutPut, e.Data ) );
        }
        //Process process = ( Process ) sender;
        //if ( mWorkProcess.ContainsKey ( process ) )
        //{
        //    Socket client = mWorkProcess[process];
        //    if ( this.mNet.isClientOnline ( client ) && e.Data != null )
        //    {
        //        this.mNet.sendMsg ( client, e.Data );
        //    }
        //}
    }
    private void doWork ( Socket client, string msg )
    {
        if ( msg.Length == 0 )
            return;

        foreach ( Process p in mProcessPool )
        {
            if ( p.StartInfo.FileName == msg )
            {
                sendMsgToClient ( client, "任务正在进行！" );
                return;
            }
        }

        try
        {
            Process process = new Process();

            process.StartInfo.FileName = msg;
            process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值
            process.StartInfo.RedirectStandardOutput = true;  //重定向输出流
            process.StartInfo.RedirectStandardError = true;  //重定向错误流
            process.StartInfo.Verb = "runas";  //
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += onDataReceivedEventHandler;
            process.ErrorDataReceived += onDataReceivedEventHandler;
            process.Exited += process_Exited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            mProcessPool.Add ( process );
        }
        catch ( System.Exception ex )
        {
            showInfo ( ex.Message );
        }

    }
    void sendMsgToClient ( Socket client, string msg )
    {
        if ( this.mNet.isClientOnline ( client ) )
        {
            PKG pkg = new PKG ( PKGID.NormalOutPut );
            pkg.setData ( msg );
            this.mNet.sendMsg ( client, pkg );
        }
    }
    void process_Exited ( object sender, EventArgs e )
    {
        Process process = ( Process ) sender;
        if ( mProcessPool.Contains ( process ) )
        {
            try
            {
                //Socket client = mProcessPool[process];
                string msg = string.Format ( "{0} :Exit code ( {1} )", process.StartInfo.FileName
                                             , process.ExitCode
                                           );
                //sendMsgToClient ( client, msg );
                mNet.broadcast ( new PKG ( PKGID.NormalOutPut, msg ) );
            }
            catch ( System.Exception ex )
            {
                showInfo ( ex.Message );
            }
            mProcessPool.Remove ( process );
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

            if ( !this.mNet.isClientOnline ( client ) )
            {
                MessageBox.Show ( "目标客户端不在线！" );
                return;
            }
            PKG pkg = new PKG ( PKGID.NormalOutPut );
            pkg.setData ( this.textBox_msg.Text );
            this.mNet.sendMsg ( client, pkg );
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
        mProcessPool = new List<Process>();
    }

    private void Form1_Load ( object sender, EventArgs e )
    {
        this.comboBox1.ItemHeight = 25;
    }

    private void Form1_FormClosed ( object sender, FormClosedEventArgs e )
    {
        if ( mNet != null )
        {
            mNet.close();
            mNet = null;
            foreach ( Process p in mProcessPool )
            {
                if ( !p.HasExited )
                {
                    p.OutputDataReceived -= onDataReceivedEventHandler;
                    p.ErrorDataReceived -= onDataReceivedEventHandler;
                    p.Exited -= process_Exited;
                    ProcessTreeNode n = new ProcessTreeNode ( p );
                    n.Kill();
                }
            }
        }
    }
}
}
