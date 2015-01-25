﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml;
using NNOldManNet;

namespace StriveEngine.SimpleDemoClient
{
/*
* 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
*
* ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
*/
public partial class Form1 : Form
{
    private Client mNet;
    private string mClientIP = "127.0.0.1";
    private int mPort = 10000;

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
    private void reContect()
    {
        if ( mNet == null )
        {
            mNet = new Client();
            mNet.mOnExpection += setState;
            mNet.mOnReceiveMessage += onReceive;
            mNet.mOnConnect += onConnect;
        }
        try
        {
            mNet.reconnect ( "127.0.0.1", mPort );


        }
        catch ( System.Exception e )
        {
            mNet.close();
            MessageBox.Show ( e.Message );
        }
    }

    void onConnect ( bool sucessed, bool local )
    {
        if ( this.button_send.InvokeRequired )
        {
            NNOldManNet.Client.OnConnect myCompare = new NNOldManNet.Client.OnConnect ( onConnect ); //代理实例化
            this.textBox_recv.Invoke ( myCompare, sucessed, local );
        }
        else
        {
            this.button_send.Enabled = sucessed;
            this.button_connect.Enabled = !sucessed;
            string st1 = sucessed ? "连接成功" : "断开连接！";
            string st2 = local ? "(本地)" : "(远程)";
            setState ( st1 + st2 );
        }
    }

    void onReceive ( byte[] bmsg )
    {
        if ( this.textBox_recv.InvokeRequired )
        {
            NNOldManNet.Client.OnReceiveMessage myCompare = new NNOldManNet.Client.OnReceiveMessage ( onReceive ); //代理实例化
            this.textBox_recv.Invoke ( myCompare, bmsg );
        }
        else
        {
            string smsg = System.Text.Encoding.UTF8.GetString ( bmsg );
            mRecvMsgs = textBox_recv.Text + smsg + "\n\r\n";
            textBox_recv.Text = mRecvMsgs;
        }
    }

    void onExpection ( string msg )
    {
        setState ( msg );
    }
    private void button3_Click ( object sender, EventArgs e )
    {
        reContect();
    }
    void setState ( string state )
    {
        if ( this.statusStrip1.InvokeRequired )
        {
            DebugInfo myCompare = new DebugInfo ( setState ); //代理实例化
            this.statusStrip1.Invoke ( myCompare, state );
        }
        else
        {
            mTS.Text = state;
        }
    }
    //void setState(string state)
    //{
    //    if (this.statusStrip1.InvokeRequired)
    //    {
    //        DebugInfo myCompare = new DebugInfo(setState); //代理实例化
    //        this.statusStrip1.Invoke(myCompare, state);
    //    }
    //    else
    //    {
    //        mTS.Text = state;
    //    }
    //}

    private void button2_Click ( object sender, EventArgs e )
    {
        if ( this.comboBox_cmd.SelectedItem != null )
        {
            string type = this.comboBox_cmd.SelectedItem.ToString();
            string content = mCmds[type];
            this.mNet.sendMsg ( content );
        }
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
