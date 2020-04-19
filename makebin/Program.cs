using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

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
            entryPoint.Body.Instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
            entryPoint.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            entryPoint.Body.OptimizeBranches();
            entryPoint.Body.OptimizeMacros();
            Console.WriteLine("done");
            var options = new ModuleWriterOptions(mod);
            options.PEHeadersOptions.Machine = dnlib.PE.Machine.AMD64;
            mod.Write("executable.exe", options);
#if DEBUG
            Console.WriteLine("press any key to close...");
            Console.ReadKey();
#endif
        }

        public static void WriteInstruction(String line, ModuleDefUser mod, CilBody epBody)
        {
            String[] splitline = line.Split(new String[] { ", " }, StringSplitOptions.None);
            if (splitline[0] == "ldstr")
            {
                ldstr(epBody, splitline[1]);
                return;
            }
            if (splitline[0] == "ldint")
            {
                ldint(epBody, int.Parse(splitline[1]));
                return;
            }
            if (splitline[0] == "call")
            {
                if (splitline[1] == "print")
                {
                    var consoleRef = new TypeRefUser(mod, "System", "Console", mod.CorLibTypes.AssemblyRef);
                    Type type = epBody.Instructions[epBody.Instructions.Count - 1].Operand.GetType();
                    var consoleWrite1 = new MemberRefUser(mod, "WriteLine", MethodSig.CreateStatic(mod.CorLibTypes.Void, gettypesig(mod, type)), consoleRef);
                    epBody.Instructions.Add(OpCodes.Call.ToInstruction(consoleWrite1));
                }
                return;
            }
            if (splitline[0] == "ldvar")
            {
                ldvar(epBody, splitline[1]);
                return;
            }
            if (splitline[0] == "setvar")
            {
                Local local = null;
                if (splitline[2] == "int")
                {
                    ldint(epBody, int.Parse(splitline[3]));
                    local = getvar(mod.CorLibTypes.Int32, splitline[1], epBody.Variables);
                }
                if (splitline[2] == "str") {
                    ldstr(epBody, splitline[3]);
                    local = getvar(mod.CorLibTypes.String, splitline[1], epBody.Variables);
                }
                if (splitline[2] == "var")
                {
                    ldvar(epBody, splitline[3]);
                    local = getvar(mod.CorLibTypes.String, splitline[1], epBody.Variables);
                }
                
                epBody.Instructions.Add(OpCodes.Stloc.ToInstruction(epBody.Variables.Add(local)));
                return;
            }
            Console.WriteLine(line);
        }

        public static Local getvar(TypeSig type, String name, LocalList variables)
        {
            foreach (Local var in variables) {
                if (var.Name == name) {
                    return var;
                }
            }
            return new Local(type, name);
        }

        public static void ldvar(CilBody epBody, String name)
        {
            Local var = epBody.Variables[0];
            int i = 0;
            while (var.Name != name)
            {
                i++;
                var = epBody.Variables[i];
            }
            epBody.Instructions.Add(OpCodes.Ldloc.ToInstruction(var));
        }

        public static void ldstr(CilBody epBody, String str)
        {
            epBody.Instructions.Add(OpCodes.Ldstr.ToInstruction(str));
        }

        public static void ldint(CilBody epBody, int integer)
        {
            epBody.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(integer));
        }

        public static TypeSig gettypesig(ModuleDefUser mod, Type type) {
            if (typeof(String) == type) {
                return mod.CorLibTypes.String;
            }
            if (typeof(Int32) == type)
            {
                return mod.CorLibTypes.Int32;
            }
            return mod.CorLibTypes.Int32;
        }
    }
}
