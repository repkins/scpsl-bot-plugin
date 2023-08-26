using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using SCPSLBot.AI;
using System;
using System.Reflection;

namespace SCPSLBot
{
    public class NWAPIPlugin
    {
        public static NWAPIPlugin Instance;

        public static Harmony HarmonyInstance;

        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("SCPSLBot", "1.0.0", "AI players addon.", "repkins(19)")]
        public void OnLoad()
        {
            Instance = this;
            Log.Info("Loaded plugin.");

            HarmonyInstance = new Harmony($"SCPSLBot.100.{DateTime.Now.Ticks}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Log.Info("Patching successful.");

            NavigationManager.Instance.Init();
            BotManager.Instance.Init();
        }

        [PluginUnload]
        public void OnUnload()
        {
            HarmonyInstance.UnpatchAll();
            HarmonyInstance = null;
            Instance = null;
        }
    }
}
