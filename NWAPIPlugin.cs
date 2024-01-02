using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using SCPSLBot.AI;
using SCPSLBot.Navigation;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Reflection;

namespace SCPSLBot
{
    public class NWAPIPlugin
    {
        public static NWAPIPlugin Instance;

        public static Harmony HarmonyInstance;

        [PluginConfig()]
        public Config Config;

        [PluginEntryPoint("SCPSLBot", "1.0.0", "AI players addon.", "repkins(19)")]
        public void OnLoad()
        {
            Instance = this;
            Log.Info("Loaded plugin.");

            HarmonyInstance = new Harmony($"SCPSLBot.100.{DateTime.Now.Ticks}");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Log.Info("Patching successful.");

            NavigationSystem.Instance.BaseDir = PluginHandler.Get(this).PluginDirectoryPath;

            NavigationMesh.Instance.Init();
            NavigationSystem.Instance.Init();
            NavigationMeshEditor.Instance.Init();

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
