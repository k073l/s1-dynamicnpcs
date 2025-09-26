using System.Reflection;
using System.Reflection.Emit;
using DynamicNPCs.Configuration;
using DynamicNPCs.Helpers;
using S1API.Entities;

namespace DynamicNPCs.Generation;

public static class NPCGenerator
{
    private static readonly AssemblyBuilder asmBuilder =
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicNPCs"), AssemblyBuilderAccess.Run);
    private static readonly ModuleBuilder moduleBuilder =
        asmBuilder.DefineDynamicModule("MainModule");

    public static Type CreateNPCSubclass(NPCConfig cfg)
    {
        var tb = moduleBuilder.DefineType(
            cfg.ClassName,
            TypeAttributes.Public | TypeAttributes.Sealed,
            typeof(S1API.Entities.NPC)
        );
        
        // IsPhysical property override
        var isPhysicalGet = tb.DefineMethod("get_IsPhysical",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            typeof(bool),
            Type.EmptyTypes);
        var ilPhys = isPhysicalGet.GetILGenerator();
        ilPhys.Emit(cfg.IsPhysical ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        ilPhys.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(isPhysicalGet, typeof(S1API.Entities.NPC)
            .GetProperty("IsPhysical", BindingFlags.NonPublic | BindingFlags.Instance)!.GetMethod!);
        
        // ctor
        var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
        var ilCtor = ctor.GetILGenerator();
        ilCtor.Emit(OpCodes.Ldarg_0);
        ilCtor.Emit(OpCodes.Ldstr, cfg.Id);
        ilCtor.Emit(OpCodes.Ldstr, cfg.FirstName);
        ilCtor.Emit(OpCodes.Ldstr, cfg.LastName);
        ilCtor.Emit(OpCodes.Ldnull); // icon
        
        var baseCtor = typeof(S1API.Entities.NPC).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(string), typeof(string), typeof(string), typeof(UnityEngine.Sprite) },
            null
        );

        if (baseCtor == null)
            throw new InvalidOperationException("Could not find NPC(string, string, string, Sprite) constructor");

        ilCtor.Emit(OpCodes.Call, baseCtor);
        ilCtor.Emit(OpCodes.Ret);
        
        // OnCreated method override
        var onCreatedMethod = tb.DefineMethod(
            "OnCreated",
            MethodAttributes.Family | MethodAttributes.Virtual,
            typeof(void),
            Type.EmptyTypes
        );
        
        var ilCreated = onCreatedMethod.GetILGenerator();
        
        // try-catch wrap
        var tryLabel = ilCreated.BeginExceptionBlock();
        
        // base OnCreated
        var baseOnCreated = typeof(S1API.Entities.NPC).GetMethod("OnCreated", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (baseOnCreated != null)
        {
            ilCreated.Emit(OpCodes.Ldarg_0);
            ilCreated.Emit(OpCodes.Call, baseOnCreated);
        }
        
        // call helper: ApplyOnCreated(this, NPCConfigRegistry.Get(this.GetType()))
        var getConfigMethod = typeof(NPCConfigRegistry).GetMethod(nameof(NPCConfigRegistry.Get));
        var applyMethod = typeof(CustomNPCHelpers).GetMethod(nameof(CustomNPCHelpers.ApplyOnCreated));
        var getTypeMethod = typeof(object).GetMethod("GetType");
        
        if (getConfigMethod != null && applyMethod != null && getTypeMethod != null)
        {
            ilCreated.Emit(OpCodes.Ldarg_0); // self
            ilCreated.Emit(OpCodes.Ldarg_0); // self (for GetType)
            ilCreated.Emit(OpCodes.Callvirt, getTypeMethod); // self.GetType()
            ilCreated.Emit(OpCodes.Call, getConfigMethod); // NPCConfigRegistry.Get(type)
            ilCreated.Emit(OpCodes.Call, applyMethod); // ApplyOnCreated(self, cfg)
        }
        
        // in case we crash (likely)
        ilCreated.BeginCatchBlock(typeof(Exception));
        ilCreated.Emit(OpCodes.Dup);
        ilCreated.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString")!);
        ilCreated.Emit(OpCodes.Ldstr, "[DynamicNPC] OnCreated exception: ");
        ilCreated.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        ilCreated.Emit(OpCodes.Call, typeof(MelonLoader.MelonLogger).GetMethod("Error", new[] { typeof(string) })!);
        ilCreated.Emit(OpCodes.Pop);
        ilCreated.EndExceptionBlock();
        
        ilCreated.Emit(OpCodes.Ret);
        
        if (baseOnCreated != null)
        {
            tb.DefineMethodOverride(onCreatedMethod, baseOnCreated);
        }
        
        // ConfigurePrefab method override
        var cfgMethod = tb.DefineMethod("ConfigurePrefab",
            MethodAttributes.Family | MethodAttributes.Virtual,
            typeof(void),
            new[] { typeof(NPCPrefabBuilder) });
        var ilCfg = cfgMethod.GetILGenerator();
        
        // try-catch wrap
        var cfgTryLabel = ilCfg.BeginExceptionBlock();
        
        ilCfg.Emit(OpCodes.Ldarg_0);
        ilCfg.Emit(OpCodes.Ldarg_1);
        ilCfg.Emit(OpCodes.Ldtoken, tb);
        ilCfg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")!);
        ilCfg.Emit(OpCodes.Call, typeof(NPCConfigRegistry).GetMethod(nameof(NPCConfigRegistry.Get))!);
        ilCfg.Emit(OpCodes.Call, typeof(CustomNPCHelpers).GetMethod(nameof(CustomNPCHelpers.ConfigureFromJson))!);
        
        // ConfigurePrefab might throw too
        ilCfg.BeginCatchBlock(typeof(Exception));
        ilCfg.Emit(OpCodes.Dup);
        ilCfg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString")!);
        ilCfg.Emit(OpCodes.Ldstr, "[DynamicNPC] ConfigurePrefab exception: ");
        ilCfg.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        ilCfg.Emit(OpCodes.Call, typeof(MelonLoader.MelonLogger).GetMethod("Error", new[] { typeof(string) })!);
        ilCfg.Emit(OpCodes.Pop);
        ilCfg.EndExceptionBlock();
        
        ilCfg.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(cfgMethod, typeof(S1API.Entities.NPC)
            .GetMethod("ConfigurePrefab", BindingFlags.NonPublic | BindingFlags.Instance)!);

        var npcType = tb.CreateType()!;
        NPCConfigRegistry.Register(npcType, cfg);
        return npcType;
    }
}