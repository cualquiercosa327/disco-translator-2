using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator2.Datatypes
{
    [Serializable]
    class TranslationDatabase
    {
        public List<Conversation> conversations = new List<Conversation>();
    }
    [Serializable]
    class Conversation
    {
        public string title;
        public List<DialogueEntry> entries = new List<DialogueEntry>();
    }
    [Serializable]
    class DialogueEntry
    {
        public string id;
        public string text;
        public string actor;
    }
}
