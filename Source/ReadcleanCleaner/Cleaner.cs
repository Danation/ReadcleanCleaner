using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ReadcleanCleaner
{
    public class Cleaner
    {
        public TextWriter Output { get; set; }

        private string _originalFilepath;
        private string _destinationFilepath;
        private string _tempDirectory;

        private Dictionary<string, List<string>> _rules = new Dictionary<string, List<string>>();

        public Cleaner(string originalFilepath, string destinationFilepath, string ruleListJSONFilepath)
        {
            if (!File.Exists(originalFilepath) || File.Exists(destinationFilepath))
            {
                throw new IOException("Input file or destination file is invalid");
            }
            _originalFilepath = originalFilepath;
            _destinationFilepath = destinationFilepath;
            _tempDirectory = Path.Combine(Path.GetDirectoryName(_originalFilepath),
                                                   Path.GetFileNameWithoutExtension(_originalFilepath));

            _rules = RulesGenerator.RulesFromJSON(ruleListJSONFilepath);
        }

        public Cleaner(string originalFilepath, string destinationFilepath, Dictionary<string, List<string>> rules)
        {
            if (!File.Exists(originalFilepath) || File.Exists(destinationFilepath))
            {
                throw new IOException("Input file or destination file is invalid");
            }
            _originalFilepath = originalFilepath;
            _destinationFilepath = destinationFilepath;
            _tempDirectory = Path.Combine(Path.GetDirectoryName(_originalFilepath),
                                                   Path.GetFileNameWithoutExtension(_originalFilepath));
            _rules = rules;
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
            foreach (string rule in _rules.Keys)
            {
                result = runRule(result, rule.ToLower());
            }
            return result;
        }

        private string runRule(string original, string searchText)
        {
            List<string> possibleReplacements = _rules[searchText];
            Random random = new Random();

            string result = Regex.Replace(original, searchText, delegate(Match match) {
                string replacement = possibleReplacements[random.Next(possibleReplacements.Count)];
                string value = match.Value;
                string matchResult;

                // All upper case
                if (value.All(c => !char.IsLetter(c) || char.IsUpper(c)))
                {
                    matchResult = replacement.ToUpper();
                }
                // First letter is upper case
                else if (char.IsUpper(value[0]))
                {
                    matchResult = replacement.ToUpperFirstChar();
                }
                // Just do a straight lower case replacement
                else
                {
                    matchResult = replacement;
                }

                // Log result
                if (Output != null)
                {
                    int startIndex = Math.Max(match.Index - 10, 0); //10 chars before or beginning of string
                    int length = Math.Min(value.Length + 20, original.Length - match.Index); //10 chars after or end of string
                    string context = original.Substring(startIndex, length); 
                    Log(string.Format("Replaced: {0} \t With: {1} \t In: {2}", value, matchResult, context));
                }
                return matchResult;
            }, RegexOptions.IgnoreCase);

            return result;
        }

        private void CreateTempDirectory()
        {
            if (Directory.Exists(_tempDirectory))
            {
                int i;
                for (i = 2; Directory.Exists(_tempDirectory + i); i++) { }
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

        private void Log(string message)
        {
            if (Output != null)
            {
                Output.WriteLine(message);
            }
        }
    }
}
