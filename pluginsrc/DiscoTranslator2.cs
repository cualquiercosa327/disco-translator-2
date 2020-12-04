using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DiscoTranslator2
{
    [BepInPlugin("pl.mssnt.DiscoTranslator2", "Disco Translator 2", "1.0.0.0")]
    [BepInProcess("disco.exe")]
    class DiscoTranslator2 : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;
        public static ConfigFile PluginConfig;

        static FileSystemWatcher fileWatcher;

        public DiscoTranslator2()
        {
            //bind configuration
            string pluginDir = Path.Combine(Paths.PluginPath, "DiscoTranslator2");
            Config.Bind("Translation", "Database path", pluginDir,
                "Where the database file is generated for other translation tools to use");
            string transPath = Config.Bind("Translation", "Translation path", pluginDir,
                "Where the translation files (.transl) are located").Value;
            Directory.CreateDirectory(pluginDir);

            //instantiate Harmony and patch over localization methods
            Harmony harmony = new Harmony("pl.mssnt.DiscoTranslator2");
            harmony.PatchAll();

            PluginLogger = Logger;
            PluginConfig = Config;

            //watch for file changes
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = transPath;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Filter = "*.transl";
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.Changed += OnTranslationChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        public void Awake()
        {
            //load translations from translation directory
            TranslationRepository.LoadTranslations();
        }
        public void Update()
        {
            //extract resources as soon as they become available
            if (!ResourceExtractor.Extracted)
                ResourceExtractor.Extract();
        }

        void OnTranslationChanged(object sender, FileSystemEventArgs e)
        {
            //reload translations
            Logger.LogMessage("Detected change in " + Path.GetFileName(e.FullPath));
            TranslationRepository.LoadTranslations();
        }
    }

    [HarmonyPatch(typeof(I2.Loc.LocalizationManager), "GetTranslation")]
    class TranslationPatch
    {
        //patch over the GetTranslation method of I2 Localization
        public static bool Prefix(string Term, string overrideLanguage, ref string __result)
        {
            //abort translation if there is no term or the override language is english
            if (Term == null) return true;
            if (overrideLanguage != null && overrideLanguage == "English") return true;

            //look up translation in dictionary, default to English if there is none
            if (!TranslationRepository.Resolve(Term, out __result))
            {
                __result = I2.Loc.LocalizationManager.GetTranslation(Term, true, 0, true, false, null, "English");

                //show missing translations, skip non-translations
                if (string.IsNullOrWhiteSpace(__result)) return false;
                if (__result == Term) return false;
                DiscoTranslator2.PluginLogger.LogInfo("Unknown term " + Term + ": " + __result);
            }

            return false;
        }
    }
}
