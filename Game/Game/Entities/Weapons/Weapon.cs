using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Vexillum.view;
using Nuclex.Input;
using Vexillum.Entities.stance;
using Microsoft.Xna.Framework.Input;
using Vexillum.net;
using Vexillum.util;

namespace  Vexillum.Entities.Weapons
{
    public abstract class Weapon
    {
        public bool hasClientEffect = false;
        public bool showHitscan = true;
        public float knockback = 0;
        public int maxHitscanLengthSquared = -1;
        private Vec2 size;
        public LivingEntity entity;
        protected Stance stance;
        public Vec2 Size {
            get{
                return size;
            }
            set{
                size = value;
                HalfSize = value / 2;
            }
        }
        public Vec2 HalfSize;

        public void SetEntity(LivingEntity e)
        {
            entity = e;
        }
        
        public Vec2 GetEnd(float angle)
        {
            return stance.GetEnd(angle);
        }

        public Vec2 GetScreenPivot(GameView view)
        {
            return stance.GetScreenPivot(view);
        }

        public Vec2 GetPivot()
        {
            return stance.GetPivot();
        }

        public abstract Stance GetStanceInstance(HumanoidEntity e);

        public void SetStance(Stance s)
        {
            stance = s;
        }

        public bool ammoUpdated = false;
        public virtual void Step(Client cl, Vec2 originPos, float angle, int gameTime)
        {

        }

        public abstract Texture2D GetImage();
        public abstract bool MouseDownServer(Level l, float angle, int button, int x, int y);
        public abstract void MouseDownClient(Client cl, Level l, float angle, int button, int x, int y);
        public abstract void MouseUpServer(Level l, float angle, int x, int y);
        public abstract void MouseUpClient(Client cl, Level l, float angle, int x, int y);
        public abstract void KeyDownServer(Level l, KeyAction key);
        public abstract void KeyDownClient(Client cl, Level l, KeyAction key);
        public abstract void MouseDragged(Level l, float angle, int button, int x, int y);
        public abstract void MouseMoved(Level l, float angle, int button, int x, int y);
        public abstract void MouseUp(Level l, float angle, int button, int x, int y);

        /*public abstract void mouseDownClient(Level l, double angle, int button, int x, int y);
        public abstract void mouseDraggedClient(Level l, double angle, int button, int x, int y);
        public abstract void mouseMovedClient(Level l, double angle, int button, int x, int y);
        public abstract void mouseUpClient(Level l, double angle, int button, int x, int y);*/
    }
}
