using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.view;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public class BasicEntity : Entity
    {
        private Texture2D texture;
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }
        public Rectangle? Rectangle = null;
        public override void Draw(GameView view, SpriteBatch spriteBatch)
        {
            screenPos = new Vec2((int)(GetDrawPosition().X - view.CamStart.X), (int)(view.CamStart.Y - GetDrawPosition().Y));
            spriteBatch.Draw(Texture, screenPos.XNAVec, Rectangle, Color.White, Rotation, HalfSize.XNAVec, 1.0f, SpriteEffects.None, 0);
        }
    }
}
