using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
namespace NNOldManNet
{
public delegate void DebugInfo ( string msg );
public class HeartBeatInfo
{
    public int mHeartTime = 0;
    public TcpClient mClient = null;
    public bool mInvalid = false;
};
public class Server
{
    public delegate void ReceiveMsg ( Socket client, byte[] msg );
    public delegate void OnConnected ( Socket client, bool sucess );

    public event DebugInfo mOnLogInfo;
    public event ReceiveMsg mOnReceiveMsg;
    public event OnConnected mOnConnected;

    Dictionary<Socket, HeartBeatInfo> mClientPools;

    public Socket mListner;
    private string mIP = "localhost";
    private int mPort = 10000;
    static byte[] mBuffer = new byte[8192];

    private System.Timers.Timer mTimer;

    public Server (  int port )
    {
        mPort = port;
    }

    public Socket getClientByIpAddress ( string ipa )
    {
        foreach ( KeyValuePair<Socket, HeartBeatInfo> pair in mClientPools )
        {
            Socket client = pair.Key;
            if ( ipa == client.RemoteEndPoint.ToString() )
                return client;
        }
        return null;
    }
    public bool isClientOnline ( Socket client )
    {
        if ( client == null )
            return false;
        return mClientPools.ContainsKey ( client );
    }
    public  void onTimer ( object sender, ElapsedEventArgs e )
    {
        Int32 t = Environment.TickCount;
        Dictionary<Socket, HeartBeatInfo> newPool = new Dictionary<Socket, HeartBeatInfo>();
        foreach ( KeyValuePair<Socket, HeartBeatInfo> pair in mClientPools )
        {
            Socket client = pair.Key;
            HeartBeatInfo info = pair.Value;
            if ( client.Connected )
            {
                if ( info.mHeartTime > 0 && Math.Abs ( t - info.mHeartTime ) >= Config.HART_BEAT_LIMIT )
                {
                    info.mInvalid = true;
                    mOnConnected ( client , false );
                }
                else
                {
                    sendMsg ( client, Config.HART_BEAT );
                    newPool.Add ( client, info );
                }
            }
            else
            {
                info.mInvalid = true;
            }
        }
        mClientPools = newPool;
    }
    public void close()
    {
        if ( mListner != null )
        {
            mListner.Close();
            mListner.Dispose();
            mListner = null;
        }
    }
    public bool restart()
    {
        try
        {
            if ( mListner == null )
            {
                mClientPools = new Dictionary<Socket, HeartBeatInfo>();
                mListner = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                //mListner.Bind(new IPEndPoint(IPAddress.Parse(mIP), mPort));
                mListner.Bind(new IPEndPoint(IPAddress.Any, mPort));
                mListner.Listen ( 2000 );
                mListner.BeginAccept ( onAccept, mListner );
                mTimer = new System.Timers.Timer ( Config.HART_BEAT_PERIOD );
                mTimer.Elapsed += new ElapsedEventHandler ( onTimer );
                mTimer.Start();
                mOnLogInfo ( "Start Success!" );
            }
        }
        catch ( System.Exception ex )
        {
            close();
            mOnLogInfo ( ex.Message );
            return false;
        }
        return true;
    }
    public void onRecv ( IAsyncResult ar )
    {
        Socket socket = ( Socket ) ar.AsyncState;
        int length = 0;
        try
        {
            length = socket.EndReceive ( ar );
            if ( length > 0 )
            {
                byte[] bmsg = new byte[length];
                Array.Copy ( mBuffer, bmsg, length );
                if ( bmsg.Length == Config.HART_BEAT.Length && Config.HART_BEAT == Encoding.UTF8.GetString ( bmsg ) )
                {
                    onHeartBeat ( socket );
                }
                else
                {
                    mOnReceiveMsg ( socket, bmsg );
                }
            }
            socket.BeginReceive ( mBuffer, 0, mBuffer.Length, SocketFlags.None, onRecv, socket );

        }
        catch ( System.Exception  )
        {
            mOnConnected ( socket, false );
            mClientPools.Remove ( socket );
        }
    }

    void onAccept ( IAsyncResult ar )
    {
        if ( mListner == null )
            return;

        Socket client = null;
        try
        {
            client = mListner.EndAccept ( ar );
        }
        catch ( System.Exception ex )
        {
            mOnLogInfo ( ex.Message );
        }

        if ( client != null )
        {
            if ( !mClientPools.ContainsKey ( client ) )
            {
                mClientPools[client] = new HeartBeatInfo();
                if ( client.Connected )
                {
                    mOnConnected ( client, true );
                }
            }
            client.BeginReceive ( mBuffer, 0, mBuffer.Length, SocketFlags.None, onRecv, client );
            sendMsg ( client, Encoding.UTF8.GetBytes ( Config.SUCESS ) );
        }
        if ( mListner != null )
            mListner.BeginAccept ( onAccept, mListner );
    }
    public void sendMsg ( Socket client, byte[] msg )
    {
        try
        {
            client.Send ( msg );
        }
        catch ( Exception ex )
        {
            mOnConnected ( client, false );
        }
    }
    public void sendMsg ( Socket client, string msg )
    {
        if ( client != null )
        {
            byte[] bMsg = System.Text.Encoding.UTF8.GetBytes ( msg ); //消息使用UTF-8编码
            sendMsg ( client, bMsg );
        }
    }

    private void onHeartBeat ( Socket client )
    {
        HeartBeatInfo info = mClientPools[client];
        info.mHeartTime = Environment.TickCount;
    }

}
}
