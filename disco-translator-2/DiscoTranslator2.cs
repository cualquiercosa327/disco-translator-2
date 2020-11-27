using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using HarmonyLib;

namespace DiscoTranslator2
{
    [BepInPlugin("pl.mssnt.DiscoTranslator2", "Disco Translator 2", "1.0.0.0")]
    [BepInProcess("disco.exe")]
    class DiscoTranslator2 : BaseUnityPlugin
    {
        public DiscoTranslator2()
        {

        }

        public void Awake()
        {
            //instantiate Harmony and patch over localization methods
            Harmony harmony = new Harmony("pl.mssnt.DiscoTranslator2");
            harmony.PatchAll();
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

            return false;
        }
    }
}
