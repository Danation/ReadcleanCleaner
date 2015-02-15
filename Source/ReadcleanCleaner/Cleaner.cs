using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ReadcleanCleaner
{
    public class Cleaner
    {
        private string _originalFilepath;
        private string _destinationFilepath;
        private string _tempDirectory;

        private Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();

        public Cleaner(string originalFilepath, string destinationFilepath)
        {
            if (!File.Exists(originalFilepath) || File.Exists(destinationFilepath))
            {
                throw new IOException("Input file or destination file is invalid");
            }
            _originalFilepath = originalFilepath;
            _destinationFilepath = destinationFilepath;
            _tempDirectory = Path.Combine(Path.GetDirectoryName(_originalFilepath),
                                                   Path.GetFileNameWithoutExtension(_originalFilepath));

            //TODO read rules from file
            List<string> hellList = new List<string>() { "heck", "hades" };
            rules.Add("hell", hellList);

            List<string> damnList = new List<string>() { "dang", "darn" };
            rules.Add("damn", damnList);
        }

        public void Clean()
        {
            CreateTempDirectory();
            ExtractEpub();
            CleanAllFiles();
            CreateNewEpub();
            DeleteTempDirectory();
        }

        private void CleanAllFiles()
        {
            string[] fileList = GetAllFiles();
            foreach (string file in fileList)
            {
                CleanFile(file);
            }
        }

        private void CleanFile(string filepath)
        {
            string filetext = File.ReadAllText(filepath);
            string newFiletext = runAllRules(filetext);
            File.WriteAllText(filepath, newFiletext);
        }

        private string runAllRules(string original)
        {
            string result = original;
            foreach (string rule in rules.Keys)
            {
                result = runRule(result, rule.ToLower(), rules[rule]);
            }
            return result;
        }

        private string runRule(string original, string searchText, List<string> possibleReplacements)
        {
            
            string result = original;

            string replacement = possibleReplacements[0]; //TODO randomly choose which replacement to use
            
            result = result.Replace(searchText.ToUpper(), replacement.ToUpper());
            result = result.Replace(searchText.ToUpperFirstChar(), replacement.ToUpperFirstChar());
            result = result.ReplaceCaseInsensitiveFind(searchText, replacement);

            return result;
        }

        private void CreateTempDirectory()
        {
            if (Directory.Exists(_tempDirectory))
            {
                int i;
                for (i = 2; Directory.Exists(_tempDirectory + i); i++) {}
                _tempDirectory += i;
            }
            Directory.CreateDirectory(_tempDirectory);
        }

        private string[] GetAllFiles()
        {
            return Directory.GetFiles(_tempDirectory, "*.*", SearchOption.AllDirectories);
        }

        private void ExtractEpub()
        {
            ZipFile.ExtractToDirectory(_originalFilepath, _tempDirectory);
        }

        private void CreateNewEpub()
        {
            ZipFile.CreateFromDirectory(_tempDirectory, _destinationFilepath);
        }

        private void DeleteTempDirectory()
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
