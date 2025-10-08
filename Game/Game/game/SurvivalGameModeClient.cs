using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.util;
using Vexillum.Entities.Weapons;
using Vexillum.view;
using Vexillum.ui;
using Vexillum.net;

namespace Vexillum.Game
{
    class SurvivalGameModeClient : SurvivalGameModeShared
    {
        private Client client;

        protected static Texture2D ui;
        protected static Rectangle greenIndicator = new Rectangle(0, 30, 15, 17);
        protected static Rectangle blueIndicator = new Rectangle(17, 30, 15, 17);
        protected static Vec2 indicatorOffset = new Vec2(-8, -56);
        protected static Rectangle topLeftTexture = new Rectangle(308, 0, 40, 40);
        protected static Rectangle topRightTexture = new Rectangle(0, 0, 180, 90);
        protected static Rectangle bottomRightTexture = new Rectangle(0, 92, 180, 90);
        protected static Rectangle bottomLeftTexture = new Rectangle(185, 92, 180, 56);
        protected static Rectangle weaponSelectTexture = new Rectangle(185, 0, 60, 40);
        protected static Rectangle weaponSelectActiveTextureOrange = new Rectangle(246, 0, 60, 40);
        protected static Rectangle weaponSelectActiveTextureGreen = new Rectangle(246, 42, 60, 40);
        protected static Rectangle bgPixel = new Rectangle(181, 0, 1, 1);
        protected static Rectangle healthPixel = new Rectangle(182, 1, 1, 1);
        protected static Rectangle humanPixel = new Rectangle(182, 0, 1, 1);
        protected static Rectangle zombiePixel = new Rectangle(183, 0, 1, 1);
        protected static Rectangle flagSafeTexture = new Rectangle(9, 225, 10, 11);
        protected static Rectangle flagTakenTexture = new Rectangle(21, 225, 10, 11);
        protected static Rectangle greenFlagRectangle = new Rectangle(Vexillum.WindowWidth - 128, Vexillum.WindowHeight - 45, 10, 11);
        protected static Rectangle blueFlagRectangle = new Rectangle(Vexillum.WindowWidth - 62, Vexillum.GameHeight - 45, 10, 11);
        protected static Vec2 greenScorePos = new Vec2(Vexillum.WindowWidth - 112, Vexillum.WindowHeight - 67);
        protected static Vec2 blueScorePos = new Vec2(Vexillum.WindowWidth - 68, Vexillum.WindowHeight - 67);
        protected static Rectangle topLeft = new Rectangle(0, 0, 40, 40);
        protected static Rectangle topRight = new Rectangle(Vexillum.WindowWidth - 180, 0, 180, 90);
        protected static Rectangle bottomLeft = new Rectangle(0, Vexillum.WindowHeight - 56, 180, 56);
        protected static Rectangle bottomRight = new Rectangle(Vexillum.WindowWidth - 180, Vexillum.WindowHeight - 90, 180, 90);
        protected const int weaponStartX = 50;

        protected ProgressBar redBar;
        protected ProgressBar greenBar;
        protected ProgressBar healthBar;
        protected AmmoIndicator ammoIndicator;
        private string maxCaptureString;
        private Vec2 maxCapturePos;


