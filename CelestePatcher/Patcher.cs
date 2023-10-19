using Mono.Cecil;
using Mono.Cecil.Cil;
using Serilog.Core;

namespace CelestePatcher;

public class Patcher
{
    private readonly AssemblyDefinition _assembly;
    private readonly Logger _log;

    public Patcher(string path, Logger log)
    {
        _assembly = AssemblyDefinition.ReadAssembly(path);
        _log = log;
    }

    public bool IsCeleste()
    {
        return _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == "Celeste.Celeste") != null;
    }

    public bool NeedsPatching()
    {
        return IsCeleste() && _assembly.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == "Steamworks.NET") !=
            null;
    }

    private void PatchCelesteClass()
    {
        _log.Verbose("Patching class Celeste.Celeste");
        var celesteClass = _assembly.MainModule.Types.First(t => t.FullName == "Celeste.Celeste");

        _log.Verbose("Patching method Celeste::.cctor");
        {
            var method = celesteClass.Methods.First(m => m.Name == ".cctor");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            var index = instructions.IndexOf(instructions.First(i =>
                i.OpCode == OpCodes.Ldc_I4 && (int)i.Operand == 504230));

            for (int i = 0; i < 3; i++) processor.RemoveAt(index);
        }

        _log.Verbose("Patching method Celeste::Update");
        {
            var method = celesteClass.Methods.First(m => m.Name == "Update");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            // Remove RunCallbacks() call
            processor.Remove(instructions.First(
                i => i.OpCode == OpCodes.Call && i.Operand is MethodReference methodRef &&
                     methodRef.DeclaringType.FullName == "Steamworks.SteamAPI" &&
                     methodRef.Name == "RunCallbacks")
            );
        }

        _log.Verbose("Patching method Celeste::Main");
        {
            var method = celesteClass.Methods.First(m => m.Name == "Main");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            var steamIdField = celesteClass.Fields.First(f => f.Name == "SteamID");

            var index = instructions.IndexOf(instructions.First(i =>
                i.OpCode == OpCodes.Ldsfld &&
                i.Operand == celesteClass.Fields.FirstOrDefault(f => f.Name == "SteamID")));

            for (int i = 0; i < 15; i++) processor.RemoveAt(index);

            celesteClass.Fields.Remove(steamIdField);
        }
    }

    private void PatchStatsClass()
    {
        _log.Verbose("Patching class Celeste.Stats");
        var statsClass = _assembly.MainModule.Types.First(t => t.FullName == "Celeste.Stats");


        _log.Verbose("Patching method Stats::MakeRequest");
        {
            var method = statsClass.Methods.First(m => m.Name == "MakeRequest");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ret));
        }

        _log.Verbose("Patching method Stats::Increment");
        {
            var method = statsClass.Methods.First(m => m.Name == "Increment");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ret));
        }

        _log.Verbose("Patching method Stats::Local");
        {
            var method = statsClass.Methods.First(m => m.Name == "Local");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ret));
        }

        _log.Verbose("Patching method Stats::Global");
        {
            var method = statsClass.Methods.First(m => m.Name == "Global");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ret));
        }

        _log.Verbose("Patching method Stats::Store");
        {
            var method = statsClass.Methods.First(m => m.Name == "Store");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ret));
        }
    }

    private void PatchAchievementsClass()
    {
        _log.Verbose("Patching class Celeste.Achievements");
        var achievementsClass = _assembly.MainModule.Types.First(t => t.FullName == "Celeste.Achievements");

        _log.Verbose("Patching method Achievements::Has");
        {
            var method = achievementsClass.Methods.First(m => m.Name == "Has");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ret));
        }

        _log.Verbose("Patching method Achievements::Register");
        {
            var method = achievementsClass.Methods.First(m => m.Name == "Register");
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            int count = instructions.Count;
            for (int i = 0; i < count; i++) processor.RemoveAt(0);

            processor.Append(Instruction.Create(OpCodes.Ret));
        }
    }

    public void Patch()
    {
        _log.Information("Removing Steamworks.NET AssemblyRef");
        var steamworksNetReference = _assembly.MainModule.AssemblyReferences
            .FirstOrDefault(r => r.Name == "Steamworks.NET");
        if (steamworksNetReference != null) _assembly.MainModule.AssemblyReferences.Remove(steamworksNetReference);
        else _log.Warning("Steamworks.NET AssemblyRef not found, may not need patching");

        _log.Information("Patching classes");

        PatchCelesteClass();
        PatchStatsClass();
        PatchAchievementsClass();

        _log.Information("Done");
    }

    public void Save(string path)
    {
        _assembly.Write(path);
    }
}