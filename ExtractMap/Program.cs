using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractMap
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the map file:");
            string path = Console.ReadLine();
            Console.WriteLine("Extracting map...");
            MapTools.MapExtractor.ExtractMap(path);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
