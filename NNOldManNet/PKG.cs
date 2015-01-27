using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
namespace NNOldManNet
{
public class PKG
{
    private static int mHeaderLength = 4;
    public PKGID mType = PKGID.None;
    private byte[] mDatas = null;
    public PKG ( PKGID id )
    {
        mType = id;
    }
    public PKG ( PKGID id, string data )
    {
        mType = id;
        mDatas = Encoding.Unicode.GetBytes ( data );
    }
    public PKG ( PKGID id, byte[] data )
    {
        mType = id;
        if ( data != null )
        {
            mDatas = new byte[data.Length];
            Array.Copy ( data, mDatas, data.Length );
        }
    }
    private PKG()
    {
    }
    public static PKG parser ( byte[] buffer )
    {
        PKG pkg = new PKG();
        pkg.mType = ( PKGID ) System.BitConverter.ToInt32 ( buffer, 0 );
        int len = buffer.Length - mHeaderLength;
        if ( len <= 0 )
            return pkg;
        pkg.mDatas = new byte[len];
        Array.Copy ( buffer, mHeaderLength, pkg.mDatas, 0, len );
        return pkg;
    }

    public string getDataString()
    {
        if (mDatas == null)
            return "";
        return Encoding.Unicode.GetString ( mDatas );
    }
    public byte[] getDataBytes()
    {
        return mDatas;
    }
    public void setData ( byte[] bmsg )
    {
        mDatas = bmsg;
    }
    public void setData ( string msg )
    {
        mDatas = Encoding.Unicode.GetBytes ( msg );
    }
    public byte[] getBuffer()
    {
        byte[] header = System.BitConverter.GetBytes ( ( int ) mType );
        Byte[] bytes = new Byte[header.Length + ( ( mDatas == null ) ? 0 : mDatas.Length )];
        Array.Copy ( header, bytes, header.Length );
        if ( mDatas != null )
            Array.Copy ( mDatas, 0, bytes, mHeaderLength, mDatas.Length );
        return bytes;
    }
}
}
