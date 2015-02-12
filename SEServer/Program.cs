using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SE
{
static class Program
{
    public delegate bool ConsoleCtrlDelegate ( int dwCtrlType );
    [DllImport ( "kernel32.dll" )]
    private static extern bool SetConsoleCtrlHandler ( ConsoleCtrlDelegate HandlerRoutine, bool Add );
    //当用户关闭Console时，系统会发送次消息
    private const int CTRL_CLOSE_EVENT = 2;
    static ServerEntity server = null;
    static void Main ( string[] args )
    {
        ConsoleCtrlDelegate newDelegate = new ConsoleCtrlDelegate ( HandlerRoutine );
        bool bRet = SetConsoleCtrlHandler ( newDelegate, true );
        server = new ServerEntity();
        try
        {
            server.restart ( 10000 );
            do
            {
                string cmd = Console.ReadLine();
                if ( cmd == "exit" )
                    break;
            }
            while ( true );
            close();
        }
        catch ( Exception ee )
        {
            MessageBox.Show ( ee.Message );
        }
    }

    static void close()
    {
        if ( server != null )
        {
            server.close();
            server = null;
        }
    }
    private static bool HandlerRoutine ( int CtrlType )
    {
        close();
        return false;
    }
}
}
