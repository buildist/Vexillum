using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.util
{
    public static class VexillumConstants
    {
        public const int MAX_PING = 5000;
        public const int FRAME_RATE = 60;
        public const int TIME_PER_FRAME = (int) (1 / (float)FRAME_RATE * 1000);
        public const int PROTOCOL_VERSION = 3;
        public const int DEFAULT_PORT = 24224;
    }
}
