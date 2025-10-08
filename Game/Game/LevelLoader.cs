using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip.Compression.LZMA;
using System.Drawing;
using System.Security.Cryptography;
using Vexillum.Game;

namespace Vexillum
{
    public class LevelLoader
    {
        public static int MagicNumber = 0x004F876B;
        private static Random rand = new Random();
        private static List<string> levelNames = new List<string>();
        public static string GetRandomLevel()
        {
            int idx = rand.Next(levelNames.Count);
            return levelNames[idx];
        }
        public static void LoadLevelList(string[] names)
        {
            List<string> list = null;
            if(names != null)
                list = new List<string>(names);
            DirectoryInfo info = new DirectoryInfo("Maps");
            foreach (FileInfo file in info.GetFiles())
            {
                string name = file.Name.Replace(".map", "");
                if(list == null || list.Contains(name))
                    levelNames.Add(name);
            }
        }
        public static LevelData LoadData(String name)
        {
            byte[] compressedBytes;
            string filePath = Path.Combine("Maps", name + ".map");
            if (!File.Exists(filePath))
                return null;
            FileStream lzmaInput = File.OpenRead(filePath);

            BinaryReader lzmaReader = new BinaryReader(lzmaInput);
            if (lzmaReader.ReadInt32() != MagicNumber)
            {
                Util.Debug("Error: Not a valid map file.");
                return null;
            }
            try
            {
                compressedBytes = lzmaReader.ReadBytes((int) lzmaInput.Length - 4);
                lzmaReader.Close();

                byte[] bytes = SevenZipHelper.Decompress(compressedBytes);
                MemoryStream input = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(input);

                string longName = reader.ReadString();
                List<util.Region> regions = new List<util.Region>();
                Dictionary<String, Bitmap> bitmaps = new Dictionary<string, Bitmap>(4);
                while (input.Position < input.Length)
                {
                    string fileName = reader.ReadString();
                    string imgName = fileName.Split('.')[0];
                    if (imgName.Trim() == "")
                        break;
                    long length = reader.ReadInt64();
                    MemoryStream stream = new MemoryStream();
                    stream.Write(reader.ReadBytes((int) length), 0, (int) length);
                    stream.Seek(0, SeekOrigin.Begin);
                    if(fileName.EndsWith(".txt"))
                    {
                        switch (imgName)
                        {
                            case "data":
                                StreamReader sr = new StreamReader(stream);
                                string s;
                                int l = 1;
                                while ((s = sr.ReadLine()) != null)
                                {
                                    try
                                    {
                                        string[] parts = s.Split(' ');
                                        string cmd = parts[0];
                                        switch (cmd)
                                        {
                                            case "region":
                                                string rname = parts[1];
                                                int x1 = int.Parse(parts[2]);
                                                int y1 = int.Parse(parts[3]);
                                                int x2 = int.Parse(parts[4]);
                                                int y2 = int.Parse(parts[5]);

                                                regions.Add(new util.Region(rname, x1, y1, x2, y2));
                                                break;
                                        }
                                        l++;
                                    }
                                    catch (Exception ex)
                                    {
                                        Vexillum.Error("Parse error on line " + l+" of data.txt for map "+name);
                                    }
                                }
                            break;
                        }
                    }
                    else
                    {
                        Bitmap bmp = new Bitmap(stream);
                        bmp.MakeTransparent(GraphicsUtil.transparent);
                        bitmaps[imgName] = bmp;
                    }
                }
                reader.Close();
                return new LevelData(compressedBytes, name, longName, bitmaps, regions);
            }
            catch (Exception ex)
            {
                Util.Debug("Error loading level:");
                Util.Debug(ex.StackTrace);
                return null;
            }
        }
        public static ClientLevel Load(string name)
        {
            LevelData d = LoadData(name);
            if (d == null)
                return null;
            else
                return new ClientLevel(name, d.longName, d.bitmaps["main"], d.bitmaps["background"], d.bitmaps["sky"], d.bitmaps["left"], d.bitmaps["right"], d.bitmaps["bottom"], d.bitmaps["collision"], d.regions);
        }
        public static bool LevelExists(string name)
        {
            return File.Exists(Path.Combine("Maps", name + ".map"));
        }
        public static byte[] LevelMd5(String name)
        {
            FileStream stream = File.OpenRead(Path.Combine("Maps", name + ".map"));
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            stream.Close();
            return hash;
        }
        public class LevelData
        {
            public String shortName;
            public String longName;
            public Dictionary<String, Bitmap> bitmaps;
            public List<util.Region> regions;
            public byte[] bytes;
            public LevelData(byte[] fileBytes, string s, string l, Dictionary<String, Bitmap> b, List<util.Region> regions)
            {
                this.regions = regions;
                shortName = s;
                longName = l;
                bitmaps = b;
                bytes = fileBytes;
            }
        }
    }
}
