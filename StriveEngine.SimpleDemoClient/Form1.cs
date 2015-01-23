using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using StriveEngine.Core;
using StriveEngine.Tcp.Passive;
using System.Diagnostics;
using System.Xml;

namespace StriveEngine.SimpleDemoClient
{
/*
* 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
*
* ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
*/
public partial class Form1 : Form
{
    private ITcpPassiveEngine mNet;
    private string mClientIP = "127.0.0.1";
    private int mPort = 9999;

    ToolStripLabel mTS = new ToolStripLabel();
    //配置文件
    private static string mConfigFile = "config.xml";
    private string mRecvMsgs;
    private Dictionary<string, string> mCmds = new Dictionary<string, string>();
    public Form1()
    {
        InitializeComponent();
        loadConfig();
        config();
    }
    public void loadConfig()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load ( mConfigFile );

        XmlElement root = null;
        root = doc.DocumentElement;
        if ( root == null )
            return;

        XmlNodeList listNodes = null;
        listNodes = root.SelectNodes ( "/config/client" );
        if ( listNodes != null )
        {
            XmlNode ele1 = listNodes[0];
            mClientIP = ele1.Attributes["ip"].Value;
            mPort = int.Parse ( ele1.Attributes["port"].Value );
        }
        listNodes = root.SelectNodes ( "/config/cmd/item" );

        foreach ( XmlNode node in listNodes )
        {
            string cmd = node.Attributes["type"].Value;
            mCmds.Add ( cmd, node.Attributes["content"].Value );
        }
    }
    private void button3_Click ( object sender, EventArgs e )
    {
        try
        {
            //初始化并启动客户端引擎（TCP、文本协议）
            this.mNet = NetworkEngineFactory.CreateTextTcpPassiveEngine ( this.textBox_IP.Text, int.Parse ( this.textBox_port.Text ), new DefaultTextContractHelper ( "\0" ) );
            this.mNet.MessageReceived += new CbDelegate<System.Net.IPEndPoint, byte[]> ( tcpPassiveEngine_MessageReceived );
            this.mNet.AutoReconnect = true;//启动掉线自动重连
            this.mNet.ConnectionInterrupted += new CbDelegate ( tcpPassiveEngine_ConnectionInterrupted );
            this.mNet.ConnectionRebuildSucceed += new CbDelegate ( tcpPassiveEngine_ConnectionRebuildSucceed );
            this.mNet.Initialize();

            this.button_send.Enabled = true;
            this.button_connect.Enabled = false;

            setState ( "连接成功！" );
        }
        catch ( Exception ee )
        {
            MessageBox.Show ( ee.Message );
        }
    }

    void tcpPassiveEngine_ConnectionRebuildSucceed()
    {
        if ( this.InvokeRequired )
        {
            this.BeginInvoke ( new CbDelegate ( this.tcpPassiveEngine_ConnectionInterrupted ) );
        }
        else
        {
            this.button_send.Enabled = true;
            setState ( "重连成功。" );
        }
    }
    void setState ( string state )
    {
        mTS.Text = state;
    }
    void tcpPassiveEngine_ConnectionInterrupted()
    {
        if ( this.InvokeRequired )
        {
            this.BeginInvoke ( new CbDelegate ( this.tcpPassiveEngine_ConnectionInterrupted ) );
        }
        else
        {
            this.button_send.Enabled = false;
            setState ( "您已经掉线。" );
        }
    }

    void tcpPassiveEngine_MessageReceived ( System.Net.IPEndPoint serverIPE, byte[] bMsg )
    {
        string msg = System.Text.Encoding.UTF8.GetString ( bMsg ); //消息使用UTF-8编码
        msg = msg.Substring ( 0, msg.Length - 1 ); //将结束标记"\0"剔除
        this.ShowMessage ( msg );
    }

    private void ShowMessage ( string msg )
    {
        if ( this.InvokeRequired )
        {
            this.BeginInvoke ( new CbDelegate<string> ( this.ShowMessage ), msg );
        }
        else
        {
            mRecvMsgs += msg + "\r\n";
            textBox_recv.Text = mRecvMsgs;
        }
    }

    private void button2_Click ( object sender, EventArgs e )
    {
        string type = this.comboBox_cmd.SelectedItem.ToString();
        string content = mCmds[type];
        string msg = content + "\0";// "\0" 表示一个消息的结尾
        byte[] bMsg = System.Text.Encoding.UTF8.GetBytes ( msg ); //消息使用UTF-8编码
        this.mNet.SendMessageToServer ( bMsg );
    }

    private void textBox_msg_TextChanged ( object sender, EventArgs e )
    {

    }

    private void Form1_Load ( object sender, EventArgs e )
    {
    }

    private void textBox1_TextChanged ( object sender, EventArgs e )
    {
        if ( checkBox_autoScrollToButtom.Checked )
        {
            this.textBox_recv.Select ( textBox_recv.Text.Length, 0 );
            textBox_recv.ScrollToCaret();
        }
    }

    private void config()
    {
        foreach ( KeyValuePair<string, string>  cmd in mCmds )
        {
            comboBox_cmd.Items.Add ( cmd.Key );
        }
        textBox_IP.Text = mClientIP;
        textBox_port.Text = string.Format ( "{0}", mPort );
        textBox_IP.Enabled = false;
        textBox_port.Enabled = false;

        statusStrip1.Items.Add ( mTS );
    }

}
}
