using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MapTools
{
    class MapUtil
    {
        public static int magic = 0x004F876B;
        private static string[] files = null;
        public static string[] GetFileNames()
        {
            if (files != null)
                return files;
            String path = "mapfile.list";
            if (!File.Exists(path))
                throw new Exception("Could not load map data");
            List<string> result = new List<string>(8);
            using (StreamReader r = File.OpenText(path))
            {
                string s;
                while ((s = r.ReadLine()) != null)
                {
                    result.Add(s);
                }
            }
            files = result.ToArray();
            return files;
        }
    }
}
