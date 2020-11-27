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

                //obtain conversation metadata
                foreach (var field in convo.fields)
                {
                    //skip empty entries
                    if (String.IsNullOrWhiteSpace(field.value))
                        continue;

                    //skip irrelevant fields
                    string articyId = Field.LookupValue(convo.fields, "Articy Id");
                    string id = EncodeConvesationId(articyId, field.title);
                    if (id == null) continue;

                    //save conversation metadata
                    conversation.metadata[id] = field.value;
                }

                //obtain dialogue entry list
                foreach (var entry in convo.dialogueEntries)
                {
                    //obtain translatable fields
                    foreach (var field in entry.fields)
                    {
                        //skip empty entries
                        if (String.IsNullOrWhiteSpace(field.value))
                            continue;
                        
                        //skip irrelevant fields
                        string articyId = Field.LookupValue(entry.fields, "Articy Id");
                        string id = EncodeDialogueId(articyId, field.title);
                        if (id == null) continue;

                        //construct entry structure
                        DT2.DialogueEntry dialogueEntry = new DT2.DialogueEntry();
                        dialogueEntry.id = id;
                        dialogueEntry.text = field.value;
                        dialogueEntry.actor = databases[0].GetActor(entry.ActorID).Name;

                        conversation.entries.Add(dialogueEntry);
                    }
                }

                data.conversations.Add(conversation);
            }

            //write json to database file
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path.Combine(path, "database.json"), json);
        }

        //easily readable entity ids
        public static string EncodeDialogueId(string entryId, string field)
        {
            if (field.StartsWith("Alternate")) return entryId + "/alt" + field[field.Length - 1];
            if (field.StartsWith("tooltip")) return entryId + "/tip" + field[field.Length - 1];
            if (field == "Dialogue Text") return entryId + "/text";
            return null;
        }
        public static string EncodeConvesationId(string convoId, string field)
        {
            if (field == "Title") return convoId + "/titl";
            if (field == "Description") return convoId + "/desc";
            if (field.StartsWith("subtask_title_")) return convoId + "/sub" + field[field.Length - 1];
            return null;
        }
    }
}
