using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.UserInterface;
using Vexillum.util;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using Vexillum.ui;
using Vexillum.Game;
using Microsoft.Xna.Framework.Content;

namespace Vexillum.view
{
    public class GameView : AbstractView
    {
        public TaskQueue tasks;
        private FrameTaskQueue frameTasks;

        private Texture2D border = AssetManager.loadTexture("border.png");
        private Texture2D border_red = AssetManager.loadTexture("border_red.png"); 
        private Texture2D paused = AssetManager.loadTexture("border_dark.png");
        public Vec2 CamPosition;
        public Vec2 CamTarget;
        public Vec2 CamStart { get; set; }
        protected int xCenter;
        protected int yCenter;
        public ClientLevel Level;
        protected LocalPlayer player;

        protected Vec2 chatPos;
        protected bool showChatBar = false;
        protected string chat = "";
        protected int chatLineHeight;
        private List<ChatMessage> chats = new List<ChatMessage>(64);

        private const int messageY = 100;

        private string message = null;
        private Vec2 messagePos;
        private Rectangle messageBackgroundRectangle;
        private int messageStartTime;
        private int messageDuration;
        private float messageAlpha = 0;
        private bool fadeMessage = false;
        private float messageAlphaStep = 0.05f;
        private Scoreboard scoreboard;
        private bool showScoreboard = false;

        private static Effect blurEffect;
        private static EffectParameter blurD;
        
        private RenderTarget2D shaderTarget;
        private Texture2D shaderTexture;

        public static void LoadShaders(ContentManager Content)
        {
            blurEffect = Content.Load<Effect>("Blur");
            blurD = blurEffect.Parameters["d"];
        }

        private static RenderTarget2D CloneRenderTarget(GraphicsDevice device)
        {
            return new RenderTarget2D(device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight
            );
        }

        public GameView(GraphicsDevice g, int width, int height, ClientLevel level)
            : base(width, height)
        {
            shaderTarget = CloneRenderTarget(g);
            shaderTexture = new Texture2D(g, width, height, false, shaderTarget.Format);
            
            tasks = new TaskQueue();
            xCenter = width / 2;
            yCenter = height / 2;
            SetLevel(level);
            chatPos = new Vec2(5, height - 16);
            chatLineHeight = TextRenderer.GetLineHeight(TextRenderer.DefaultFont);
            scoreboard = new Scoreboard();
        }
        public void SetPlayers(List<Player> p)
        {
            scoreboard.SetPlayers(p);
        }

        public void SetLevel(ClientLevel l)
        {
            Level = l;
            frameTasks = new FrameTaskQueue(tasks, l);
        }

        public void SetLocalPlayer(LocalPlayer p)
        {
            player = p;
            Level.SetLocalPlayer(p);
        }

        public void Pause()
        {
            if(Menu == null)
                Menu = new PauseMenu(this);
        }

        public void Unpause()
        {
            Menu = null;
        }

        public bool IsPaused()
        {
            return Menu != null;
        }

        public override void KeyPressed(Keys key, bool isNew)
        {
            KeyAction action = ControlSystem.GetAction(key);
            switch (action)
            {
                case KeyAction.Pause:
                    if (!showChatBar)
                    {
                        if (IsPaused())
                            Unpause();
                        else
                            Pause();
                    }
                    break;
                case KeyAction.SendChat:
                    if(showChatBar)
                    {
                        if(chat != "")
                            player.SendChat(chat);
                        showChatBar = false;
                    }
                    break;
                case KeyAction.Chat:
                    if (!showChatBar)
                    {
                        AddTask(delegate()
                        {
                            showChatBar = true;
                            chat = "";
                        });
                    }
                    break;
                case KeyAction.Show_Scoreboard:
                    showScoreboard = true;
                    break;
                default:
                    if(!showChatBar)
                        player.KeyPressed(key, isNew);
                    break;
            }
            if (showChatBar)
            {
                if (key == Keys.Escape)
                {
                    showChatBar = false;
                }
                else if (key == Keys.Back && chat.Length > 0)
                {
                    chat = chat.Substring(0, chat.Length - 1);
                }
            }
        }
        public override void CharacterEntered(char character)
        {
            if (showChatBar && !Char.IsControl(character))
            {
                chat += character;
            }
        }

