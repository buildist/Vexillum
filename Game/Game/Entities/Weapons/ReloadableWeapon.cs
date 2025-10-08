using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.util;
using Vexillum.net;
using Microsoft.Xna.Framework;

namespace  Vexillum.Entities.Weapons
{
    public abstract class ReloadableWeapon : Weapon
    {
        protected bool fireNext = false;
        protected bool reloading = false;
        protected int lastReloadStepTime = 0;
        protected int lastClickTime;

        public float baseDamage;
        public float minimumDamage;
        public float minRange;
        public float maxRange;

        public int maxClipAmmo;
        public int maxAmmo;

        public int clipAmmo;
        public int totalAmmo;

        protected int fireDelay;
        protected int nextFireFrame;
        protected int reloadDelay;


        protected bool mouseDown = false;

        protected ReloadableWeapon(int totalAmmo, int clipAmmo, int fireDelay, int reloadDelay)
        {
            this.maxClipAmmo = this.clipAmmo = clipAmmo;
            this.totalAmmo = maxAmmo = totalAmmo;
            this.fireDelay = fireDelay;
            this.reloadDelay = reloadDelay;
        }
        public void SetAmmo(int totalAmmo, int clipAmmo)
        {
            if (clipAmmo != -1)
            {
                this.totalAmmo = totalAmmo;
                this.clipAmmo = clipAmmo;
            }
        }
        protected bool BeforeFire(Level l)
        {
            if (nextFireFrame > l.GetTime())
                return false;
            else if (clipAmmo == 0)
            {
                if (!Util.IsServer && l.GetTime() - lastClickTime > 500)
                {
                    l.PlaySound(null, Sounds.CLICK, entity);
                    lastClickTime = l.GetTime();
                }
                return false;
            }
            nextFireFrame = l.GetTime() + fireDelay;
            if (reloading)
            {
                reloading = false;
            }
            if (clipAmmo != -1)
            {
                clipAmmo--;
            }
            return true;
        }
        public void Reload()
        {
            if (clipAmmo != -1 && clipAmmo < maxClipAmmo && totalAmmo > 0)
            {
                reloading = true;
                nextFireFrame = entity.Level.GetTime() + 2*fireDelay;
            }
        }
        public override void Step(Client cl, Vec2 originPos, float angle, int gameTime)
        {
            if (mouseDown)
            {
                if (Util.IsServer && BeforeFire(entity.Level))
                {
                    FireServer(entity.Level, angle);
                }
                else if (!Util.IsServer && BeforeFire(entity.Level))
                {
                    FireClient(cl, entity.Level, angle);
                }
            }
            if (reloading && clipAmmo < maxClipAmmo && clipAmmo  < totalAmmo && gameTime - lastReloadStepTime > reloadDelay)
            {
                clipAmmo++;
                if (clipAmmo == maxClipAmmo || clipAmmo == totalAmmo)
                {
                    reloading = false;
                    ammoUpdated = true;
                }
                lastReloadStepTime = gameTime;
            }
        }
        public abstract void FireServer(Level l, float angle);
        public abstract void FireMenu(Level l, float angle);
        public virtual void FireClient(Client cl, Level l, float angle)
        {

        }

        public override bool MouseDownServer(Level l, float angle, int button, int x, int y)
        {
            if (button == 0)
            {
                mouseDown = true;
                if (BeforeFire(l))
                {
                    FireServer(l, angle);
                    return true;
                }
            }
            return false;
        }
        public override void MouseDownClient(Client cl, Level l, float angle, int button, int x, int y)
        {
            if (button == 0)
            {
                mouseDown = true;
                if (BeforeFire(l))
                    FireClient(cl, l, angle);
            }
        }

        public override void MouseUpClient(Client cl, Level l, float angle, int x, int y)
        {
            mouseDown = false;
        }

        public override void MouseUpServer(Level l, float angle, int x, int y)
        {
            mouseDown = false;
        }

        public override void KeyDownServer(Level l, KeyAction key)
        {
            if (key == KeyAction.Reload)
                Reload();
        }
        public override void KeyDownClient(Client cl, Level l, KeyAction key)
        {
            if (key == KeyAction.Reload)
                Reload();
        }

        public override void MouseDragged(Level l, float angle, int button, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public override void MouseMoved(Level l, float angle, int button, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public override void MouseUp(Level l, float angle, int button, int x, int y)
        {
            //throw new NotImplementedException();
        }

        public virtual float GetDamage(float distance)
        {
            distance = Math.Max(distance, 1);
            if (distance < minRange)
                return baseDamage;
            else if (distance > maxRange)
                return minimumDamage;
            else
                return minimumDamage + (baseDamage - minimumDamage)*(1 - distance/maxRange);
        }

        public override stance.Stance GetStanceInstance(HumanoidEntity e)
        {
            throw new NotImplementedException();
        }
    }
}
