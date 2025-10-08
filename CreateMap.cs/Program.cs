using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateMap
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the map's folder:");
            string shortName = Console.ReadLine();
            Console.WriteLine("Enter the human-readable map name:");
            string friendlyName = Console.ReadLine();
            Console.WriteLine("Creating map...");
            MapTools.MapCreator.createMap(shortName, friendlyName);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