        public override void KeyReleased(Keys key)
        {
            KeyAction action = ControlSystem.GetAction(key);
            switch (action)
            {
                case KeyAction.Show_Scoreboard:
                    showScoreboard = false;
                    break;
                default:
                    player.KeyReleased(key);
                    break;
            }
        }

        public void ResetCamera()
        {
            CamPosition = CamTarget;
        }

        public override void DrawStuff(Microsoft.Xna.Framework.Graphics.GraphicsDevice g, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            showScoreboard = showScoreboard || player.GetGameMode().gameOver;
            if (player != null)
            {
                CamTarget = player.Position;
            }
            Vec2 d = CamTarget - CamPosition;
            CamPosition += d * (float) Math.Pow(0.9, VexillumConstants.TIME_PER_FRAME);
            CamStart = new Vec2(CamPosition.X - xCenter, CamPosition.Y + yCenter);
            if (d.LengthSquared() > 0.01f)
                MouseMove(mouseX, mouseY);
            Rectangle rect = new Rectangle((int)CamStart.X, (int) CamStart.Y-height, width, height);

            RenderTargetBinding[] temp = g.GetRenderTargets();
            g.SetRenderTarget(shaderTarget);
            spriteBatch.End();
            Vexillum.game.BeginSpriteBatchScaled();
            Level.Draw(this, g, spriteBatch, rect, (int) CamStart.X, (int) CamStart.Y, width, height);
            spriteBatch.End();
            Vexillum.game.BeginSpriteBatch();
            g.SetRenderTargets(temp);
            shaderTexture = shaderTarget;
            spriteBatch.End();
            Vexillum.game.BeginSpriteBatch(blurEffect);
            blurD.SetValue(Level.cameraShake.XNAVec);
            blurEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(shaderTarget, Vec2.Zero.XNAVec, Color.White);
            spriteBatch.End();
            Vexillum.game.BeginSpriteBatch();
            if (menu == null)
            {
                spriteBatch.Draw(border, Vec2.Zero.XNAVec, Color.White);
                if (player != null && Level.frame != 0 && Level.frame - player.hurtFrame < 20)
                {
                    float alpha = 1 - (float)(Level.frame - player.hurtFrame) / 20;
                    spriteBatch.Draw(border_red, Vec2.Zero.XNAVec, new Color(255, 255, 255, (int) (alpha * 255)));
                }
            }
            if (player != null)
                player.Draw(this, spriteBatch, menu == null && !showScoreboard);
            if (showChatBar)
            {
                TextRenderer.DrawString(spriteBatch, TextRenderer.DefaultFont, chat+"_", chatPos, Color.White, false);
            }
            DrawChats(spriteBatch, showChatBar);
            if (message != null)
            {
                spriteBatch.Draw(GraphicsUtil.pixel, messageBackgroundRectangle, new Color(24, 24, 24, (int)(messageAlpha * 0.75f * 255)));
                TextRenderer.DrawFormattedString(spriteBatch, TextRenderer.FancyFont, message, messagePos, (int) (messageAlpha * 255), false, 0, 0);
            }
            //DebugOverlay.Draw(spriteBatch, this, Level, player.GetClient());
            if (menu != null)
            {
                spriteBatch.Draw(paused, Vec2.Zero.XNAVec, Color.White);
                menu.Draw(spriteBatch);
            }
            else if (showScoreboard)
            {
                scoreboard.Draw(spriteBatch);
            }

        }

        public override void MouseDown(MouseButtons button)
        {
            if (menu != null && button == MouseButtons.Left)
            {
                menu.MouseDown(windowMouseX, windowMouseY);
                return;
            }
            player.MouseDown(button, mouseX, mouseY);
            int worldX = (int)(CamPosition.X - xCenter + mouseX);
            int worldY = (int)(yCenter - mouseY + CamPosition.Y - 1);
            Vec2 diff = new Vec2(worldX, worldY) - player.Position;
            diff.Normalize();
        }
        public override void MouseUp(MouseButtons button)
        {
            player.MouseUp(button, mouseX, mouseY);
        }
        public override void MouseWheelMoved(float ticks)
        {
            if (ticks < 0)
                player.SelectWeapon(player.WeaponIndex + 1);
            else
                player.SelectWeapon(player.WeaponIndex - 1);
            Level.PlaySound(null, Sounds.CLICK2, player.Entity);
        }
        public override void MouseDrag(MouseButtons b, float x, float y)
        {
            if (player.CurrentClass == PlayerClass.Spectator)
            {
                Vec2 diff = new Vec2(x - mouseX, mouseY-y);
                player.Entity.Position -= diff;
                player.Entity.Position = new Vec2(Math.Min(Math.Max(player.Entity.Position.X, 0), Level.Size.X), Math.Min(Math.Max(player.Entity.Position.Y, 0), Level.Size.Y));
                mouseX = (int) x;
                mouseY = (int) y;
            }
        }


