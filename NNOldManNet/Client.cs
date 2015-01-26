using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace NNOldManNet
{

public class Client
{
    public delegate void OnReceiveMessage ( byte[] bmsg );
    public delegate void OnConnect ();
    public delegate void OnDisconnectLocal();
    public delegate void OnDisconnectRemote();
    public delegate void OnOutOfReach();

    public event OnReceiveMessage mOnReceiveMessage;

    public event DebugInfo mOnExpection;

    //连接成功
    public event OnConnect mOnConnect;
    //本地网络断开
    public event OnDisconnectLocal mOnDisconnectLocal;
    //远程网络断开
    public event OnDisconnectRemote mOnDisconnectRemote;
    //远程服务器未开启
    public event OnOutOfReach mOnOutOfReach;


    private int mHeartTime = 0;
    private System.Timers.Timer mTimer;

    public const UInt32 mReceiveBufferLength = 8192;
    public byte[] mBuffer = new byte[mReceiveBufferLength];

    private Socket mNet;

    ~Client()
    {
        close();
    }
    public void close()
    {
        mHeartTime = 0;
        if ( mNet != null )
        {
            mNet.Close();
            mNet = null;
        }
        if ( mTimer != null )
        {
            mTimer.Stop();
            mTimer.Dispose();
            mTimer = null;
        }
    }
    public void sendMsg ( string smsg )
    {
        byte[] bmsg = System.Text.Encoding.UTF8.GetBytes ( smsg );
        sendMsg ( bmsg );
    }
    public void sendMsg ( byte[] bmsg )
    {
        try
        {
            if ( mNet != null && mNet.Connected )
            {
                mNet.Send ( bmsg );
            }
        }
        catch ( System.Exception ex )
        {
            mOnExpection ( ex.Message );
        }

    }
    public  void onTimer ( object sender, ElapsedEventArgs e )
    {
        Int32 t = Environment.TickCount;
        if ( mHeartTime > 0 && Math.Abs ( t - mHeartTime ) >= Config.HART_BEAT_LIMIT )
        {
            close();
            mOnDisconnectLocal();
            return;
        }
        sendMsg ( Config.HART_BEAT );
    }
    public void onHeartBeat()
    {
        mHeartTime = Environment.TickCount;
    }

    public void reconnect ( string ipa, int port )
    {
        try
        {
            mOnExpection += onLogInfo;
            mOnOutOfReach += onOutOfReach;
            mOnDisconnectLocal += onDisconnectLocal;
            mOnDisconnectRemote += onDisconnectRemote;
            mOnConnect += onConnect;
            close();
            mNet = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            mNet.Connect ( ipa, port );
            mNet.BeginReceive ( mBuffer, 0, mBuffer.Length, SocketFlags.None, onRecv, mNet );
            mTimer = new System.Timers.Timer ( Config.HART_BEAT_PERIOD );
            mTimer.Elapsed += new ElapsedEventHandler ( onTimer );
            mTimer.Start();
        }
        catch ( System.Exception ex )
        {
            mOnExpection ( ex.Message );
            mOnOutOfReach();
            close();
        }
    }

    void onConnect()
    {
        this.mOnExpection("连接成功！");
    }

    void onDisconnectRemote()
    {
        this.mOnExpection("与服务器断开连接！");
    }

    void onDisconnectLocal()
    {
        this.mOnExpection("本地连接断开！");
    }


    public void onRecv ( IAsyncResult ar )
    {
        try
        {
            Socket socket = ( Socket ) ar.AsyncState;

            int length = 0;
            try
            {
                length = socket.EndReceive ( ar );
            }
            catch ( System.Exception ex )
            {
                mOnExpection ( ex.Message );
                mOnDisconnectRemote();
                close();
                return;
            }
            byte[] bmsg = new byte[length];
            Array.Copy ( mBuffer, bmsg, length );
            if ( length > 0 )
            {
                if ( bmsg.Length == Config.HART_BEAT.Length && Config.HART_BEAT == Encoding.UTF8.GetString ( bmsg ) )
                {
                    onHeartBeat();
                }
                else if ( bmsg.Length == Config.SUCESS.Length && Config.SUCESS == Encoding.UTF8.GetString ( bmsg ) )
                {
                    mOnConnect();
                }
                else
                {
                    mOnReceiveMessage ( bmsg );
                }
            }
            else
            {

            }
            mNet.BeginReceive ( mBuffer, 0, mBuffer.Length, SocketFlags.None, onRecv, mNet );
        }
        catch ( Exception ex )
        {
            mOnExpection ( ex.Message );
        }
    }
    private void onLogInfo(string info)
    {
        Console.WriteLine(info);
    }
    void onOutOfReach()
    {
        this.mOnExpection("服务器为开启！");
    }
}
}
