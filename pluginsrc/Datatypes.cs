using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator2.Datatypes
{
    [Serializable]
    class TranslationDatabase
    {
        public List<Conversation> conversations = new List<Conversation>();
        public List<MiscEntry> miscellaneous = new List<MiscEntry>();
    }
    [Serializable]
    class Conversation
    {
        public string title;
        public string type = "dialogue";
        public Dictionary<string, string> metadata = new Dictionary<string, string>();
        public List<DialogueEntry> entries = new List<DialogueEntry>();
    }
    [Serializable]
    class DialogueEntry
    {
        public string id;
        public string text;
        public string actor;
    }
    [Serializable]
    class MiscEntry
    {
        public string id;
        public string text;
        public string type = "misc";
    }
}
