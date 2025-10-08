using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip.Compression.LZMA;

namespace MapTools
{
    public static class MapExtractor
    {
        public static void ExtractMap(String filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: File does not exist.");
                return;
            }

            string mapName = Path.GetFileNameWithoutExtension(filePath);
            string folderPath = Path.Combine(Path.GetDirectoryName(filePath), mapName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            FileStream lzmaInput = File.Open(filePath, FileMode.Open);
            BinaryReader lzmaReader = new BinaryReader(lzmaInput);
            if (lzmaReader.ReadInt32() != MapUtil.magic)
            {
                Console.WriteLine("Error: Not a valid map file.");
                return;
            }
            byte[] compressedBytes = new byte[lzmaInput.Length - 4];
            int i = 0;
            while (lzmaInput.Position < lzmaInput.Length)
            {
                compressedBytes[i] = (byte) lzmaInput.ReadByte();
                i++;
            }
            lzmaReader.Close();

            byte[] bytes = SevenZipHelper.Decompress(compressedBytes);
            MemoryStream input = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(input);

            string name = reader.ReadString();
            while (input.Position < input.Length)
            {
                string imgName = reader.ReadString();
                if (imgName.Trim() == "")
                    break;
                FileStream imgFile = File.Open(Path.Combine(folderPath, mapName + "_" + imgName), FileMode.Create); 

                long length = reader.ReadInt64();
                for (int j = 0; j < length; j++)
                {
                    imgFile.WriteByte(reader.ReadByte());
                }
                imgFile.Close();
            }

            reader.Close();
            Console.WriteLine("Map files saved to" + folderPath);
        }
    }
}
