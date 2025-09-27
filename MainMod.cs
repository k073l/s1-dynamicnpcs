using System;
using System.Collections.Generic;
using System.IO;
using DynamicNPCs.Configuration;
using DynamicNPCs.Generation;
using DynamicNPCs.Helpers;
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
    public const string Author = "k073l";
    public const string Version = "1.0.0";
}

public class DynamicNPCS : MelonMod
{
    private static MelonLogger.Instance Logger;
    private const string DirectoryName = "DynamicNPCS";

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("DynamicNPCs initialized");

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
        var path = Path.Combine(MelonEnvironment.UserDataDirectory, DirectoryName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Logger.Msg($"Created directory for NPC configurations at {path}. Please add JSON files and restart the game.");
            return;
        }
        var jsons = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        
        var settings = new JsonSerializerSettings
        {
            Converters =
            {
                new TupleConverter<float, float>(),
                new TupleConverter<int, int>(),
                new TupleConverter<string, float>()
            }
        };
        foreach (var json in jsons)
        {
            try
            {
                var jsonContent = File.ReadAllText(json);
                var cfg = JsonConvert.DeserializeObject<NPCConfig>(jsonContent, settings);
                if (cfg == null) throw new Exception($"Failed to load NPC config from {json}");
                var npcType = NPCGenerator.CreateNPCSubclass(cfg);
                Logger.Msg($"Created NPC: {cfg.ClassName}");
            }
            catch (Exception ex)
            {
                Logger.Msg($"Failed loading JSON file {json}: {ex.Message}");
            }
        }
    }
}