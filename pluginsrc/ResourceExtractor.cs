using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using I2.Loc;
using Newtonsoft.Json;
using DT2 = DiscoTranslator2.Datatypes;

namespace DiscoTranslator2
{
    static class ResourceExtractor
    {
        public static bool Extracted = false;

        //misc entry type dictionary
        static readonly Dictionary<string, string> typeDictionary = new Dictionary<string, string>
        {
            {"ANNOTATION", "ui"},
            {"Abilities", "ui"},
            {"Actors", "actors"},
            {"Archetypes", "ui"},
            {"Area Names", "areas"},
            {"Buttons", "ui"},
            {"CHARSHEET", "ui"},
            {"Difficulties", "ui"},
            {"EFFECT", "ui"},
            {"F1_SCREEN", "ui"},
            {"INVENTORY", "ui"},
            {"Items", "items"},
            {"Messages", "messages"},
            {"Newspapers", "newspapers"},
            {"SETTINGS", "settings"},
            {"Skills", "skills"},
            {"TOOLTIP", "tooltips"},
            {"Thoughts", "thoughts"},
            {"Tips", "tips"}
        };
        //id decoding dictionary
        //misc entry type dictionary
        static readonly Dictionary<string, string> propDictionary = new Dictionary<string, string>
        {
            {"title", "@Title"},
            {"desc", "@Description"},
            {"sub", "@subtask_title_0"},
            {"text", "Dialogue Text"},
            {"alt", "Alternate"},
            {"tip", "tooltip"}
        };

        public static void Extract()
        {
            //obtain resource collections
            DialogueDatabase[] dialDbs = Resources.FindObjectsOfTypeAll<DialogueDatabase>();
            LanguageSourceAsset[] langAssets = Resources.FindObjectsOfTypeAll<LanguageSourceAsset>();

            //mark extraction as successful
            if (dialDbs.Length == 0 || langAssets.Length == 0) return;
            Extracted = true;

            //extract resources
            DT2.TranslationDatabase transDatabase = new DT2.TranslationDatabase();
            ExtractConversations(dialDbs[0], ref transDatabase);
            ExtractRemaining(langAssets, ref transDatabase);

            //write obtained translation database to file
            string json = JsonConvert.SerializeObject(transDatabase);
            string path = (string)DiscoTranslator2.PluginConfig["Translation", "Database path"].BoxedValue;
            File.WriteAllText(Path.Combine(path, "database.json"), json);
        }

        static void ExtractConversations(DialogueDatabase database, ref DT2.TranslationDatabase output)
        {            
            //extract English conversations
            foreach (var conversation in database.conversations)
            {
                //create conversation entry and assign helper title
                DT2.Conversation conversationEntry = new DT2.Conversation();
                conversationEntry.title = conversation.Title;

                //get conversation root
                DialogueEntry rootEntry = conversation.GetFirstDialogueEntry();
                ResolveLeads(rootEntry, conversation, ref conversationEntry.roots);

                //detect special conversation types
                if (Field.LookupValue(conversation.fields, "subtask_title_01") != null)
                    conversationEntry.type = "journal";
                else if (Field.LookupValue(conversation.fields, "orbSoundVolume") != null)
                    conversationEntry.type = "orb";
                else if (conversation.Title.ToLower().Contains("barks"))
                    conversationEntry.type = "barks";

                //obtain conversation metadata
                foreach (var field in conversation.fields)
                {
                    //skip empty entries
                    if (String.IsNullOrWhiteSpace(field.value))
                        continue;

                    //skip irrelevant fields
                    string articyId = Field.LookupValue(conversation.fields, "Articy Id");
                    string id = EncodeConvesationId(articyId, field.title);
                    if (id == null) continue;

                    //update conversation metadata
                    conversationEntry.metadata[id] = field.value;
                }

                //obtain conversation dialogue entry list
                foreach (var entry in conversation.dialogueEntries)
                {
                    //create dialogue entry
                    DT2.DialogueEntry dialogueEntry = new DT2.DialogueEntry();
                    string entryId = Field.LookupValue(entry.fields, "Articy Id");

                    //obtain translatable fields
                    foreach (var field in entry.fields)
                    {
                        //skip emprt or irrelevant fields
                        if (string.IsNullOrWhiteSpace(field.value)) continue;
                        string fieldId = EncodeDialogueId(entryId, field.title);
                        if (fieldId == null) continue;

                        dialogueEntry.fields[fieldId] = field.value;
                    }

                    //skip empty entries
                    if (dialogueEntry.fields.Count == 0)
                        continue;

                    //find where the entry leads to
                    dialogueEntry.actor = database.GetActor(entry.ActorID).Name;
                    ResolveLeads(entry, conversation, ref dialogueEntry.leadsTo);
                    conversationEntry.entries[entryId] = dialogueEntry;
                }

                //add conversation to list, skip empty dialogues
                if (conversationEntry.type != "dialogue" || conversationEntry.entries.Count != 0)
                    output.conversations.Add(conversationEntry);
            }
        }
        static void ExtractRemaining(LanguageSourceAsset[] langSource, ref DT2.TranslationDatabase output)
        {
            //find a language source with English strings
            LanguageSourceData englishSource = null;
            int englishIndex = -1;

            //iterate over sources
            foreach (var source in langSource)
            {
                //only search miscellaneous sources
                if (!source.name.Contains("General"))
                    continue;

                //skip if English unavailable
                int index = source.mSource.GetLanguageIndex("English");
                if (index != -1)
                {
                    englishSource = source.mSource;
                    englishIndex = index;
                    break;
                }
            }

            //initialize type-specific dictionary
            foreach (KeyValuePair<string, string> kvp in typeDictionary)
                if (!output.miscellaneous.ContainsKey(kvp.Value))
                    output.miscellaneous.Add(kvp.Value, new Dictionary<string, string>());
            output.miscellaneous.Add("misc", new Dictionary<string, string>());

            //extract remaining terms
            foreach (var term in englishSource.mTerms)
            {
                //skip entries covered by conversations and empty entries
                if (term.Term.StartsWith("Conversation/")) continue;
                if (string.IsNullOrWhiteSpace(term.Languages[englishIndex])) continue;

                //detect special types
                string entryType = "misc";
                foreach (KeyValuePair<string, string> kvp in typeDictionary)
                    if (term.Term.StartsWith(kvp.Key))
                    {
                        entryType = kvp.Value;
                        break;
                    }

                //skip redundant actor entries
                if (entryType == "actors" && char.IsDigit(term.Term, 26))
                    continue;

                //add entry to database
                output.miscellaneous[entryType].Add(term.Term, term.Languages[englishIndex]);
            }
        }

