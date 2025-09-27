$npcCount = 50
$outputDir = "C:\Games\Schedule I\S1-files\Schedule I Mono\UserData\DynamicNPCS"
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir | Out-Null }
$rand = New-Object System.Random

function Get-HexColor { param([System.Random]$r) "#{0:X2}{1:X2}{2:X2}" -f $r.Next(30,256), $r.Next(30,256), $r.Next(30,256) }

$baseNpc = @{
    ClassName    = "CustomNPC"
    Id           = "npc_custom"
    FirstName    = "NPC"
    LastName     = "Test"
    IsPhysical   = $true
    Spawn        = @(0, 4, 0)
    Aggressiveness = 5.0
    Region       = "Downtown"
    Appearance   = @{
        Fields = @{
            Height                = 0.9
            Weight                = 0.5
            SkinColor             = "#8B7355"
            EyeBallTint           = "#FFFF99"
            PupilDilation         = 0.8
            EyebrowScale          = 1.2
            EyebrowThickness      = 0.8
            EyebrowRestingHeight  = -0.1
            EyebrowRestingAngle   = -0.15
            EyeLidRestingStateLeft  = @(0.3, 0.4)
            EyeLidRestingStateRight = @(0.3, 0.4)
            HairStyle             = "Avatar/Hair/Spiky/Spiky"
            HairColor             = "#2D4A22"
        }
        BodyLayers = @{
            Shirts = @("Avatar/Layers/Top/T-Shirt", "#8B4513")
            Pants  = @("Avatar/Layers/Bottom/Jeans", "#654321")
        }
        FaceLayers = @{
            Face = @("Avatar/Layers/Face/Face_Agitated", "#FF6B6B")
        }
        AccessoryLayers = @{}
    }
    Relationship = @{
        Delta = 1.5
        IsUnlocked = $true
        UnlockType = "DirectApproach"
        Connections = @("kyle_cooley")
    }
    Customer = @{
        IsCustomer = $true
        Spending = @(150.0, 500.0)
        OrdersPerWeek = @(1, 3)
        PreferredOrderDay = "Sunday"
        OrderTime = 1300
        Standards = "Medium"
        AllowDirectApproach = $true
        GuaranteeFirstSample = $false
        MutualRelationRequirement = @(1.0, 3.5)
        CallPoliceChance = 0.2
        Dependence = @(0.1, 1.2)
        Affinities = ,@(@("Marijuana", 0.2))
        PreferredProperties = @("Athletic")
    }
    Schedules = @(
        @{ Type = "UseVendingMachine"; StartTime = 900 },
        @{ Type = "WalkTo"; X = -28.06; Y = 1.065; Z = 62.07; StartTime = 925 }
    )
}

for ($i = 1; $i -le $npcCount; $i++) {
    $index = [int]$i
    
    $npc = $baseNpc.Clone()
    $npc.ClassName = "CustomNPC$index"
    $npc.Id = "npc_custom_{0:D3}" -f $index
    $npc.FirstName = "NPC_$index"
    $npc.Spawn = @([int](($index % 10) * 2), 4, [int]([math]::Floor($index / 10) * 2))
    $npc.Appearance.Fields.Height = [math]::Round(($rand.NextDouble() * 0.6) + 0.7, 2)
    $npc.Appearance.Fields.Weight = [math]::Round(($rand.NextDouble() * 0.8) + 0.3, 2)
    $npc.Appearance.Fields.SkinColor = Get-HexColor $rand
    $npc.Appearance.Fields.HairColor = Get-HexColor $rand
    $npc.Appearance.BodyLayers.Shirts[1] = Get-HexColor $rand
    $npc.Appearance.BodyLayers.Pants[1]  = Get-HexColor $rand
    $npc.Appearance.FaceLayers.Face[1]   = Get-HexColor $rand
    $fileName = Join-Path $outputDir ("npc_custom_{0:D3}.json" -f $index)
    $npc | ConvertTo-Json -Depth 10 | Out-File $fileName -Encoding utf8
}

