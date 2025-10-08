using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using SevenZip;
using SevenZip.Compression.LZMA;
using System.Drawing.Imaging;

namespace MapTools
{
    public static class MapCreator
    {
        public static void createMap(String folderPath, String friendlyName)
        {
            string mapName = Path.GetFileName(folderPath);

            MemoryStream temp = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(temp);
            writer.Write(friendlyName);
            foreach (string name in MapUtil.GetFileNames())
            {
                Console.WriteLine("Adding file: " + name);
                writer.Write(name);
                string fileName = Path.Combine(folderPath, mapName + "_" + name);

                if (!File.Exists(fileName))
                {
                    Console.WriteLine("Error: Couldn't find required file " + fileName);
                    return;
                }
                FileStream imgStream = File.Open(fileName, FileMode.Open);
                BinaryReader reader = new BinaryReader(imgStream);
                writer.Write(imgStream.Length);
                while (imgStream.Position < imgStream.Length)
                {
                    writer.Write((byte)reader.ReadByte());
                }
                reader.Close();
            }
            writer.Flush();

            Console.WriteLine("Compressing...");
            temp.Seek(0, SeekOrigin.Begin);

            string outputName = Path.Combine(folderPath, mapName + ".map");
            FileStream output = File.Open(outputName, FileMode.Create);
       
            BinaryWriter outWriter = new BinaryWriter(output);
            outWriter.Write(MapUtil.magic);
            outWriter.Write(SevenZipHelper.Compress(temp.GetBuffer()));
            
            Console.WriteLine("Map saved to " + outputName);

            outWriter.Close();
            temp.Close();
        }
        private static ImageCodecInfo GetJpgEncoder()
        {
            foreach(ImageCodecInfo c in ImageCodecInfo.GetImageEncoders())
            {
                if (c.FormatID == ImageFormat.Jpeg.Guid)
                    return c;
            }
            return null;
        }
    }
}
