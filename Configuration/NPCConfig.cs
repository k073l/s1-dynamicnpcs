using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using S1API.Map;
using UnityEngine;

namespace DynamicNPCs.Configuration;

public class NPCConfig
{
    public string ClassName { get; set; } = "";
    public string Id { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public bool IsPhysical { get; set; }
    public float[] Spawn { get; set; } = new float[3];
    public float Aggressiveness { get; set; } = 0f;
    public Region Region { get; set; } = Region.Northtown;
    
    public CustomerConfig CustomerConfig { get; set; } = new();
    public AppearanceConfig Appearance { get; set; } = new();
    public List<ScheduleConfig> Schedules { get; set; } = new();
}

public class AppearanceConfig
{
    public Dictionary<string, object> Fields { get; set; } = new();
    public Dictionary<string, object> BodyLayers { get; set; } = new();
    public Dictionary<string, object> FaceLayers { get; set; } = new();
    public Dictionary<string, object> AccessoryLayers { get; set; } = new();
}

public class CustomerConfig
{
    public bool IsCustomer { get; set; } = true;
    public (float, float) Spending { get; set; } = (200f, 800f);
    public (int, int) OrdersPerWeek { get; set; } = (2, 5);
    public string PreferredOrderDay { get; set; } = "Monday";
    public int OrderTime { get; set; } = 1800;
    public string Standards { get; set; } = "Low";
    public bool AllowDirectApproach { get; set; } = true;
    public bool GuaranteeFirstSample { get; set; } = false;
    public (float, float) MutualRelationRequirement { get; set; } = (2.5f, 4.0f);
    public float CallPoliceChance { get; set; } = 0.1f;
    public (float, float) Dependence { get; set; } = (0.2f, 1.8f);
    public List<(string, float)> Affinities { get; set; } = new() { ("Weed", 0.3f) };
    public List<string> PreferredProperties { get; set; } = new() { "Athletic", "Sneaky" };
}

public class ScheduleConfig
{
    public string Type { get; set; } = ""; // "UseVendingMachine" | "WalkTo" | "StayInBuilding"
    public int StartTime { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string? BuildingName { get; set; }
    public int DurationMinutes { get; set; }
}