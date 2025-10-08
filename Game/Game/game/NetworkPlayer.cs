using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Vexillum.view;

namespace Vexillum.Game
{
    class NetworkPlayer : Player
    {
        public NetworkPlayer(string name, HumanoidEntity e) : base(name, e)
        {
        }
        public override int Score
        {
            get
            {
                return base.Score;
            }
            set
            {
                base.Score = value;
                if (Vexillum.game.View != null && Vexillum.game.View is GameView)
                    ((GameView)Vexillum.game.View).UpdateScoreboard();
            }
        }
        public override PlayerClass CurrentClass
        {
            get
            {
                return base.CurrentClass;
            }
            set
            {
                base.CurrentClass = value;
                if (Vexillum.game.View != null && Vexillum.game.View is GameView)
                    ((GameView)Vexillum.game.View).UpdateScoreboard();
            }
        }
    }
}
