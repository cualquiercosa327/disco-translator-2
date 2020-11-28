using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DiscoTranslator2
{
    static class TranslReader
    {
        public static Dictionary<string, string> ReadAllFiles(string path)
        {
            //find all transl files
            string[] translFiles = Directory.GetFiles(path, "*.transl", SearchOption.AllDirectories);

            //open and process each file
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string translPath in translFiles)
            {
                //read single file
                Dictionary<string, string> fileOutput = ReadFile(translPath);

                //merge output
                foreach (KeyValuePair<string, string> kvp in fileOutput)
                    AddEntry(ref output, kvp.Key, kvp.Value, translPath);
            }

            return output;
        }
        public static Dictionary<string, string> ReadFile(string path)
        {
            //open file and prepare output dictionary
            StreamReader reader = File.OpenText(path);
            Dictionary<string, string> output = new Dictionary<string, string>();

            //parser state
            string entryId = null;
            string entryText = "";
            bool continued = false;

            //read lines until end of file reached
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                //trim whitespace
                line = line.Trim();
                if (line == String.Empty && !continued) continue;

                //handle comments
                if (line.StartsWith("#")) continue;
                if (line.StartsWith(@"\#")) line = line.Substring(1);
                if (line.Contains("#")) line = line.Substring(0, line.IndexOf('#'));
                line = line.TrimEnd();

                //handle continuation
                if (!continued)
                {
                    //last entry finished
                    if (entryId != null) AddEntry(ref output, entryId, entryText, path);

                    //separate line into id and text
                    int separator = line.IndexOf(": ");
                    if (separator == -1)
                    {
                        DiscoTranslator2.PluginLogger.LogWarning("Warning: malformed transl file");
                        DiscoTranslator2.PluginLogger.LogWarning("Missing separator: " + line);
                        DiscoTranslator2.PluginLogger.LogWarning("Source file: " + path);
                        entryId = null;
                        entryText = "";
                        continue;
                    }

                    //update parser state
                    entryId = line.Substring(0, separator);
                    entryText = line.Substring(separator + 2);
                }
                else
                {
                    //append text to entry and finish continuation
                    entryText += line;
                    continued = false;
                }

                //detect future continuation
                if (entryText.EndsWith(@"\"))
                {
                    entryText = entryText.Substring(0, entryText.Length - 1) + '\n';
                    continued = true;
                }
            }

            //add last entry
            if (entryId != null) AddEntry(ref output, entryId, entryText, path);

            reader.Close();
            return output;
        }
        static void AddEntry(ref Dictionary<string, string> dictionary, string key, string value, string source)
        {
            //detect collisions
            if (dictionary.ContainsKey(key))
            {
                DiscoTranslator2.PluginLogger.LogWarning("Warning: duplicate entry key: " + key);
                DiscoTranslator2.PluginLogger.LogWarning("Source file: " + source);

                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }
    }
}
