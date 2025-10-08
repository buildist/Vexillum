using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;

namespace Vexillum.util
{
    class RandomSoundPlayer
    {
        private Sounds[] sounds;
        private Random random = new Random();
        private int lastRandom = -1;
        private int count;
        public RandomSoundPlayer(Sounds[] sounds)
        {
            this.sounds = sounds;
            count = sounds.Length;
        }
        private int UniqueRandom()
        {
            int r = random.Next(count);
            if (r == lastRandom)
                return UniqueRandom();
            else
                return r;
        }
        public void PlaySound(Level Level, Entity entity)
        {
            int r = UniqueRandom();
            lastRandom = r;
            Level.PlaySound(null, sounds[r], entity);
        }
    }
}
