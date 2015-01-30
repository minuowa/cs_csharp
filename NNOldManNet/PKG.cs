using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
namespace NNOldManNet
{
public class PKGResult
{
    public List<PKG> mPKGList = new List<PKG>();
    public byte[] mTail = null;
}
public class PKG
{
    //len+pkgid
    private static int mHeaderLength = 8;
    public int mType = (int)PKGID.None;
    private int mLength = 8;
    private byte[] mDatas = null;
    public PKG ( PKGID id )
    {
        mType = (int)id;
    }
    public PKG ( PKGID id, string data )
    {
        mType = (int)id;
        mDatas = Config.Encodinger.GetBytes ( data );
        calLen();
    }
    public PKG ( PKGID id, byte[] data )
    {
        mType = (int)id;
        if (data != null)
        {
            mDatas = new byte[data.Length];
            Array.Copy ( data, mDatas, data.Length );
            calLen();
        }
    }
    private PKG()
    {
    }
    //解决粘包问题
    public static PKGResult parser ( byte[] tail, byte[] bufferRaw )
    {
        byte[] buffer = null;
        if ( tail == null )
        {
            buffer = bufferRaw;
        }
        else
        {
            buffer = new byte[tail.Length + bufferRaw.Length];
            Array.Copy ( tail, buffer, tail.Length );
            Array.Copy ( bufferRaw, 0, buffer, tail.Length, bufferRaw.Length );
        }
        PKGResult res = new PKGResult();
        int startIndex = 0;
        int len = buffer.Length;
        while ( len > 0 )
        {
            PKG pkg = new PKG();
            pkg.mType =  System.BitConverter.ToInt32 ( buffer, startIndex );
            pkg.mLength = System.BitConverter.ToInt32 ( buffer, startIndex + 4 );
            if ( len < pkg.mLength )
            {
                res.mTail = new byte[len];
                Array.Copy ( buffer, startIndex, res.mTail, 0, len );
                return res;
            }
            int dataLen = pkg.mLength - mHeaderLength;
            if ( dataLen > 0 )
            {
                pkg.mDatas = new byte[dataLen];
                Array.Copy ( buffer, startIndex + mHeaderLength, pkg.mDatas, 0, dataLen );
            }
            res.mPKGList.Add ( pkg );
            len -= pkg.mLength;
            startIndex += pkg.mLength;
        }
        return res;
    }

    public string getDataString()
    {
        if ( mDatas == null )
            return string.Empty;
        return Config.Encodinger.GetString ( mDatas );
    }
    public byte[] getDataBytes()
    {
        return mDatas;
    }
    private void calLen()
    {
        mLength = mHeaderLength + mDatas.Length;
    }
    public void setData ( byte[] bmsg )
    {
        mDatas = bmsg;
        calLen();
    }
    public void setData ( string msg )
    {
        mDatas = Config.Encodinger.GetBytes ( msg );
        calLen();
    }
    public byte[] getBuffer()
    {
        byte[] header = System.BitConverter.GetBytes ( ( int ) mType );
        byte[] bytes = new byte[mHeaderLength + ( ( mDatas == null ) ? 0 : mDatas.Length )];
        Array.Copy ( header, bytes, header.Length );
        Array.Copy ( System.BitConverter.GetBytes ( mLength ), 0, bytes, 4, 4 );
        if ( mDatas != null )
            Array.Copy ( mDatas, 0, bytes, mHeaderLength, mDatas.Length );
        return bytes;
    }
}
}
