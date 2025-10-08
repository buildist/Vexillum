using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;
using Vexillum.Entities.stance;

namespace  Vexillum.Entities
{
    public abstract class LivingEntity : Entity
    {
        private Weapon weapon;
        public virtual Weapon Weapon
        {
            get
            {
                return weapon;
            }
            set
            {
                weapon = value;
                weapon.SetEntity(this);
            }
        }

        private float armAngle;
        public float ArmAngle
        {
            get
            {
                return armAngle;
            }
            set
            {
                armAngle = value;
            }
        }

        private static RandomSoundPlayer soundPlayer;

        private Random random = new Random();
        private int lastRandom;
        private int lastStepTime;
        public bool movementChanged;
        public bool direction = true;
        public bool moving = false;
        public float xVelocity;

        static LivingEntity()
        {
            soundPlayer = new RandomSoundPlayer(new Sounds[] { Sounds.WALK1, Sounds.WALK2, Sounds.WALK3, Sounds.WALK4, Sounds.WALK5 });
        }
        public override void OnClientCollide(Entity e, int direction)
        {
            if (!(this.player is LocalPlayer) && direction == 1 && Velocity.Y <= 0 && Level.GetTime() - lastStepTime > 500)
            {
                PlayWalkSound();
            }
        }
        public void PlayWalkSound()
        {
            lastStepTime = (int) Level.GetTime();
            soundPlayer.PlaySound(Level, this);
        }
        public void Jump()
        {
            velocity.Y = 6;
            jumping = true;
        }
        public void SetMovement()
        {
            if (moving)
            {
                if (direction)
                    xVelocity = Speed;
                else
                    xVelocity = -Speed;
            }
            else
                xVelocity = 0;
        }
        public override void Step(int time)
        {
            base.Step(time);
            if (xVelocity != 0)
                velocity.X = xVelocity;
            velocity += Force;
            Force = Vec2.Zero;
        }
        public Vec2 GetWeaponScreenPivot(GameView view)
        {
            if (weapon == null)
                return Vec2.Zero;
            return weapon.GetScreenPivot(view);
        }
        public int GetTotalAmmo()
        {
            if (weapon is ReloadableWeapon)
                return ((ReloadableWeapon)weapon).totalAmmo;
            else
                return 0;
        }
        public int GetMaxAmmo()
        {
            if (weapon is ReloadableWeapon)
                return ((ReloadableWeapon)weapon).maxAmmo;
            else
                return 0;
        }
        public int GetClipAmmo()
        {
            if (weapon is ReloadableWeapon)
                return ((ReloadableWeapon)weapon).clipAmmo;
            else
                return 0;
        }
        public int GetMaxClipAmmo()
        {
            if (weapon is ReloadableWeapon)
                return ((ReloadableWeapon)weapon).maxClipAmmo;
            else
                return 0;
        }
    }
}
