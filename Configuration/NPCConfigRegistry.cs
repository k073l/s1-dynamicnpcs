using System;
using System.Collections.Generic;

namespace DynamicNPCs.Configuration;

public static class NPCConfigRegistry
{
    private static readonly Dictionary<Type, NPCConfig> ConfigMap = new();

    public static void Register(Type npcType, NPCConfig config) => ConfigMap[npcType] = config;
    public static NPCConfig Get(Type npcType) => ConfigMap[npcType];
}