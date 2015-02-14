using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReadcleanCleaner;

namespace CleanerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                printUsage();
                return;
            }

            string filepath = args[0];

            if (!File.Exists(filepath) || File.GetAttributes(filepath).HasFlag(FileAttributes.Encrypted | FileAttributes.System))
            {
                Console.WriteLine("File does not exist or is unreadable");
                return;
            }

            string suffix = "_clean";
            string newFilepath = Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + suffix + Path.GetExtension(filepath));

            Cleaner cleaner = new Cleaner(filepath, newFilepath);
            cleaner.Clean();

            Console.WriteLine(newFilepath);
            Console.ReadLine();
        }

        public static void printUsage()
        {
            // TODO
        }
    }
}
