using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Drawing;
using Vexillum.util;

namespace Vexillum
{
    public static class AssetManager
    {
        private static Dictionary<String, Texture2D> textures = new Dictionary<String, Texture2D>();
        private static Dictionary<Sounds, SoundEffect> sounds = new Dictionary<Sounds, SoundEffect>();
        private static Type soundType = typeof(Sounds);
        public static Texture2D loadTexture(string path)
        {
            if (Util.IsServer)
                return null;
            if (!textures.ContainsKey(path))
                textures[path] = Util.loadTexture(path);
            return textures[path];
        }
        public static Texture2D loadTexture(string path, Bitmap bmp)
        {
            if (!textures.ContainsKey(path))
                textures[path] = Util.loadTexture(bmp);
            return textures[path];
        }
        public static void addSound(Sounds name, SoundEffect effect)
        {
            sounds[name] = effect;
        }
        public static int GetIndex(Sounds name)
        {
            return Array.IndexOf(Enum.GetValues(soundType), name);
        }
        public static Sounds GetSound(int index)
        {
            return (Sounds) Enum.GetValues(soundType).GetValue(index);
        }
        public static SoundEffect GetSound(Sounds name)
        {
            return sounds[name];
        }
    }
}
