using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ReadcleanCleaner
{
    public static class RulesGenerator
    {
        public static Dictionary<string, List<string>> RulesFromJSON(string jsonFilename)
        {
            Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();

            using (FileStream ms = File.OpenRead(jsonFilename))
            {
                DataContractJsonSerializerSettings settings =
                        new DataContractJsonSerializerSettings();
                settings.UseSimpleDictionaryFormat = true;

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RulesDicts), settings);

                RulesDicts results = (RulesDicts)serializer.ReadObject(ms);

                foreach (var pair in results.rules)
                {
                    rules.Add(pair.Key.ToLower(), results.replacements[pair.Value]);
                }
            }

            return rules;
        }

        [DataContract]
        private class RulesDicts
        {
            [DataMember(Name = "rules")]
            public Dictionary<string, string> rules = new Dictionary<string, string>();

            [DataMember(Name = "replacements")]
            public Dictionary<string, List<string>> replacements = new Dictionary<string, List<string>>();
        }
    }
}
