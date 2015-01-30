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
    public delegate void OnReceiveMessage ( PKG pkg );
    public delegate void OnConnect ();
    public delegate void OnDisconnectLocal();
    public delegate void OnDisconnectRemote();
    public delegate void OnOutOfReach();

    public event OnReceiveMessage mOnReceiveMessage;

    public event DebugInfo mOnExpection;

    /// <summary>
    /// 连接成功
    /// </summary>
    public event OnConnect mOnConnect;
    /// <summary>
    /// 本地网络断开
    /// </summary>
    public event OnDisconnectLocal mOnDisconnectLocal;
    /// <summary>
    /// 远程网络断开
    /// </summary>
    public event OnDisconnectRemote mOnDisconnectRemote;
    /// <summary>
    /// 远程服务器未开启
    /// </summary>
    public event OnOutOfReach mOnOutOfReach;


    private int mHeartTime = 0;
    private System.Timers.Timer mTimer;

    public const UInt32 mReceiveBufferLength = 8192;
    private byte[] mBuffer = new byte[mReceiveBufferLength];
    private byte[] mTail = null;

    private Socket mNet;

    ~Client()
    {
        close();
    }
    /// <summary>
    /// 关闭网络
    /// </summary>
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
    /// <summary>
    /// 发送消息包
    /// </summary>
    /// <param name="pkg"></param>
    public void sendMsg ( PKG pkg )
    {
        sendMsg ( pkg.getBuffer() );
    }
    /// <summary>
    /// 发送消息包
    /// </summary>
    /// <param name="smsg"></param>
    private void sendMsg ( string smsg )
    {
        byte[] bmsg = Config.Encodinger.GetBytes ( smsg );
        sendMsg ( bmsg );
    }
    private void sendMsg ( byte[] bmsg )
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
    private void onTimer ( object sender, ElapsedEventArgs e )
    {
        Int32 t = Environment.TickCount;
        if ( mHeartTime > 0 && Math.Abs ( t - mHeartTime ) >= Config.HEART_BEAT_LIMIT )
        {
            close();
            mOnDisconnectLocal();
            return;
        }
        PKG pkg = new PKG ( PKGID.HeartBeat );
        sendMsg ( pkg );
    }
    private void onHeartBeat()
    {
        mHeartTime = Environment.TickCount;
    }

    /// <summary>
    /// 重连
    /// </summary>
    /// <param name="ipa"></param>
    /// <param name="port"></param>
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
            mTimer = new System.Timers.Timer ( Config.HEART_BEAT_PERIOD );
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
        this.mOnExpection ( "连接成功！" );
    }

    void onDisconnectRemote()
    {
        this.mOnExpection ( "与服务器断开连接！" );
    }

    void onDisconnectLocal()
    {
        this.mOnExpection ( "本地连接断开！" );
    }

    private void processPKG ( PKG pkg )
    {
        switch ( ( PKGID ) pkg.mType )
        {
        case PKGID.HeartBeat:
        {
            onHeartBeat();
        }
        break;
        case PKGID.ConnectSuccessed:
        {
            mOnConnect();
        }
        break;
        default:
        {
            mOnReceiveMessage ( pkg );
        }
        break;
        }
    }
    private void onRecv ( IAsyncResult ar )
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
                PKGResult res = PKG.parser ( mTail, bmsg );
                List<PKG> pkgList = res.mPKGList;
                mTail = res.mTail;
                foreach ( PKG pkg in pkgList )
                processPKG ( pkg );
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
    private void onLogInfo ( string info )
    {
        Console.WriteLine ( info );
    }
    void onOutOfReach()
    {
        this.mOnExpection ( "服务器尚未开启！" );
    }
}
}
