using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNOldManNet
{
    class Config
    {
        public const string SUCESS = "%^";
        public const string HART_BEAT = "!@#$";
        public  const int HART_BEAT_PERIOD = 3000;
        public  const int HART_BEAT_PERIOD_FLOAT = 1000;
        public  const int HART_BEAT_LIMIT = HART_BEAT_PERIOD + HART_BEAT_PERIOD_FLOAT;
    }
}