        public SurvivalGameModeClient(Client c)
        {
            client = c;
            //redBar = new ProgressBar(Vexillum.GameWidth - 150, 16, TextRenderer.TitleFont, ui, humanPixel, bgPixel, 130, 0, true);
            //greenBar = new ProgressBar(Vexillum.GameWidth - 150, 36, TextRenderer.TitleFont, ui, zombiePixel, bgPixel, 130, 0, true);
            healthBar = new ProgressBar(20, Vexillum.GameHeight - 36, TextRenderer.TitleFont, ui, healthPixel, bgPixel, 130, 0, true);
            healthBar.SetMaxValue(100);
            healthBar.SetValue(100);
            ammoIndicator = new AmmoIndicator(ui, 5, 5);
        }
        public static void LoadContent()
        {
            ui = AssetManager.loadTexture("ui/gameui.png");
            ctfTexture = AssetManager.loadTexture("ctf.png");
        }
        public override void UpdatePlayerList()
        {

        }
        public override void DrawUI(GameView view, SpriteBatch spriteBatch, LocalPlayer player)
        {
            PlayerClass pClass = player.CurrentClass;

            if (player.Entity != null)
            {
                spriteBatch.Draw(ui, topLeft, topLeftTexture, Color.White);
                if(player.Entity.GetClipAmmo() != -1 && player.CurrentClass != PlayerClass.Spectator)
                    ammoIndicator.Draw(spriteBatch, player.Entity.GetClipAmmo(), player.Entity.GetMaxAmmo(), player.Entity.GetClipAmmo(), player.Entity.GetMaxClipAmmo(), Colors.ammoIndicatorColor);

                int weaponX = weaponStartX;
                int i = 1;
                foreach (Weapon w in player.Inventory)
                {
                    spriteBatch.Draw(ui, new Rectangle(weaponX, 0, 60, 40), w == player.Entity.Weapon ? weaponSelectActiveTextureOrange : weaponSelectTexture, Color.White);
                    Texture2D texture = w.GetImage();
                    spriteBatch.Draw(texture, new Vec2(weaponX + (60 - texture.Width) / 2, (40 - texture.Height) / 2).XNAVec, Color.White);
                    TextRenderer.DrawString(spriteBatch, TextRenderer.TinyFont, "" + i, new Vec2(weaponX + 6, 6), Color.White, false);
                    weaponX += 65;
                    i++;
                }
            }

            spriteBatch.Draw(ui, bottomLeft, bottomLeftTexture, Color.White);
            healthBar.Draw(spriteBatch);
            spriteBatch.Draw(ui, bottomRight, bottomRightTexture, Color.White);
            if (blueFlagCarrier != null)
                spriteBatch.Draw(ui, greenFlagRectangle, flagTakenTexture, Color.White);
            else
                spriteBatch.Draw(ui, greenFlagRectangle, flagSafeTexture, Color.White);
            if (greenFlagCarrier != null)
                spriteBatch.Draw(ui, blueFlagRectangle, flagTakenTexture, Color.White);
            else
                spriteBatch.Draw(ui, blueFlagRectangle, flagSafeTexture, Color.White);
            TextRenderer.DrawString(spriteBatch, TextRenderer.TitleFont, ""+greenCaptures, greenScorePos, Color.White, false);
            TextRenderer.DrawString(spriteBatch, TextRenderer.TitleFont, ""+blueCaptures, blueScorePos + new Vec2(-TextRenderer.MeasureString(TextRenderer.TitleFont, "" + greenCaptures).X, 0), Color.White, false);
            if(maxCaptureString != null)
                TextRenderer.DrawString(spriteBatch, TextRenderer.DefaultFont, maxCaptureString, maxCapturePos, Color.White, false);
        }

        public override void PlayerHit(Player attacker, Player defender, Weapon weapon, Vec2 hitPos)
        {

        }

        public override void PlayerHealthChanged(Player p, Weapon w)
        {
            if (p == client.player)
            {
                healthBar.SetValue((int)p.Entity.Health);
                if (p.Entity.Health != p.Entity.MaxHealth)
                {
                    client.player.hurtFrame = level.frame;
                }
                if (p.Entity.Health == 0)
                {
                    if (p.Entity.Weapon != null)
                        p.Entity.Weapon.MouseUpClient(client, level, 0, 0, 0);
                }
            }
        }

        public override void ProcessCommand(int c, object value)
        {
            switch (c)
            {
                case GameModeCommand.BLUE_SCORE:
                    blueCaptures = (byte)value;
                    break;
                case GameModeCommand.GREEN_SCORE:
                    greenCaptures = (byte)value;
                    break;
                case GameModeCommand.BLUEFLAG_CARRIER:
                    int id = (short)value;
                    if (id == -1)
                        blueFlagCarrier = null;
                    else
                        blueFlagCarrier = level.GetEntityByID(id).player;
                    break;
                case GameModeCommand.GREENFLAG_CARRIER:
                    int id2 = (short)value;
                    if (id2 == -1)
                        greenFlagCarrier = null;
                    else
                        greenFlagCarrier = level.GetEntityByID(id2).player;
                    break;
                case GameModeCommand.MAX_CAPTURES:
                    maxCaptures = (byte)value;
                    maxCaptureString = "Playing to: " + maxCaptures;
                    maxCapturePos = new Vec2(Vexillum.WindowWidth - 180 + 180/2 - TextRenderer.MeasureString(TextRenderer.DefaultFont, maxCaptureString).X/2, Vexillum.WindowHeight - 20);
                    break;
                case GameModeCommand.GREEN_WIN:
                    Win(PlayerClass.Green);
                    break;
                case GameModeCommand.BLUE_WIN:
                    Win(PlayerClass.Blue);
                    break;
            }
        }
        private void Win(PlayerClass cl)
        {
            gameOver = true;
        }
        public override void DrawEntity(Entities.HumanoidEntity e, SpriteBatch s)
        {
            if (e.player == greenFlagCarrier)
            {
                s.Draw(ctfTexture, (e.GetScreenPosition() + indicatorOffset).XNAVec, blueIndicator, Color.White);
            }
            else if (e.player == blueFlagCarrier)
            {
                s.Draw(ctfTexture, (e.GetScreenPosition() + indicatorOffset).XNAVec, greenIndicator, Color.White);
            }
        }
    }
}
