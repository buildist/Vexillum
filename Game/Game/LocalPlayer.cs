using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Vexillum.Entities.Weapons;
using Nuclex.Input;
using Vexillum.Game;
using Vexillum.net;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;

namespace Vexillum
{
    public class LocalPlayer : Player
    {
        private Client client;
        public AudioListener Listener;
        public int hurtFrame;
        private IGameMode gameMode;
        bool left;
        bool right;
        bool jumping = false;

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

        public LocalPlayer(string name, HumanoidEntity e) : base(name, e)
        {
            Entity = e;
            Listener = new AudioListener();
        }

        public void SetClient(Client c)
        {
            client = c;
        }
        public Client GetClient()
        {
            return client;
        }

        public void MouseDown(MouseButtons button, int x, int y)
        {
            if (IsAlive() && entity.Weapon != null && ((HumanoidEntity)entity).hook == null)
            {
                client.SendWeaponActivate(Util.GetMouseButtonInt(button), 1);
                entity.Weapon.MouseDownClient(client, entity.Level, ArmAngle, Util.GetMouseButtonInt(button), x, y);
            }
        }

        public void MouseUp(MouseButtons button, int x, int y)
        {
            if (IsAlive() && entity.Weapon != null)
            {
                client.SendWeaponActivate(Util.GetMouseButtonInt(button), 0);
                entity.Weapon.MouseUpClient(client, entity.Level, ArmAngle, x, y);
            }
        }

        public void MouseMove(GameView view, int x, int y)
        {
            Vec2 weaponPos = this.entity.GetWeaponScreenPivot(view);
            int cx = (int)weaponPos.X;
            int cy = (int)weaponPos.Y;
            ArmAngle = (float) Math.Atan2(y - cy, x - cx);
            UpdateCanGrapple();
        }

        public void KeyPressed(Keys key, bool isNew)
        {
            KeyAction action = ControlSystem.GetAction(key);
            if (entity.hook == null)
            {
                switch (action)
                {
                    case KeyAction.Move_Left:
                        entity.xVelocity = -entity.Speed;
                        left = true;
                        if (isNew)
                        {
                            entity.moving = true;
                            entity.direction = false;
                            entity.movementChanged = true;
                        }
                        break;
                    case KeyAction.Move_Right:
                        entity.xVelocity = entity.Speed;
                        right = true;
                        if (isNew)
                        {
                            entity.moving = true;
                            entity.direction = true;
                            entity.movementChanged = true;
                        }
                        break;
                    case KeyAction.Move_Down:
                        if(entity.ladder)
                            entity.ladderDirection = -1;
                        break;
                    case KeyAction.Jump:
                        if (entity.ladder && entity.Level.terrain.GetLadder((int)entity.Position.X, (int)(entity.Position.Y - entity.HalfSize.Y + 6)))
                            entity.ladderDirection = 1;
                        else if(!entity.jumping)
                        {
                            entity.Jump();
                            jumping = true;
                            if (isNew)
                                entity.movementChanged = true;
                        }
                        break;
                }
            }
            if (action == KeyAction.GrapplingHook || (action == KeyAction.Jump && entity.hook != null))
            {
                if (entity.hook != null)
                {
                    entity.SetGrapplingHook(null);
                    entity.Velocity *= 0.5f;
                    client.SendWeaponActivate(254, 0);
                }
                else if(canGrapple)
                {
                    client.SendWeaponActivate(255, 0);
                }
            }
            else if (isNew && entity.Weapon != null && action != KeyAction.None)
            {
                client.SendWeaponAction(WeaponIndex, action);
                entity.Weapon.KeyDownClient(client, entity.Level, action);
            }
            else if (isNew)
            {
                string keyStr = key.ToString();
                if (keyStr.Length == 2 && keyStr.StartsWith("D"))
                {
                    int idx;
                    try
                    {
                        idx = int.Parse(keyStr.Substring(1));
                    }
                    catch (Exception ex)
                    {
                        idx = -1;
                    }
                    idx--;
                    if (idx >= 0 && idx < inventory.Length)
                    {
                        SelectWeapon(idx);
                        client.SendWeaponSelect(idx);
                        entity.Level.PlaySound(null, Sounds.CLICK2, entity);
                    }
                }
            }
        }

        public void KeyReleased(Keys key)
        {
            KeyAction action = ControlSystem.GetAction(key);
            switch (action)
            {
                case KeyAction.Move_Left:
                    left = false;
                    if (entity.xVelocity < 0)
                    {
                        if (right)
                        {
                            entity.xVelocity = entity.Speed;
                        }
                        else
                        {
                            entity.xVelocity = 0;
                            entity.moving = false;
                            entity.movementChanged = true;
                        }
                    }
                    break;
                case KeyAction.Move_Right:
                    right = false;
                    if (entity.xVelocity > 0)
                    {
                        if (left)
                        {
                            entity.xVelocity = -entity.Speed;
                        }
                        else
                        {
                            entity.xVelocity = 0;
                            entity.moving = false;
                            entity.movementChanged = true;
                        }
                    }
                    break;
                case KeyAction.Jump:
                    if (entity.ladderDirection == 1)
                    {
                        entity.ladderDirection = 0;
                        entity.FixedVelocity.Y = 0;
                        entity.jumping = false;
                    }
                    break;
                case KeyAction.Move_Down:
                    if (entity.ladderDirection == -1)
                    {
                        entity.ladderDirection = 0;
                        entity.FixedVelocity.Y = 0;
                        entity.jumping = false;
                    }
                    break;
            }
        }
        public void Step(int gameTime)
        {
            if (jumping && !entity.jumping)
            {
                jumping = false;
                entity.movementChanged = true;
            }
            if(entity.Weapon != null)
                entity.Weapon.Step(client, entity.Weapon.GetPivot(), ArmAngle, gameTime);
            Listener.Position = new Vector3(entity.Position.X, entity.Position.Y, 0);
            Listener.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);
        }
        public bool SetGameMode(IGameMode gameMode)
        {
            try
            {
                this.gameMode = gameMode;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public IGameMode GetGameMode()
        {
            return gameMode;
        }
        public void SendChat(string msg)
        {
            client.SendChat(msg);
        } 
        public void Draw(GameView view, SpriteBatch spriteBatch, bool showUI)
        {
            entity.DrawCrosshair(view, spriteBatch, canGrapple);
            if(showUI)
                gameMode.DrawUI(view, spriteBatch, this);
        }
    }
}
