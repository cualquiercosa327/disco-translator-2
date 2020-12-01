using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator2.Datatypes
{
    [Serializable]
    class TranslationDatabase
    {
        public List<Conversation> conversations = new List<Conversation>();
        public Dictionary<string, Dictionary<string, string>> miscellaneous = new Dictionary<string, Dictionary<string, string>>();
    }
    [Serializable]
    class Conversation
    {
        public string title;
        public string type = "dialogue";
        public List<string> roots = new List<string>();
        public Dictionary<string, string> metadata = new Dictionary<string, string>();
        public Dictionary<string, DialogueEntry> entries = new Dictionary<string, DialogueEntry>();
    }
    [Serializable]
    class DialogueEntry
    {
        public string actor;
        public Dictionary<string, string> fields = new Dictionary<string, string>();
        public List<string> leadsTo = new List<string>();
    }
}