        public override void MouseClick(int x, int y)
        {

        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);
            if (player != null)
            {
                player.MouseMove(this, (int) x, (int) y);
            }
        }

        public void AddTask(TaskDelegate task)
        {
            tasks.AddTask(task);
        }
        public void AddConditionalTask(ConditionalTaskDelegate task)
        {
            tasks.AddConditionalTask(task);
        }

        public void AddTask(TaskDelegate task, int frame, Entity entity)
        {
            if (frame == 0)
                tasks.AddTask(task);
            frameTasks.Add(task, frame, entity);
        }

        public void SetFrame(int f)
        {
            Level.frameDiff = f - Level.frame;
            Level.frame = f;
            frameTasks.setFrame(f);
        }

        public void AddChat(string msg)
        {
            ChatMessage chat = new ChatMessage(msg, Level.GetTime());
            lock (chats)
            {
                chats.Add(chat);
                if (chats.Count > 17)
                    chats.RemoveAt(0);
            }
        }

        private void CleanChats(int time)
        {
            lock (chats)
            {
                for (int i = 0; i < chats.Count; i++)
                {
                    ChatMessage c = chats[i];
                    if (time - c.addTime > 20000)
                    {
                        c.old = true;
                    }
                }
            }
        }

        private void DrawChats(SpriteBatch spriteBatch, bool showOld)
        {
            int x = 4;
            int y = yCenter;
            lock (chats)
            {
                for (int i = chats.Count - 1; i >= 0; i--)
                {
                    ChatMessage c = chats[i];
                    if (!showOld && c.old)
                        break;
                    TextRenderer.DrawFormattedString(spriteBatch, TextRenderer.DefaultFont, c.message, new Vec2(x, y), true);
                    y -= chatLineHeight;
                }
            }
        }

        private class ChatMessage
        {
            public string message;
            public int addTime;
            public bool old;
            public ChatMessage(string m, int t)
            {
                message = m;
                addTime = t;
                old = false;
            }
        }
        public void Message(string msg, int time)
        {
            message = msg;
            messageDuration = time;
            messageStartTime = Level.GetTime();
            messageAlpha = 0;
            fadeMessage = false;
            Vec2 messageSize = TextRenderer.MeasureString(TextRenderer.FancyFont, TextRenderer.RemoveColors(message));
            messagePos = new Vec2(xCenter - messageSize.X / 2, messageY);
            messageBackgroundRectangle = new Rectangle((int) (messagePos.X - 10), (int) (messagePos.Y - 10), (int) (messageSize.X + 20), (int) (messageSize.Y + 20));
        }

        public override void Step(GameTime gameTime)
        {
            frameTasks.Process();
            tasks.Process((int)gameTime.TotalGameTime.TotalMilliseconds);
            Level.Step((int) gameTime.TotalGameTime.TotalMilliseconds);
            if(player != null)
                player.Step((int)gameTime.TotalGameTime.TotalMilliseconds);
            CleanChats((int)gameTime.TotalGameTime.TotalMilliseconds);
            if (message != null)
            {
                if (messageAlpha == 1 && !fadeMessage && (int)gameTime.TotalGameTime.TotalMilliseconds - messageStartTime > messageDuration)
                    fadeMessage = true;
                else if(fadeMessage)
                {
                    messageAlpha -= messageAlphaStep;
                    if (messageAlpha <= 0)
                        message = null;
                }
                else if (messageAlpha < 1)
                {
                    messageAlpha += messageAlphaStep;
                    if (messageAlpha > 1)
                        messageAlpha = 1;
                }
            }
        }
        public void UpdateScoreboard()
        {
            scoreboard.Update();
        }
    }
}
