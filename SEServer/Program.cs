using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SE
{
static class Program
{
    static void Main ( string[] args )
    {
        ServerEntity server = new ServerEntity();
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
            server.close();
        }
        catch ( Exception ee )
        {
            MessageBox.Show ( ee.Message );
        }
    }
}
}
