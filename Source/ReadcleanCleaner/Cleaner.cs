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

        private Dictionary<string, List<string>> _rules = new Dictionary<string, List<string>>();

        public Cleaner(string originalFilepath, string destinationFilepath, string ruleListJSONFilepath)
        {
            if (!File.Exists(originalFilepath) || File.Exists(destinationFilepath))
            {
                throw new IOException("Input file or destination file is invalid");
            }
            _originalFilepath = originalFilepath;
            _destinationFilepath = destinationFilepath;
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
            _rules = rules;
        }

        /// <summary>
        /// Given the epub in the contructor, this copies the epub and cleans the copy
        /// </summary>
        public void Clean()
        {
            CreateNewEpub();

            using (ZipArchive archive = ZipFile.Open(_destinationFilepath, ZipArchiveMode.Update))
            {
                CleanArchive(archive);
            }
        }

        private void CreateNewEpub()
        {
            File.Copy(_originalFilepath, _destinationFilepath, true);
        }

        private void CleanArchive(ZipArchive archive)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.ToLower().Contains("htm"))
                {
                    CleanZipArchiveEntry(entry);
                }
            }
        }

        private void CleanZipArchiveEntry(ZipArchiveEntry entry)
        {
            string fileContents = ReadEntryContents(entry);
            string changedContents = RunAllRules(fileContents);

            WriteNewEntryContents(entry, changedContents);
        }

        private string ReadEntryContents(ZipArchiveEntry entry)
        {
            string fileContents;
            using (Stream stream = entry.Open())
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    fileContents = reader.ReadToEnd();
                    reader.Close();
                }
            }

            return fileContents;
        }

        private void WriteNewEntryContents(ZipArchiveEntry entry, string newContents)
        {
            using (Stream stream = entry.Open())
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    writer.Write(newContents);
                    writer.Close();
                }
            }
        }

        private string RunAllRules(string original)
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
            Random random = new Random(1366);

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
                    Log(string.Format("Replaced: {0} \t With: {1}\n In: {2}\n", value, matchResult, context));
                }
                return matchResult;
            }, RegexOptions.IgnoreCase);

            return result;
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
