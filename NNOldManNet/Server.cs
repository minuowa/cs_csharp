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

    public event DebugInfo mOnDebugInfo;
    public event DebugInfo mOnErrorInfo;
    public event ReceiveMsg mOnReceiveMsg;
    public event OnConnected mOnConnected;

    Dictionary<Socket, HeartBeatInfo> mClientPools;

    public Socket mListner;
    public string mIP = "127.0.0.1";
    public int mPort = 10000;

    private System.Timers.Timer mTimer;

    public List<TcpClient> getClientList()
    {
        return null;
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
    public bool IsClientOnline ( Socket client )
    {
        return true;
    }
    public  void TimerCallback ( object sender, ElapsedEventArgs e )
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
    static byte[] buffer = new byte[1024];
    public void restart()
    {
        try
        {
            if ( mListner == null )
            {
                mClientPools = new Dictionary<Socket, HeartBeatInfo>();
                mListner = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                mListner.Bind ( new IPEndPoint ( IPAddress.Parse ( mIP ), mPort ) );
                mListner.Listen ( 2000 );
                mListner.BeginAccept ( onAccept, mListner );
                //mAcceptThread = new Thread ( listenThreadCallBack );
                //mAcceptThread.Start();
                mTimer = new System.Timers.Timer ( Config.HART_BEAT_PERIOD );
                mTimer.Elapsed += new ElapsedEventHandler ( TimerCallback );
                mTimer.Start();
                mOnDebugInfo ( "开始监听" );
            }
        }
        catch ( System.Exception ex )
        {
            close();
            mOnDebugInfo ( ex.Message );
        }

    }
    public void listenThreadCallBack()
    {
        while ( true )
        {
        }
    }
    public void recvThreadCallBack()
    {

    }
    public void onRecv ( IAsyncResult ar )
    {
        Socket socket = ( Socket ) ar.AsyncState;
        int length = 0;
        try
        {
            length=socket.EndReceive(ar);
            if ( length > 0 )
            {
                byte[] bmsg = new byte[length];
                Array.Copy(buffer, bmsg, length);
                if ( bmsg.Length == Config.HART_BEAT.Length && Config.HART_BEAT == Encoding.UTF8.GetString ( bmsg ) )
                {
                    onHeartBeat ( socket );
                }
                else
                {
                    mOnReceiveMsg(socket, bmsg);
                }
            }
            socket.BeginReceive ( buffer, 0, buffer.Length, SocketFlags.None, onRecv, socket );

        }
        catch ( System.Exception  )
        {
            mOnConnected ( socket, false );
            mClientPools.Remove ( socket );
        }
    }

    void onAccept ( IAsyncResult ar )
    {
        Socket client = null;
        try
        {
            client = mListner.EndAccept ( ar );
        }
        catch ( System.Exception ex )
        {
            mOnErrorInfo ( ex.Message );
        }
        if ( !mClientPools.ContainsKey ( client ) )
        {
            mClientPools[client] = new HeartBeatInfo();
            if ( client.Connected )
            {
                mOnConnected ( client, true );
            }
        }
        client.BeginReceive ( buffer, 0, buffer.Length, SocketFlags.None, onRecv, client );
        byte[] bmsg = Encoding.UTF8.GetBytes ( Config.SUCESS );
        sendMsg ( client, bmsg );
        mListner.BeginAccept ( onAccept, mListner );
    }
    public void sendMsg ( Socket client, byte[] msg )
    {
        client.Send ( msg );
    }
    public void sendMsg ( Socket client, string msg )
    {
        if ( client != null )
        {
            byte[] bMsg = System.Text.Encoding.UTF8.GetBytes ( msg ); //消息使用UTF-8编码
            sendMsg ( client, bMsg );
        }
    }
    public  void ReceiveMessage ( IAsyncResult ar )
    {
        //Socket client = (Socket)ar.AsyncState;

        //string receivemessage = reader.ReadString();
        //reader.Close();
        //if (receivemessage == Config.HART_BEAT)
        //{
        //    onHeartBeat(client);
        //    return;
        //}
        //mOnReceiveMsg(client, receivemessage);
    }
    private void readFromClient ( Socket client, int len )
    {
        byte[] buffer = new byte[len];
        IAsyncResult receiveAr = client.BeginReceive ( buffer, 0, buffer.Length, SocketFlags.None, ReceiveMessage, client );
        mOnReceiveMsg ( client,  buffer  );
        client.EndReceive ( receiveAr );
    }
    public void onClient ( IAsyncResult ar )
    {
        TcpListener listner = ( TcpListener ) ar.AsyncState;
        Socket tcpClient = listner.Server;
        if ( tcpClient != null )
        {
            mOnDebugInfo ( "接受到连接" );


            if ( tcpClient.Available == 0 )
            {
                if ( mOnConnected != null )
                    mOnConnected ( tcpClient, true );
            }
            else
            {
                readFromClient ( tcpClient, tcpClient.Available );
            }
        }
        listner.EndAcceptTcpClient ( ar );
    }


    private void onHeartBeat ( Socket client )
    {
        HeartBeatInfo info = mClientPools[client];
        info.mHeartTime = Environment.TickCount;
    }

}
}
