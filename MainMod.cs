using System;
using System.Collections.Generic;
using System.IO;
using DynamicNPCs.Configuration;
using DynamicNPCs.Generation;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(DynamicNPCs.DynamicNPCS), DynamicNPCs.BuildInfo.Name,
    DynamicNPCs.BuildInfo.Version, DynamicNPCs.BuildInfo.Author)]
[assembly: MelonColor(1, 255, 0, 0)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace DynamicNPCs;

public static class BuildInfo
{
    public const string Name = "DynamicNPCS";
    public const string Description = "Dynamic NPC generation from JSON configuration";
    public const string Author = "me";
    public const string Version = "1.0.0";
}

public class DynamicNPCS : MelonMod
{
    private static MelonLogger.Instance Logger;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("DynamicNPCS initialized");

        try
        {
            LoadNPCConfigurations();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load NPC configurations: {ex.Message}");
        }
    }

    private void LoadNPCConfigurations()
    {
        string json = File.ReadAllText(Path.Combine(MelonEnvironment.UserDataDirectory, "npcs.json"));
        if (string.IsNullOrWhiteSpace(json))
        {
            Logger.Error("NPC configuration JSON is empty");
            return;
        }

        var serializer = new JsonSerializer();
        var reader = new JsonTextReader(new StringReader(json));
        var configs = serializer.Deserialize<List<NPCConfig>>(reader);

        foreach (var cfg in configs)
        {
            var npcType = NPCGenerator.CreateNPCSubclass(cfg);
            Logger.Msg($"Created NPC: {cfg.ClassName}");
        }
    }
}