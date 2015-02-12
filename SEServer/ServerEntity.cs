using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Configuration;
using NNOldManNet;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Principal;
namespace SE
{
public class ServerEntity
{
    private Server mNet;
    private List<Process> mProcessPool;
    public ServerEntity()
    {
        mProcessPool = new List<Process>();
    }
    public void close()
    {
        closeAllProcess();
        closeNet();
    }
    public void restart ( int port )
    {
        close();
        mNet = new Server ( port );
        mNet.mOnLogInfo += showInfo;
        mNet.mOnReceiveMsg += processPKG;
        mNet.mOnConnected += onConnected;
        mNet.restart();
    }
    void calCount()
    {
        string msg = string.Format ( "OnLine Count:{0}", mNet.countOfOnLine() );
        showInfo ( msg );
    }
    void onConnected ( Socket client, bool sucess )
    {
        if ( sucess )
        {
            string msg = string.Format ( "{0} Up Line!", client.RemoteEndPoint.ToString() );
            showInfo ( msg );
            broadcastWork();
        }
        else
        {
            string msg = string.Format ( "{0} Down Line!", client.RemoteEndPoint.ToString() );
            showInfo ( msg );
            broadcastWork();
        }
        calCount();
    }

    void broadcastWork()
    {
        foreach ( Process p in mProcessPool )
        {
            if ( !p.HasExited )
            {
                PKG pkg = new PKG ( PKGID.CurTaskAdd );
                pkg.setData ( p.StartInfo.FileName );
                mNet.broadcast ( pkg );
            }
        }
    }
    private void showInfo ( string msg )
    {
        Console.WriteLine ( msg );
    }
    private void processPKG ( Socket client, PKG pkg )
    {
        switch ( ( PKGID ) pkg.mType )
        {
        case PKGID.StartWork:
        {
            onStartWorkMsg ( client, pkg );
        }
        break;
        case PKGID.StopWork:
        {
            onStopWorkMsg ( client, pkg );
        }
        break;
        }
    }
    private void onStartWorkMsg ( Socket client, PKG pkg )
    {
        doWork ( client, pkg.getDataString() );
    }

    private void onStopWorkMsg ( Socket client, PKG pkg )
    {
        foreach ( Process p in mProcessPool )
        {
            if ( p.StartInfo.FileName == pkg.getDataString() )
            {
                if ( !p.HasExited )
                {
                    ProcessTreeNode root = new ProcessTreeNode ( p );
                    root.Kill();
                    return;
                }
            }
        }
    }
    public void onDataReceivedEventHandler ( object sender, DataReceivedEventArgs e )
    {
        if ( e.Data != null )
        {
            mNet.broadcast ( new PKG ( PKGID.NormalOutPut, e.Data ) );
        }
    }
    private void doWork ( Socket client, string msg )
    {
        if ( msg.Length == 0 )
            return;

        foreach ( Process p in mProcessPool )
        {
            if ( p.StartInfo.FileName == msg )
            {
                sendMsgToClient ( client, "任务正在进行！" );
                return;
            }
        }

        try
        {
            Process process = new Process();

            process.StartInfo.FileName = msg;
            process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值
            process.StartInfo.RedirectStandardOutput = true;  //重定向输出流
            process.StartInfo.RedirectStandardError = true;  //重定向错误流
            process.StartInfo.Verb = "runas";  //
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += onDataReceivedEventHandler;
            process.ErrorDataReceived += onDataReceivedEventHandler;
            process.Exited += onProcessExited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            mProcessPool.Add ( process );

            PKG pkg = new PKG ( PKGID.CurTaskAdd );
            pkg.setData ( msg );
            mNet.broadcast ( pkg );
        }
        catch ( System.Exception ex )
        {
            showInfo ( ex.Message );
        }

    }
    void sendMsgToClient ( Socket client, string msg )
    {
        if ( this.mNet.isClientOnline ( client ) )
        {
            PKG pkg = new PKG ( PKGID.NormalOutPut );
            pkg.setData ( msg );
            this.mNet.sendMsg ( client, pkg );
        }
    }
    void onProcessExited ( object sender, EventArgs e )
    {
        Process process = ( Process ) sender;
        if ( mProcessPool.Contains ( process ) )
        {
            try
            {
                //Socket client = mProcessPool[process];
                string msg = string.Format ( "{0} :Exit code ( {1} )", process.StartInfo.FileName
                                             , process.ExitCode
                                           );

                PKG pkg = new PKG ( PKGID.CurTaskDelete );
                pkg.setData ( process.StartInfo.FileName );
                mNet.broadcast ( pkg );

                mNet.broadcast ( new PKG ( PKGID.NormalOutPut, msg ) );
            }
            catch ( System.Exception ex )
            {
                showInfo ( ex.Message );
            }
            mProcessPool.Remove ( process );
        }
    }

    private void closeNet()
    {
        if ( mNet != null )
        {
            mNet.close();
            mNet = null;
        }
    }
    private void closeAllProcess()
    {
        if ( mNet != null )
        {
            foreach ( Process p in mProcessPool )
            {
                if ( !p.HasExited )
                {

                    p.OutputDataReceived -= onDataReceivedEventHandler;
                    p.ErrorDataReceived -= onDataReceivedEventHandler;
                    p.Exited -= onProcessExited;

                    PKG pkg = new PKG ( PKGID.CurTaskDelete );
                    pkg.setData ( p.StartInfo.FileName );
                    mNet.broadcast ( pkg );

                    ProcessTreeNode n = new ProcessTreeNode ( p );
                    n.Kill();
                }
            }
        }
    }
}
}
