using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NNOldManNet
{
public class ProcessTreeNode
{
    public struct ProcessBasicInformation
    {
        public int ExitStatus;
        public int PebBaseAddress;
        public int AffinityMask;
        public int BasePriority;
        public uint UniqueProcessId;
        public uint InheritedFromUniqueProcessId;
    }

    [DllImport("ntdll.dll")]

    public static extern int NtQueryInformationProcess(
           IntPtr hProcess,
           int processInformationClass /* 0 */,
           ref ProcessBasicInformation processBasicInformation,
           uint processInformationLength,
           out uint returnLength
         );

    public Process mData;
    public List<ProcessTreeNode> mChildren;

    public ProcessTreeNode(Process root)
    {
        mData = root;
        mChildren = new List<ProcessTreeNode>();
        addChildren(this);
    }

    public ProcessTreeNode()
    {
        mData = null;
        mChildren = new List<ProcessTreeNode>();
    }

    private static void addChildren ( ProcessTreeNode parentNode )
    {
        var processes = Process.GetProcesses();

        int pid = parentNode.mData.Id;

        foreach ( var p in processes )
        {
            var pbi = new ProcessBasicInformation();

            try
            {
                uint bytesWritten;

                if ( NtQueryInformationProcess ( p.Handle, 0, ref pbi, ( uint ) Marshal.SizeOf ( pbi ), out bytesWritten ) == 0 ) // == 0 is OK
                {
                    if ( pbi.InheritedFromUniqueProcessId == pid )
                    {
                        Process childProcess = Process.GetProcessById ( ( int ) pbi.UniqueProcessId );
                        ProcessTreeNode pnode = new ProcessTreeNode();
                        parentNode.mChildren.Add ( pnode );
                        pnode.mData = childProcess;
                        addChildren ( pnode );
                    }
                }
            }
            catch ( System.ComponentModel.Win32Exception ex )
            {
                Console.WriteLine ( ex.Message );
            }
            catch ( System.Exception ex )
            {
                Console.WriteLine ( ex.Message );
            }
        }
    }


    public void KillThis()
    {
        if ( mData != null )
        {
            try
            {
                mData.Kill();
                string sout = string.Format ( "Kill Process Name: {0}", mData.ProcessName );
                Console.WriteLine ( sout );
            }
            catch ( System.Exception ex )
            {
                Console.WriteLine ( ex.Message );
            }
        }
    }
    public void Kill()
    {
        foreach ( var n in mChildren )
        {
            n.Kill();
        }
        KillThis();
    }
}
}