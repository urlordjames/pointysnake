using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace makebin
{
    class Program
    {
        static void Main(string[] args)
        {
            String[] file = File.ReadAllLines("precomp.psnbin");
            var mod = new ModuleDefUser("PointySnakeModule");
            mod.Kind = ModuleKind.Console;
            var asm = new AssemblyDefUser("PointySnakeAssembly");
            asm.Modules.Add(mod);
            var startUpType = new TypeDefUser("PointySnake", "Program", mod.CorLibTypes.Object.TypeDefOrRef);
            mod.Types.Add(startUpType);
            var entryPoint = new MethodDefUser("Main", MethodSig.CreateStatic(mod.CorLibTypes.Int32, new SZArraySig(mod.CorLibTypes.String)));
            entryPoint.Attributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            entryPoint.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            entryPoint.ParamDefs.Add(new ParamDefUser("args", 1));
            var epBody = new CilBody();
            entryPoint.Body = epBody;
            startUpType.Methods.Add(entryPoint);
            mod.EntryPoint = entryPoint;
            foreach (String line in file) {
                WriteInstruction(line, mod, entryPoint.Body);
            }
            entryPoint.Body.Instructions.Add(OpCodes.Endfinally.ToInstruction());
            entryPoint.Body.OptimizeBranches();
            entryPoint.Body.OptimizeMacros();
            Console.WriteLine("done");
            mod.Write("assembly.exe");
            #if DEBUG
            Console.WriteLine("press any key to close...");
            Console.ReadKey();
            #endif
        }

        public static void WriteInstruction(String line, ModuleDefUser mod, CilBody epBody)
        {
            String[] splitline = line.Split(new String[] {", "}, StringSplitOptions.None);
            if (splitline[0] == "ldstr")
            {
                epBody.Instructions.Add(OpCodes.Ldstr.ToInstruction(splitline[1]));
                return;
            }
            if (splitline[0] == "call")
            {
                if (splitline[1] == "print") {
                    var consoleRef = new TypeRefUser(mod, "System", "Console", mod.CorLibTypes.AssemblyRef);
                    var consoleWrite1 = new MemberRefUser(mod, "WriteLine", MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.String), consoleRef);
                    epBody.Instructions.Add(OpCodes.Call.ToInstruction(consoleWrite1));
                }
                return;
            }
            Console.WriteLine(line);
        }
    }
}
