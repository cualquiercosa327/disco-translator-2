using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Newtonsoft.Json;
using DT2 = DiscoTranslator2.Datatypes;

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
            DT2.TranslationDatabase data = new DT2.TranslationDatabase();
            foreach (var convo in databases[0].conversations)
            {
                //create conversation structure
                DT2.Conversation conversation = new DT2.Conversation();
                conversation.title = convo.Title;

                //obtain dialogue entry list
                foreach (var entry in convo.dialogueEntries)
                {
                    //skip empty entries
                    if (String.IsNullOrWhiteSpace(entry.DialogueText))
                        continue;

                    //construct entry structure
                    DT2.DialogueEntry dialogueEntry = new DT2.DialogueEntry();
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
}
