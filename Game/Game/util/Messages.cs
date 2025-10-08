using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.util
{
    public static class Messages
    {
        public const int CUSTOM = 0;
        public const int PLAYER_JOIN = 1;
        public const int PLAYER_LEAVE = 2;
        public const int OURFLAG_TAKEN = 3;
        public const int THEIRFLAG_TAKEN = 4;
        public const int OURFLAG_DROPPED = 5;
        public const int THEIRFLAG_DROPPED = 6;
        public const int OURFLAG_CAPTURED = 7;
        public const int THEIRFLAG_CAPTURED = 8;
        public const int GREEN_WIN = 9;
        public const int BLUE_WIN = 10;
        public const int OURFLAG_RETURNED = 11;
        public const int THEIRFLAG_RETURNED = 12;
        public const int NOOB_INSTRUCTIONS = 13;
        public const int KILLED_BY = 14;
        public const int OURFLAG_CAPTURED_1 = 15;
        public const int YOU_KILLED = 16;
        private static string[] messages = new string[]
        {
            "$0",
            "$0"+TextUtil.COLOR_WHITE+" joined the game",
            "$0"+TextUtil.COLOR_WHITE+" left the game",
            TextUtil.COLOR_ORANGE+"The enemy has taken your flag!",
            "$0"+TextUtil.COLOR_WHITE+" has taken the enemy's flag!",
            "The enemy has dropped your flag.",
            "$0"+TextUtil.COLOR_WHITE+" has dropped the enemy's flag.",
            TextUtil.COLOR_ORANGE+"The enemy has captured your flag!",
            "$0"+TextUtil.COLOR_WHITE+" has captured the enemy's flag!",
            "The "+TextUtil.COLOR_GREEN+"green"+TextUtil.COLOR_WHITE+" team has won the game!",
            "The "+TextUtil.COLOR_BLUE+"blue"+TextUtil.COLOR_WHITE+" team has won the game!",
            "Your flag has been returned to your base.",
            "The enemy's flag has been returned to their base.",
            "Return the enemy flag to your flag to capture it!",
            "You were killed by $0",
            "You have captured the enemy's flag!",
            "You killed $0"
        };
        public static string Parse(int messageID, string[] args)
        {
            string m = messages[messageID];
            for (int i = 0; i < args.Length; i++)
            {
                m = m.Replace("$" + i, args[i]);
            }
            return m;
        }
    }
}
