using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Newtonsoft.Json;

namespace DiscoTranslator2
{
    static class ResourceExtractor
    {
        public static bool Extracted = false;

        public static void Extract(string path)
        {
            //obtain dialogue database
            DialogueDatabase[] databases = Resources.FindObjectsOfTypeAll<DialogueDatabase>();

            //mark extraction as successful
            if (databases.Length == 0) return;
            Extracted = true;

            //find English dialogue
            DialogueList data = new DialogueList();
            foreach (var convo in databases[0].conversations)
            {
                //create conversation structure
                Conversation conversation = new Conversation();
                conversation.title = convo.Title;

                //obtain dialogue entry list
                foreach (var entry in convo.dialogueEntries)
                {
                    //skip empty entries
                    if (String.IsNullOrWhiteSpace(entry.DialogueText))
                        continue;

                    //construct entry structure
                    DialogueEntry dialogueEntry = new DialogueEntry();
                    dialogueEntry.id = Field.LookupValue(entry.fields, "Articy Id");
                    dialogueEntry.text = entry.DialogueText;
                    dialogueEntry.actor = databases[0].GetActor(entry.ActorID).Name;

                    conversation.entries.Add(dialogueEntry);
                }

                //skip empty conversations
                if (conversation.entries.Count > 0)
                    data.conversations.Add(conversation);
            }

            //write json to database file
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path.Combine(path, "database.json"), json);
        }
    }

    [Serializable]
    class DialogueList
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
