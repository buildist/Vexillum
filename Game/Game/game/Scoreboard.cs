using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.Game
{
    class Scoreboard
    {
        private static Texture2D texture;
        private List<Player> players;
        private Dictionary<PlayerClass, Vec2[,]> positions = new Dictionary<PlayerClass,Vec2[,]>(2);
        private const int max = 12;
        private List<Player> green = new List<Player>(max);
        private List<Player> blue = new List<Player>(max);
        static Scoreboard()
        {
            texture = AssetManager.loadTexture("scoreboard.png");
        }
        public Scoreboard()
        {
            positions[PlayerClass.Green] = new Vec2[max,3];
            positions[PlayerClass.Blue] = new Vec2[max,3];
            int y = 110;
            for (int i = 0; i < max; i++)
            {
                positions[PlayerClass.Green][i,0] = new Vec2(60, y);
                positions[PlayerClass.Blue][i,0] = new Vec2(430, y);
                positions[PlayerClass.Green][i, 1] = new Vec2(340, y);
                positions[PlayerClass.Blue][i, 1] = new Vec2(710, y);
                positions[PlayerClass.Green][i, 2] = new Vec2(384, y);
                positions[PlayerClass.Blue][i, 2] = new Vec2(754, y);
                y += 40;
            }
        }
        private int Compare(Player p1, Player p2)
        {
            return -p1.Score.CompareTo(p2.Score);
        }
        public void Update()
        {
            if(this.players != null)
                SetPlayers(this.players);
        }
        public void SetPlayers(List<Player> players)
        {
            this.players = players;
            green.Clear();
            blue.Clear();
            foreach (Player p in players)
            {
                if (p.CurrentClass == PlayerClass.Green)
                {
                    green.Add(p);
                }
                else if (p.CurrentClass == PlayerClass.Blue)
                {
                    blue.Add(p);
                }
            }
            green.Sort(Compare);
            blue.Sort(Compare);
        }
        public void Draw(SpriteBatch s)
        {
            s.Draw(texture, Vec2.Zero.XNAVec, Color.White);
            int i = 0;
            int j = 0;
            foreach(Player p in green)
            {
                if (p.CurrentClass == PlayerClass.Green && i < max)
                {
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.name, positions[PlayerClass.Green][i, 0], Color.White, false);
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.Score+"", positions[PlayerClass.Green][i,1], Color.White, false);
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.pingString, positions[PlayerClass.Green][i,2], Color.White, false);
                    i++;
                }
            }
            foreach (Player p in blue)
            {
                if (p.CurrentClass == PlayerClass.Blue && j < max)
                {
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.name, positions[PlayerClass.Blue][j, 0], Color.White, false);
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.Score + "", positions[PlayerClass.Blue][j, 1], Color.White, false);
                    TextRenderer.DrawString(s, TextRenderer.TitleFont, p.pingString, positions[PlayerClass.Blue][j, 2], Color.White, false);
                    j++;
                }
            }
        }
    }
}
