using System;
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
public partial class Form_Client : Form
{
    private Client mNet;
    private string mClientIP = "127.0.0.1";
    private int mPort = 10000;
    int mSelectIndex = 0;

    //配置文件
    private static string mConfigFile = "config.xml";
    private string mRecvMsgs;
    private Dictionary<string, string> mCmds = new Dictionary<string, string>();

    ToolStripLabel mTS = new ToolStripLabel();
    public Form_Client()
    {
        loadConfig();
        InitializeComponent();
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
            mNet.mOnDisconnectLocal += onDisconnect;
            mNet.mOnDisconnectRemote += onDisconnect;
        }
        try
        {
            mNet.reconnect ( mClientIP, mPort );
        }
        catch ( System.Exception e )
        {
            mNet.close();
            MessageBox.Show ( e.Message );
        }
    }

    void onDisconnect()
    {
        if ( this.button_start.InvokeRequired )
        {
            NNOldManNet.Client.OnDisconnectLocal myCompare = new NNOldManNet.Client.OnDisconnectLocal ( onDisconnect ); //代理实例化
            this.textBox_recv.Invoke ( myCompare );
        }
        else
        {
            this.button_start.Enabled = false;
            this.button_stop.Enabled = false;
            this.button_connect.Enabled = true;
        }
    }

    void onConnect ()
    {
        if ( this.button_start.InvokeRequired )
        {
            NNOldManNet.Client.OnConnect myCompare = new NNOldManNet.Client.OnConnect ( onConnect ); //代理实例化
            this.textBox_recv.Invoke ( myCompare );
        }
        else
        {
            this.button_start.Enabled = true;
            this.button_stop.Enabled = true;
            this.button_connect.Enabled = false;
        }
    }

    void onReceive ( PKG pkg )
    {
        if ( this.textBox_recv.InvokeRequired )
        {
            NNOldManNet.Client.OnReceiveMessage myCompare = new NNOldManNet.Client.OnReceiveMessage ( onReceive ); //代理实例化
            this.textBox_recv.Invoke ( myCompare, pkg );
        }
        else
        {
            switch ( ( PKGID ) pkg.mType )
            {
            case PKGID.NormalOutPut:
            {
                string smsg = DateTime.Now.ToString() + " : " +  pkg.getDataString();
                mRecvMsgs = textBox_recv.Text + smsg + "\r\n";
                textBox_recv.AppendText ( smsg + "\r\n" );
            }
            break;
            case PKGID.CurTaskAdd:
            {
                string task = pkg.getDataString();
                foreach ( var v in this.comboBox_taskList.Items )
                {
                    if ( v.ToString() == task )
                        return;
                }
                comboBox_taskList.Items.Add ( task );
                comboBox_taskList.Text = task;
            }
            break;
            case PKGID.CurTaskDelete:
            {
                string task = pkg.getDataString();
                comboBox_taskList.Items.Remove ( task );
            }
            break;
            }
        }
    }

    void onExpection ( string msg )
    {
        setState ( msg );
    }
    private void button_connect_Click ( object sender, EventArgs e )
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

    private void button_start_Click ( object sender, EventArgs e )
    {
        if ( this.comboBox_cmd.SelectedItem != null )
        {
            string type = this.comboBox_cmd.SelectedItem.ToString();
            string content = mCmds[type];
            this.mNet.sendMsg ( new PKG ( PKGID.StartWork, content )  );
        }
    }

    private void textBox1_TextChanged ( object sender, EventArgs e )
    {
        if ( checkBox_autoScrollToButtom.Checked )
        {
            mSelectIndex = textBox_recv.Text.Length;
            this.textBox_recv.Select ( mSelectIndex, 0 );
        }
        //textBox_recv.ScrollToCaret();
    }

    private void config()
    {
        this.comboBox_cmd.IntegralHeight = true;
        this.comboBox_cmd.ItemHeight = 30;

        foreach ( KeyValuePair<string, string> cmd in mCmds )
        {
            comboBox_cmd.Items.Add ( cmd.Key );
        }
        comboBox_cmd.SelectedIndex = 0;
        textBox_IP.Text = mClientIP;
        textBox_port.Text = string.Format ( "{0}", mPort );
        textBox_IP.Enabled = false;
        textBox_port.Enabled = false;

        statusStrip1.Items.Add ( mTS );
    }

    private void button_stop_Click ( object sender, EventArgs e )
    {
        if ( this.comboBox_cmd.SelectedItem != null )
        {
            string type = this.comboBox_cmd.SelectedItem.ToString();
            string content = mCmds[type];
            this.mNet.sendMsg ( new PKG ( PKGID.StopWork, content ) );
        }
    }

    private void label2_Click ( object sender, EventArgs e )
    {

    }

}
}
