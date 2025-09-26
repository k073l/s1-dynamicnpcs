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

    public RelationshipConfig Relationship { get; set; } = new();
    public CustomerConfig Customer { get; set; } = new();
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

public class RelationshipConfig
{
    public float Delta { get; set; } = 1.5f;
    public bool IsUnlocked { get; set; } = false;
    public string UnlockType { get; set; } = "DirectApproach";
    public List<string> Connections { get; set; } = new();
}

public class CustomerConfig
{
    public bool IsCustomer { get; set; } = false;
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
    public List<(string, float)> Affinities { get; set; } = new();
    public List<string> PreferredProperties { get; set; } = new();
}

public class ScheduleConfig
{
    public string Type { get; set; } = ""; // "UseVendingMachine" | "WalkTo" | "StayInBuilding" | "LocationDialogue" | "DriveToCarPark" | "EnsureDealSignal"
    public string Name { get; set; } // Optional name for the schedule entry
    public int StartTime { get; set; }
    
    // WalkTo & LocationDialogue
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public bool FaceDesinationDirection { get; set; }
    public float Within { get; set; }
    public bool WarpIfSkipped { get; set; }
    
    // UseVendingMachine
    public string MachineGUID { get; set; }
    
    // StayInBuilding
    public string BuildingName { get; set; }
    public int DurationMinutes { get; set; }
    public int DoorIndex { get; set; }
    
    // LocationDialogue
    public int GreetingOverrideToEnable { get; set; }
    public int ChoiceToEnable { get; set; }
    
    // DriveToCarPark
    public string ParkingLotGUID { get; set; }
    public string VehicleGUID { get; set; }
    public bool? OverrideParkingType { get; set; }
    public int? ParkingType { get; set; }
    public string Alignment { get; set; }
}