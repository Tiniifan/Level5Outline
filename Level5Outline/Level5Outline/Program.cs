using System;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Level5Outline.Level5.Outline;

namespace Level5Outline
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Options:");
                Console.WriteLine("-h: help");
                Console.WriteLine("-d [input_path]: decompress .sil to json");
                Console.WriteLine("-c [input_path]: compress readable json to .sil");
                return;
            }

            if (args[0] == "-h")
            {
                Console.WriteLine("Options:");
                Console.WriteLine("-h: help");
                Console.WriteLine("-d [input_path]: decompress .sil to json");
                Console.WriteLine("-c [input_path]: compress readable json to .sil");
            }
            else if (args[0] == "-d")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide input and output file names for the -d option.");
                    return;
                }

                XCSL outline = new XCSL(new FileStream(args[1], FileMode.Open, FileAccess.Read));

                // Get the output path
                string outputFileName = Path.GetFileNameWithoutExtension(args[1]) + ".json";
                string outputDirectory = Path.Combine(Path.GetDirectoryName(args[1]), outputFileName);

                // Convert as json
                string jsonContent = JsonConvert.SerializeObject(outline, Formatting.Indented);
                File.WriteAllText(outputDirectory, jsonContent);
            }
            else if (args[0] == "-c")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide input and output file names for the -c option.");
                    return;
                }

                // Convert json to XCSL object
                XCSL outline = JsonConvert.DeserializeObject<XCSL>(string.Join("", File.ReadAllLines(args[1])));

                // Get the output path
                string outputFileName = Path.GetFileNameWithoutExtension(args[1]) + ".sil";
                string outputDirectory = Path.Combine(Path.GetDirectoryName(args[1]), outputFileName);

                // Save as sil
                outline.Save(outputDirectory);
            }

        }
    }
}
