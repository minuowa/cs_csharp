using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace StriveEngine.SimpleDemoServer
{
    public class ReceiveEvent : EventArgs
    {
        public EndPoint mIP;
        public byte[] mMsg;
    }
    public delegate void MessageRecive(TcpClient ip, byte[] msg);
    public class Server
    {
        public  event MessageRecive MessageReciveEvent;
        TcpListener mListener;
        public void close()
        {
            mListener.Stop();
        }
        public void Initialize(string puerto)
        {
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipa = getIPv4(ipe);
            mListener = new TcpListener(ipa, int.Parse(puerto));
            mListener.Start();
            mListener.BeginAcceptTcpClient(AcceptCallback, mListener);
            //clientConnected.WaitOne();
        }
        public static IPAddress getIPv4(IPHostEntry e)
        {
            foreach(IPAddress r in e.AddressList)
            {
                if (r.AddressFamily == AddressFamily.InterNetwork)
                    return r;
            }
            return null;
        }
        public void SendMessageToClient(TcpClient client,byte[] bmsg)
        {
            if (!client.Connected)
            {
                return;
            }
            NetworkStream netStream = client.GetStream();
            netStream.Write(bmsg, 0, bmsg.Length);
        }
        private  void AcceptCallback(IAsyncResult ar)
        {
            TcpListener server = (TcpListener)ar.AsyncState;
            ClientState state = new ClientState();
            try
            {
                state.client = server.EndAcceptTcpClient(ar);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            EndPoint ipadd = state.client.Client.RemoteEndPoint;
            // We're going to start reading from the client's stream, and
            // we need a buffer for that:

            state.buffer = new byte[4096];

            // Note that the TcpClient and the byte[] are both put into
            // this "ClientState" object.  We're going to need an easy
            // way to get at those values in the callback for the read
            // operation.

            // Next, start a new accept operation so that we can process
            // another client connection:

            server.BeginAcceptTcpClient(AcceptCallback, server);

            // Finally, start a read operation on the client we just
            // accepted.  Note that you could do this before starting the
            // accept operation; the order isn't really important.

            state.client.GetStream().BeginRead(state.buffer, 0, state.buffer.Length, ReadCallback, state);
        }
        public void log(string msg)
        {

        }
        private void ReadCallback(IAsyncResult ar)
        {
            ClientState state = (ClientState)ar.AsyncState;
            TcpClient client=state.client;
            if (!client.Connected)
            {
                log("下线了");
                return;
            }
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
            string strData = Encoding.UTF8.GetString(state.buffer, 0, cbRead);

            byte[] smgs=new byte[cbRead];
            Array.Copy(state.buffer,smgs,cbRead);
            MessageReciveEvent(state.client, smgs);
            // Your data is in state.buffer, and there are cbRead
            // bytes to process in the buffer.  This number may be
            // anywhere from 1 up to the length of the buffer.
            // The i/o completes when there is _any_ data to be read,
            // not necessarily when the buffer is full.

            // So, for example:

            Console.WriteLine(strData);

            // For ASCII you won't have to worry about partial characters
            // but for pretty much any other common encoding you'll have to
            // deal with that possibility, as there's no guarantee that an
            // entire character will be transmitted in one piece.

            // Of course, even with ASCII, you need to watch your string
            // terminations.  You'll have to either check the read buffer
            // directly for a null terminator, or have some other means
            // of detecting the actual end of a string.  By the time the
            // string goes through the decoding process, you'll have lost
            // that information.

            // As with the accept operation, we need to start a new read
            // operation on this client, so that we can process the next
            // bit of data that's sent:

            state.client.GetStream().BeginRead(state.buffer, 0, state.buffer.Length, ReadCallback, state);
        }
        public bool IsClientOnline(TcpClient client)
        {
            return true;
        }
    }
}
