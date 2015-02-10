using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNOldManNet
{
public class Config
{
    /// <summary>
    /// 网络流编码方式
    /// </summary>
    public static Encoding Encodinger = Encoding.UTF8;
    /// <summary>
    /// 心跳周期
    /// </summary>
    public  const int HEART_BEAT_PERIOD = 30000;
    /// <summary>
    /// 心跳误差允许的最大值
    /// </summary>
    public  const int HEART_BEAT_LIMIT = HEART_BEAT_PERIOD * 2;
}
}
