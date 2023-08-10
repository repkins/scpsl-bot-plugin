using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System;
using System.Reflection;
using TestPlugin.SLBot;

namespace TestPlugin
{
    public class NWAPIPlugin
    {
        public static NWAPIPlugin Instance;

        public static Harmony HarmonyInstance;

        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("TestPlugin", "1.0.0", "NWAPI plugin for testing purposes.", "repkins(19)")]
        public void OnLoad()
        {
            Instance = this;
            Log.Info("Loaded plugin.");

            HarmonyInstance = new Harmony($"TestPlugin.100.{DateTime.Now.Ticks}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Log.Info("Patching successful.");

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
