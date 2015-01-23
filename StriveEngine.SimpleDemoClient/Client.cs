using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace StriveEngine.SimpleDemoClient
{
    public class Client
    {
        // Thread signal.
        public delegate void MessageRecive(TcpClient ip, byte[] msg);
        public event MessageRecive MessageReciveEvent;
        public static ManualResetEvent clientConnected = new ManualResetEvent(false);
        public TcpClient mNet;
        public Client()
        {
            mNet = new TcpClient();
        }
        public void initilize(string ip, int port)
        {
            mNet.BeginConnect(ip, port, onMessage, mNet);
        }
        public void sendMsg(string msg)
        {
            if (!mNet.Connected)
            {
                return;
            }
            NetworkStream netStream=mNet.GetStream();
            string content = msg + "\0";// "\0" 表示一个消息的结尾
            byte[] bMsg = System.Text.Encoding.UTF8.GetBytes ( content ); //消息使用UTF-8编码
            netStream.Write(bMsg, 0, bMsg.Length);
        }
        public void onMessage(IAsyncResult ar)
        {
            TcpClient t = (TcpClient)ar.AsyncState;
            if (t == null)
            {
                MessageBox.Show("链接失败");
                return;
            }
            else if (t.Connected)
            {
                MessageBox.Show("链接成功");
                t.EndConnect(ar);
            }
            ClientState state = new ClientState();
            state.client = t;
            state.buffer = new byte[4096];
            t.GetStream().BeginRead(state.buffer, 0, state.buffer.Length, ReadCallback, state);
        }


        private  void ReadCallback(IAsyncResult ar)
        {
            ClientState state = (ClientState)ar.AsyncState;
            int cbRead = state.client.GetStream().EndRead(ar);

            if (cbRead == 0)
            {
                // The client has closed the connection
                return;
            }
            //else
            //{
            //Console.WriteLine("hay datos");
            //}

            // Your data is in state.buffer, and there are cbRead
            // bytes to process in the buffer.  This number may be
            // anywhere from 1 up to the length of the buffer.
            // The i/o completes when there is _any_ data to be read,
            // not necessarily when the buffer is full.

            // So, for example:

            string strData = Encoding.ASCII.GetString(state.buffer, 0, cbRead);

            byte[] smgs = new byte[cbRead];
            Array.Copy(state.buffer, smgs, cbRead);
            MessageReciveEvent(state.client, smgs);
        }
    }
}
