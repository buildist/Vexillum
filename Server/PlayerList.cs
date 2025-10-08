using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum;
using System.IO;

namespace Server
{
    class PlayerList
    {
        private static Dictionary<string, List<string>> lists = new Dictionary<string, List<string>>();
        public static void Load(string name)
        {
            lists[name] = new List<string>(32);
            using (FileStream listFile = File.Open(Util.GetServerFile(name+".txt"), FileMode.OpenOrCreate))
            {
                StreamReader sr = new StreamReader(listFile);
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    lists[name].Add(line);
                }
            }
        }
        public static void Save(string name)
        {
            try
            {
                using (FileStream listFile = File.Open(name+".txt", FileMode.Truncate))
                {
                    StreamWriter sw = new StreamWriter(listFile);
                    foreach (string playerName in lists[name])
                    {
                        sw.WriteLine(playerName);
                    }
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Util.Debug("Error saving "+name+" list:" +ex.ToString());
            }
        }
        public static void Add(string list, string player)
        {
            if (!lists[list].Contains(player))
            {
                lists[list].Add(player);
                Save(list);
            }
        }
        public static void Remove(string list, string player)
        {
            lists[list].Remove(player);
            Save(list);
        }
        public static bool Check(string list, string player)
        {
            return lists[list].Contains(player);
        }
    }
}
