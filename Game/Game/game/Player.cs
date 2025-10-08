using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.Game
{
    public class Player
    {
        private PlayerClass currentClass;
        public virtual PlayerClass CurrentClass
        {
            get
            {
                return currentClass;
            }
            set
            {
                currentClass = value;
            }
        }
        public virtual PlayerClass PlayerClass { get; set; }
        protected HumanoidEntity entity;
        public bool isBot = false;
        protected Weapon[] inventory;
        public bool canGrapple;
        public Vec2 Position
        {
            get
            {
                return entity.Position;
            }
            set
            {
                entity.Position = value;
            }
        }
        public float ArmAngle
        {
            get
            {
                return entity.ArmAngle;
            }
            set
            {
                this.entity.ArmAngle = value;
            }
        }
        public bool IsAlive()
        {
            return CurrentClass != PlayerClass.Spectator;
        }
        public void UpdateCanGrapple()
        {
            Vec2 unitVec = new Vec2((float)Math.Cos(ArmAngle), -(float)Math.Sin(ArmAngle));
            Vec2 pos = this.entity.Weapon.GetPivot();
            bool done = false;
            bool terrain = false;
            Entity entity = null;
            int px, py, x, y;
            px = (int)pos.X;
            py = (int)pos.Y;
            while (!done)
            {
                x = (int)pos.X;
                y = (int)pos.Y;
                if (this.entity.Level.terrain.GetTerrain(x, y))
                {
                    done = true;
                    terrain = true;
                }
                else
                {
                    int eID = this.entity.Level.terrain.GetEntity(this.entity, x, y, this.entity.Level);
                    if (eID != 0)
                    {
                        done = true;
                        entity = this.entity.Level.GetEntityByID(eID);
                    }
                }
                if (!done)
                    pos += unitVec;
            }
            float d2 = (pos - this.entity.Position).LengthSquared();
            if ((terrain || entity != null) && d2 < 176400 && pos.X >= 0 && pos.Y >= 0 && pos.X < this.entity.Level.Size.X && pos.Y < this.Entity.Level.Size.Y)
            {
                canGrapple = true;
            }
            else
            {
                canGrapple = false;
            }

        }
        public HumanoidEntity Entity
        {
            get
            {
                return entity;
            }
            set
            {
                entity = value;
                if (value != null)
                {
                    entity.player = this;
                    entity.SetReady(true);
                }
            }
        }
        public Weapon[] Inventory
        {
            get
            {
                return inventory;
            }
            set
            {
                inventory = value;
                SelectWeapon(0);
            }
        }
        private Weapon currentWeapon;
        public int WeaponIndex;
        public virtual void SelectWeapon(int index)
        {
            if (inventory.Length == 0)
                return;
            if (index < 0)
                index = 0;
            else if (index >= inventory.Length)
                index = inventory.Length - 1;
            currentWeapon = Inventory[index];
            if(entity != null)
                entity.Weapon = currentWeapon;
            WeaponIndex = index;
        }
        public ulong steamId;
        public string name;
        public string pingString = "0";
        private int score;
        public virtual int Score
        {
            get
            {
                return score;
            }
            set
            {
                this.score = value;
            }
        }
        public Player(string name, HumanoidEntity e)
        {
            this.name = name;
            Entity = e;
            PlayerClass = PlayerClass.Spectator;
        }
        public int GetID()
        {
            return entity.ID;
        }
    }
}
