using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Game;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public class GreenFlagEntity : BasicEntity
    {
        public GreenFlagEntity()
        {
            Texture = SurvivalGameModeShared.ctfTexture;
            Size = new Vec2(22, 30);
            Rectangle = SurvivalGameModeShared.greenFlag;
            anchored = true;
        }
        public override void OnCollide(Entity e, int direction)
        {
            if (e is HumanoidEntity)
            {
                Level.OnFlagCollide(e.player, this);
            }
        }
    }
}
