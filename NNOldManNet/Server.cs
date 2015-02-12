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
};
public class Server
{
    public delegate void ReceiveMsg ( Socket client, PKG msg );
    public delegate void OnConnected ( Socket client, bool sucess );

    public event DebugInfo mOnLogInfo;
    public event ReceiveMsg mOnReceiveMsg;
    public event OnConnected mOnConnected;

    Dictionary<Socket, HeartBeatInfo> mClientPools;

    private Socket mListner;
    private int mPort = 10000;
    public int mClientLimit = 2000;
    static byte[] mBuffer = new byte[8192];
    static byte[] mTail = null;

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
    private void onTimer ( object sender, ElapsedEventArgs e )
    {
        Int32 t = Environment.TickCount;
        Dictionary<Socket, HeartBeatInfo> newPool = new Dictionary<Socket, HeartBeatInfo>();
        foreach ( KeyValuePair<Socket, HeartBeatInfo> pair in mClientPools )
        {
            Socket client = pair.Key;
            HeartBeatInfo info = pair.Value;
            if ( client.Connected )
            {
                if ( info.mHeartTime > 0 && Math.Abs ( t - info.mHeartTime ) >= Config.HEART_BEAT_LIMIT )
                {
                    mOnConnected ( client , false );
                }
                else
                {
                    sendHeartBeat ( client );
                    newPool.Add ( client, info );
                }
            }
        }
        mClientPools = newPool;
    }
    private void sendHeartBeat ( Socket client )
    {
        //sendMsg ( client, Config.HEART_BEAT );
        PKG pkg = new PKG ( PKGID.HeartBeat );
        sendMsg ( client, pkg );
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
                mListner.Bind ( new IPEndPoint ( IPAddress.Any, mPort ) );
                mListner.Listen ( mClientLimit );
                mTimer = new System.Timers.Timer ( Config.HEART_BEAT_PERIOD );
                mTimer.Elapsed += new ElapsedEventHandler ( onTimer );
                mListner.BeginAccept ( onAccept, mListner );
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
    private void processPKG ( Socket socket, PKG pkg )
    {
        switch ( ( PKGID ) pkg.mType )
        {
        case PKGID.HeartBeat:
        {
            onHeartBeat ( socket );
        }
        break;
        default:
        {
            mOnReceiveMsg ( socket, pkg );
        }
        break;
        }
    }
    private void onRecv ( IAsyncResult ar )
    {
        Socket socket = ( Socket ) ar.AsyncState;
        int length = 0;
        try
        {
            length = socket.EndReceive ( ar );
        }
        catch ( System.Exception ex )
        {
            mClientPools.Remove ( socket );
            mOnConnected ( socket, false );
            return;
        }

        if ( length > 0 )
        {
            byte[] bmsg = new byte[length];
            Array.Copy ( mBuffer, bmsg, length );
            PKGResult res = PKG.parser ( mTail, ref bmsg );
            List<PKG> pkgList = res.mPKGList;
            mTail = res.mTail;
            foreach ( PKG pkg in pkgList )
            processPKG ( socket, pkg );
        }

        try
        {
            socket.BeginReceive ( mBuffer, 0, mBuffer.Length, SocketFlags.None, onRecv, socket );
        }
        catch ( System.Exception ex )
        {
            mOnLogInfo ( ex.Message );
        }
    }
    public int countOfOnLine()
    {
        return mClientPools.Count;
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
            PKG pkg = new PKG ( PKGID.ConnectSuccessed );
            sendMsg ( client, pkg );
        }
        if ( mListner != null )
            mListner.BeginAccept ( onAccept, mListner );
    }
    private void sendMsg ( Socket client, byte[] msg )
    {
        if ( !client.Connected )
            return;
        try
        {
            client.Send ( msg );
        }
        catch ( Exception ex )
        {
            //mOnConnected(client, false);
        }
    }
    public void broadcast ( PKG pkg )
    {
        foreach ( KeyValuePair<Socket, HeartBeatInfo> p in mClientPools )
        {
            sendMsg ( p.Key, pkg.getBuffer() );
        }
    }
    private void broadcast ( string msg )
    {
        foreach ( KeyValuePair<Socket, HeartBeatInfo> p in mClientPools )
        {
            sendMsg ( p.Key, msg );
        }
    }
    public void sendMsg ( Socket client, PKG pkg )
    {
        sendMsg ( client, pkg.getBuffer() );
    }
    private void sendMsg ( Socket client, string msg )
    {
        if ( client != null )
        {
            byte[] bMsg = Config.Encodinger.GetBytes ( msg ); //消息使用UTF-8编码
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
