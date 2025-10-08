using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.Game
{
    public static class GameModeCommand
    {
        public const byte GREENFLAG_CARRIER = 0;
        public const byte BLUEFLAG_CARRIER = 1;
        public const byte GREEN_SCORE = 2;
        public const byte BLUE_SCORE = 3;
        public const byte MAX_CAPTURES = 4;
        public const byte GREEN_WIN = 5;
        public const byte BLUE_WIN = 6;
        public const byte DEATH = 7;
    }
}
