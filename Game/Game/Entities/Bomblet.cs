using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Vexillum.util;
using Vexillum.Game;

namespace  Vexillum.Entities
{
    public class Bomblet : BasicEntity
    {
        private static Texture2D bomblet;
        public Player ownerPlayer;
        static Bomblet()
        {
            bomblet = AssetManager.loadTexture("bomblet.png");
        }
        public Bomblet()
        {
            Texture = bomblet;
            Size = new Vec2(5, 5);
        }
        public override void OnCollide(Entity e, int direction)
        {
            Vec2 unitVelocity = Velocity;
            unitVelocity.Normalize();
            Level.Explode((int)(Position.X), (int)(Position.Y), 26, ownerPlayer, null);
            Level.RemoveEntity(this);
        }
        public override void OnClientCollide(Entity e, int direction)
        {
            enablePhysics = false;
        }
    }
}
