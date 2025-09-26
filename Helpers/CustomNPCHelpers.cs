using System.Runtime.CompilerServices;
using DynamicNPCs.Configuration;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using S1API.Entities;
using S1API.Entities.Schedule;
using S1API.Vehicles;
using UnityEngine;

namespace DynamicNPCs.Helpers;

public static class CustomNPCHelpers
{
    public static void ConfigureFromJson(S1API.Entities.NPC self, NPCPrefabBuilder builder, NPCConfig cfg)
    {
        var spawn = new Vector3(cfg.Spawn[0], cfg.Spawn[1], cfg.Spawn[2]);
        builder.WithSpawnPosition(spawn);
        if (cfg.Customer.IsCustomer)
        {
            // debug
            MelonLogger.Msg($"Configuring {cfg.Id} as customer");
            var dump = JsonConvert.SerializeObject(cfg.Customer, Formatting.Indented);
            MelonLogger.Msg(dump);
            // end debug
            var cCfg = cfg.Customer;
            try
            {
                builder.EnsureCustomer()
                    .WithCustomerDefaults(cd =>
                    {
                        cd.WithSpending(cCfg.Spending.Item1, cCfg.Spending.Item2)
                            .WithOrdersPerWeek(cCfg.OrdersPerWeek.Item1, cCfg.OrdersPerWeek.Item2)
                            .WithPreferredOrderDay(cCfg.PreferredOrderDay)
                            .WithOrderTime(cCfg.OrderTime)
                            .WithStandards(cCfg.Standards)
                            .AllowDirectApproach(cCfg.AllowDirectApproach)
                            .GuaranteeFirstSample(cCfg.GuaranteeFirstSample)
                            .WithMutualRelationRequirement(cCfg.MutualRelationRequirement.Item1,
                                cCfg.MutualRelationRequirement.Item2)
                            .WithCallPoliceChance(cCfg.CallPoliceChance)
                            .WithDependence(cCfg.Dependence.Item1, cCfg.Dependence.Item2)
                            .WithAffinities(cCfg.Affinities)
                            .WithPreferredPropertiesByName(cCfg.PreferredProperties.ToArray());
                    });
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error in customer config for {cfg.Id}: {e}");
            }
        }

        builder.WithRelationshipDefaults(r =>
        {
            var rCfg = cfg.Relationship;
            r.WithDelta(rCfg.Delta)
                .SetUnlocked(rCfg.IsUnlocked)
                .SetUnlockType(rCfg.UnlockType)
                .WithConnectionsById(rCfg.Connections.ToArray());
        });

        builder.WithSchedule(plan =>
        {
            foreach (var sch in cfg.Schedules)
            {
                switch (sch.Type)
                {
                    case "UseVendingMachine":
                        plan.Add(new UseVendingMachineSpec
                        {
                            StartTime = sch.StartTime,
                            MachineGUID = string.IsNullOrWhiteSpace(sch.MachineGUID) ? null : sch.MachineGUID,
                            Name = sch.Name
                        });
                        break;
                    case "WalkTo":
                        plan.Add(new WalkToSpec
                        {
                            Destination = new Vector3(sch.X, sch.Y, sch.Z),
                            StartTime = sch.StartTime,
                            FaceDestinationDirection = sch.FaceDesinationDirection,
                            Within = sch.Within,
                            WarpIfSkipped = sch.WarpIfSkipped,
                            Name = sch.Name
                        });
                        break;
                    case "StayInBuilding":
                        plan.Add(new StayInBuildingSpec
                        {
                            BuildingName = sch.BuildingName,
                            StartTime = sch.StartTime,
                            DurationMinutes = sch.DurationMinutes,
                            DoorIndex = sch.DoorIndex,
                            Name = sch.Name
                        });
                        break;
                    case "LocationDialogue":
                        plan.Add(new LocationDialogueSpec
                        {
                            Destination = new Vector3(sch.X, sch.Y, sch.Z),
                            StartTime = sch.StartTime,
                            FaceDestinationDirection = sch.FaceDesinationDirection,
                            Within = sch.Within,
                            WarpIfSkipped = sch.WarpIfSkipped,
                            GreetingOverrideToEnable = sch.GreetingOverrideToEnable,
                            ChoiceToEnable = sch.ChoiceToEnable,
                            Name = sch.Name
                        });
                        break;
                    case "DriveToCarPark":
                        plan.Add(new DriveToCarParkSpec
                        {
                            ParkingLotGUID = sch.ParkingLotGUID,
                            VehicleGUID = sch.VehicleGUID,
                            OverrideParkingType = sch.OverrideParkingType,
                            ParkingType = sch.ParkingType,
                            Alignment = Enum.Parse<ParkingAlignment>(sch.Alignment),
                            StartTime = sch.StartTime,
                            Name = sch.Name
                        });
                        break;
                    case "EnsureDealSignal":
                        plan.EnsureDealSignal();
                        break;
                }
            }
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ApplyOnCreated(S1API.Entities.NPC self, NPCConfig cfg)
    {
        MelonLogger.Msg("[DynamicNPC] OnCreated - applying config");

        self.Aggressiveness = cfg.Aggressiveness;
        self.Region = cfg.Region;

        if (cfg.Appearance != null)
        {
            try
            {
                ApplyAppearanceConfig(self.Appearance, cfg.Appearance);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[DynamicNPC] Failed to apply appearance config: {ex}");
            }
        }

        self.Appearance.Build();

        self.Schedule?.Enable();
        self.Schedule?.InitializeActions();
    }

    public static void ApplyAppearanceConfig(NPCAppearance appearance, AppearanceConfig cfg)
    {
        ApplyFields(appearance, cfg.Fields);
        ApplyLayerDictionary(appearance, cfg.BodyLayers, "BodyLayerFields");
        ApplyLayerDictionary(appearance, cfg.FaceLayers, "FaceLayerFields");
        ApplyLayerDictionary(appearance, cfg.AccessoryLayers, "AccessoryLayerFields");
    }

    private static void ApplyFields(NPCAppearance appearance, Dictionary<string, object> fields)
    {
        var s1Assembly = typeof(NPCAppearance).Assembly;

        foreach (var kv in fields)
        {
            var fieldType = s1Assembly.GetType($"S1API.Entities.Appearances.CustomizationFields.{kv.Key}");
            if (fieldType == null)
            {
                var availableTypes = s1Assembly.GetTypes()
                    .Where(t => t.Namespace == "S1API.Entities.Appearances.CustomizationFields")
                    .Select(t => t.Name)
                    .ToArray();

                MelonLogger.Warning($"[DynamicNPC] Unknown appearance field: {kv.Key}");
                MelonLogger.Warning($"[DynamicNPC] Available CustomizationFields: {string.Join(", ", availableTypes)}");
                continue;
            }

            object valueToSet = kv.Value;

            try
            {
                if (kv.Value is JValue jValue)
                {
                    if (fieldType == typeof(float))
                        valueToSet = jValue.ToObject<float>();
                    else if (fieldType == typeof(int))
                        valueToSet = jValue.ToObject<int>();
                    else if (fieldType == typeof(bool))
                        valueToSet = jValue.ToObject<bool>();
                    else if (fieldType == typeof(string))
                        valueToSet = jValue.ToString();
                    else if (fieldType == typeof(Color) || fieldType == typeof(Color32))
                        valueToSet = ParseColor(jValue.ToString());
                    else
                        valueToSet = jValue.ToObject(fieldType);
                }
                else if (kv.Value is JArray jArray && jArray.Count == 2)
                {
                    var x = jArray[0]?.ToObject<float>() ?? 0f;
                    var y = jArray[1]?.ToObject<float>() ?? 0f;

                    // eyelids are weird
                    if (kv.Key is "EyeLidRestingStateLeft" or "EyeLidRestingStateRight")
                    {
                        valueToSet = (x, y);
                    }
                    else if (fieldType == typeof(ValueTuple<float, float>))
                    {
                        valueToSet = (x, y);
                    }
                    else
                    {
                        valueToSet = jArray.ToObject(fieldType);
                    }
                }
                else
                {
                    if (fieldType == typeof(float))
                        valueToSet = Convert.ToSingle(kv.Value);
                    else if (fieldType == typeof(int))
                        valueToSet = Convert.ToInt32(kv.Value);
                    else if (fieldType == typeof(bool))
                        valueToSet = Convert.ToBoolean(kv.Value);
                    else if (fieldType == typeof(Color) || fieldType == typeof(Color32))
                    {
                        if (kv.Value is string hex)
                            valueToSet = ParseColor(hex);
                        else
                        {
                            valueToSet = Color.white;
                            MelonLogger.Warning(
                                $"[DynamicNPC] Expected color string for {kv.Key}, using white fallback.");
                        }
                    }
                    else if (fieldType == typeof(string))
                        valueToSet = kv.Value.ToString();
                }

                var setMethod = appearance.GetType().GetMethod("Set")?.MakeGenericMethod(fieldType);
                if (setMethod != null)
                    setMethod.Invoke(appearance, new[] { valueToSet });
                else
                    MelonLogger.Warning($"[DynamicNPC] Set method not found for {fieldType.Name}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[DynamicNPC] Failed to set field {kv.Key} ({kv.Value}): {ex}");
            }
        }
    }

    public static void ApplyLayerDictionary(NPCAppearance appearance, Dictionary<string, object> layers,
        string layerNamespace)
    {
        var s1Assembly = typeof(NPCAppearance).Assembly;

        foreach (var kv in layers)
        {
            string path;
            Color color;

            try
            {
                switch (kv.Value)
                {
                    case JArray jArray when jArray.Count >= 2:
                        path = jArray[0]?.ToString() ?? "";
                        color = ParseColor(jArray[1]?.ToString() ?? "#ffffff");
                        break;
                    case JArray jArray when jArray.Count == 1:
                        path = jArray[0]?.ToString() ?? "";
                        color = Color.white;
                        break;
                    case ValueTuple<string, string> tuple:
                        path = tuple.Item1;
                        color = ParseColor(tuple.Item2);
                        break;
                    case string s:
                        path = s;
                        color = Color.white;
                        break;
                    default:
                        MelonLogger.Warning(
                            $"[DynamicNPC] Unknown layer value type for {kv.Key}: {kv.Value?.GetType().Name}, skipping.");
                        continue;
                }

                var layerType = s1Assembly.GetType($"S1API.Entities.Appearances.{layerNamespace}.{kv.Key}");
                if (layerType == null)
                {
                    var availableTypes = s1Assembly.GetTypes()
                        .Where(t => t.Namespace == $"S1API.Entities.Appearances.{layerNamespace}")
                        .Select(t => t.Name)
                        .ToArray();

                    MelonLogger.Warning($"[DynamicNPC] Layer type not found: {layerNamespace}.{kv.Key}");
                    MelonLogger.Warning(
                        $"[DynamicNPC] Available {layerNamespace} types: {string.Join(", ", availableTypes)}");
                    continue;
                }

                var methodName = "With" + layerNamespace.Replace("Fields", "");
                var withLayerMethod = appearance.GetType()
                    .GetMethod(methodName, new[] { typeof(string), typeof(Color) })
                    ?.MakeGenericMethod(layerType);

                if (withLayerMethod != null)
                {
                    MelonLogger.Msg($"[DynamicNPC] Setting {layerNamespace}.{kv.Key}: path='{path}', color={color}");
                    withLayerMethod.Invoke(appearance, new object[] { path, color });
                }
                else
                {
                    MelonLogger.Warning($"[DynamicNPC] {methodName} method not found for {layerType.Name}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[DynamicNPC] Failed to apply layer {kv.Key}: {ex}");
            }
        }
    }

    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var c))
            return c;
        MelonLogger.Warning($"[DynamicNPC] Failed to parse color: {hex}, using white");
        return Color.white;
    }
}