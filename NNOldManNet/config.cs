using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNOldManNet
{
    class Config
    {
        public static Encoding Encodinger = Encoding.UTF8;
        public const string SUCESS = "%^";
        public const string HEART_BEAT = "!@#$";
        public  const int HEART_BEAT_PERIOD = 2000000;
        public const int HEART_BEAT_PERIOD_FLOAT = 1000000;
        public  const int HEART_BEAT_LIMIT = HEART_BEAT_PERIOD + HEART_BEAT_PERIOD_FLOAT;
    }
}
