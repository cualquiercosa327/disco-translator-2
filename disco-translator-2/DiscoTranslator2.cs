using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;

namespace DiscoTranslator2
{
    [BepInPlugin("pl.mssnt.DiscoTranslator2", "Disco Translator 2", "0.1.0.0")]
    [BepInProcess("disco.exe")]
    class DiscoTranslator2 : BaseUnityPlugin
    {
        public DiscoTranslator2()
        {
            Logger.LogInfo("Hello world");
        }
    }
}
