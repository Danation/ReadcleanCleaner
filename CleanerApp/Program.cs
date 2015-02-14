using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Cleaner cleaner = new Cleaner(filepath, filepath + "_clean"); // TODO handle file extension better
            cleaner.Clean();

            Console.ReadLine();
        }

        public static void printUsage()
        {
            // TODO
        }
    }
}