        static void ResolveLeads(DialogueEntry entry, Conversation conversation, ref List<string> leads)
        {
            //don't follow final entries
            if (entry.Sequence != null && entry.Sequence.Contains("FinishDialogue()"))
                return;
            
            //follow forced sequence
            while (entry.Sequence == "Continue()")
            {
                if (entry.outgoingLinks.Count > 0)
                {
                    int contId = entry.outgoingLinks[0].destinationDialogueID;
                    entry = conversation.dialogueEntries[contId];
                }
                else
                    return;
            }

            //analyze each link of an entry
            foreach (Link link in entry.outgoingLinks)
            {
                //skip self-referential leads
                if (link.destinationDialogueID == entry.id) continue;

                //resolve lead entry
                int destinationId = link.destinationDialogueID;
                if (destinationId >= conversation.dialogueEntries.Count) continue;
                DialogueEntry destination = conversation.dialogueEntries[destinationId];

                //resolve group leads and other anomalies
                if (Field.LookupValue(destination.fields, "Dialogue Text") == null)
                {
                    ResolveLeads(destination, conversation, ref leads);
                    continue;
                }

                //add entry lead
                string destinationArticyId = Field.LookupValue(destination.fields, "Articy Id");
                leads.Add(destinationArticyId);
            }
        }

        //easily readable entity ids
        static string EncodeDialogueId(string entryId, string field)
        {
            if (field.StartsWith("Alternate")) return entryId + "/alt" + field[field.Length - 1];
            if (field.StartsWith("tooltip")) return entryId + "/tip" + field[field.Length - 1];
            if (field == "Dialogue Text") return entryId + "/text";
            return null;
        }
        static string EncodeConvesationId(string convoId, string field)
        {
            if (field == "Title") return convoId + "/titl";
            if (field == "Description") return convoId + "/desc";
            if (field.StartsWith("subtask_title_")) return convoId + "/sub" + field[field.Length - 1];
            return null;
        }
        public static string DecodeId(string id)
        {
            //separate entry id from property name
            int slash = id.IndexOf("/");
            if (slash == -1) return null;

            string property = id.Substring(slash + 1);
            string articyId = id.Substring(0, slash);
            if (property.Length != 4) return null;

            //check against dictionary
            string decodedProperty = null;
            foreach (KeyValuePair<string, string> kvp in propDictionary)
            {
                if (property == kvp.Key) decodedProperty = kvp.Value;
                else if (property.StartsWith(kvp.Key)) decodedProperty = kvp.Value + property[3];
            }

            //check failed
            if (decodedProperty == null)
                return null;

            //tell conversations apart from dialogue entries
            bool isConversation = decodedProperty[0] == '@';
            if (isConversation) decodedProperty = decodedProperty.Substring(1);

            //decode id
            if (isConversation) return "Conversation/" + articyId;
            else return decodedProperty + "/" + articyId;
        }
    }
}
