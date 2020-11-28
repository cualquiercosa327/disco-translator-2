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

        public DiscoTranslator2()
        {
            //bind configuration
            string pluginDir = Path.Combine(Paths.PluginPath, "DiscoTranslator2");
            Config.Bind("Translation", "Database path", pluginDir,
                "Where the database file is generated for other translation tools to use");
            Config.Bind("Translation", "Translation path", pluginDir,
                "Where the translation files (.transl) are located");
            Directory.CreateDirectory(pluginDir);

            //instantiate Harmony and patch over localization methods
            Harmony harmony = new Harmony("pl.mssnt.DiscoTranslator2");
            harmony.PatchAll();

            PluginLogger = Logger;
            PluginConfig = Config;
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
                __result = I2.Loc.LocalizationManager.GetTranslation(Term, true, 0, true, false, null, "English");

            return false;
        }
    }
}
