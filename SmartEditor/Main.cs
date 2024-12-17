using System.Reflection;
using System.Reflection.Emit;
using JALib.Core;
using JALib.Core.Patch;
using SmartEditor.LevelEvent;
using UnityModManagerNet;

namespace SmartEditor;

public class Main : JAMod {
    public static Main Instance;
    private static ModuleBuilder _moduleBuilder;
    private static AssemblyBuilder assemblyBuilder;
    public static ModuleBuilder ModuleBuilder {
        get {
            if(_moduleBuilder == null) {
                assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SmartEditor.Custom"), AssemblyBuilderAccess.Run);
                _moduleBuilder = assemblyBuilder.DefineDynamicModule("SmartEditor.Custom");
            }
            return _moduleBuilder;
        }
    }

    public Main(UnityModManager.ModEntry modEntry) : base(modEntry, false) {
        Patcher.AddAllPatch(PatchBinding.AllPatch);
        GenerateEventType.Generate();
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}