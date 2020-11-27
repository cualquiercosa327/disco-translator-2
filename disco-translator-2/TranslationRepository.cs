using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator2
{
    static class TranslationRepository
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public static bool Resolve(string resourceId, out string output)
        {
            //if the dictionary has no translation, return false
            if (!dictionary.ContainsKey(resourceId))
            {
                output = null;
                return false;
            }

            //otherwise provide translation and return true
            output = dictionary[resourceId];
            return true;
        }
    }
}
