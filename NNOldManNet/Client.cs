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
    // 摘要:
    //     连接返回
    //
    // 返回结果:
    //     true,连接成功；false,连接失败
    public delegate void OnConnect ( bool sucessed, bool local );
    public delegate void OnLoseConnection();
    public event OnReceiveMessage mOnReceiveMessage;

    public event DebugInfo mOnExpection;
    public event OnConnect mOnConnect;
    public event OnLoseConnection mOnLoseConnection;


    private int mHeartTime = 0;
    private System.Timers.Timer mTimer;

    public UInt32 mReceiveBufferLength = 8192;
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
    public  void TimerCallback ( object sender, ElapsedEventArgs e )
    {
        Int32 t = Environment.TickCount;
        if ( mHeartTime > 0 && Math.Abs ( t - mHeartTime ) >= Config.HART_BEAT_LIMIT )
        {
            close();
            mOnConnect ( false, true );
            return;
        }
        sendMsg ( Config.HART_BEAT );
    }
    public void onHeartBeat()
    {
        mHeartTime = Environment.TickCount;
    }
    static byte[] buffer = new byte[1024];
    public void reconnect ( string ipa, int port )
    {
        try
        {
            close();
            mNet = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            mNet.Connect(ipa, port);
            mNet.BeginReceive ( buffer, 0, buffer.Length, SocketFlags.None, onRecv, mNet );
            mTimer = new System.Timers.Timer ( Config.HART_BEAT_PERIOD );
            mTimer.Elapsed += new ElapsedEventHandler ( TimerCallback );
            mTimer.Start();
        }
        catch ( System.Exception ex )
        {
            mOnExpection ( ex.Message );
            mOnConnect(false, true);
            close();
        }
    }
    public void onRecv ( IAsyncResult ar )
    {
        try
        {
            Socket socket = ( Socket ) ar.AsyncState;

            int length = 0;
            try
            {
                length = socket.EndReceive(ar);
            }
            catch ( System.Exception ex )
            {
                mOnExpection ( ex.Message );
                mOnConnect ( false, false );
                close();
                return;
            }
            byte[] bmsg = new byte[length];
            Array.Copy ( buffer, bmsg, length );
            if ( length > 0 )
            {
                if ( bmsg.Length == Config.HART_BEAT.Length && Config.HART_BEAT == Encoding.UTF8.GetString ( bmsg ) )
                {
                    onHeartBeat();
                }
                else if (bmsg.Length == Config.SUCESS.Length && Config.SUCESS == Encoding.UTF8.GetString(bmsg))
                {
                    if (mOnConnect != null)
                        mOnConnect(true, false);
                }
                else
                {
                    mOnReceiveMessage(bmsg);
                }
            }
            else
            {

            }
            mNet.BeginReceive ( buffer, 0, buffer.Length, SocketFlags.None, onRecv, mNet );
        }
        catch ( Exception ex )
        {
            mOnExpection ( ex.Message );
        }
    }

}
}
