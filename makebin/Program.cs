using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace makebin
{
    class Program
    {
        public static MethodDef currentfunc = null;
        public static TypeDefUser startUpType = null;
        public static BranchManager manager = new BranchManager();

        static void Main(string[] args)
        {
            String[] file = File.ReadAllLines("precomp.psnbin");
            var mod = newmod("PointySnakeModule");
            var entryPoint = new MethodDefUser("Main", MethodSig.CreateStatic(mod.CorLibTypes.Int32, new SZArraySig(mod.CorLibTypes.String)));
            startUpType = new TypeDefUser("PointySnake", "Program", mod.CorLibTypes.Object.TypeDefOrRef);
            mod.Types.Add(startUpType);
            entryPoint.Attributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            entryPoint.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            entryPoint.ParamDefs.Add(new ParamDefUser("args", 1));
            var epBody = new CilBody();
            entryPoint.Body = epBody;
            startUpType.Attributes = TypeAttributes.Public;
            startUpType.Methods.Add(entryPoint);
            mod.EntryPoint = entryPoint;
            currentfunc = mod.EntryPoint;
            foreach (String line in file) {
                WriteInstruction(line, mod, currentfunc.Body);
            }
            if (epBody.Instructions[epBody.Instructions.Count - 1].OpCode != OpCodes.Ret)
            {
                entryPoint.Body.Instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
                entryPoint.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            }
            entryPoint.Body.OptimizeBranches();
            entryPoint.Body.OptimizeMacros();
            Console.WriteLine("done");
            var options = new ModuleWriterOptions(mod);
            options.PEHeadersOptions.Machine = dnlib.PE.Machine.AMD64;
            mod.Write("executable.exe", options);
            awaitbutton();
        }

        public static ModuleDefUser newmod(String name) {
            var mod = new ModuleDefUser(name);
            mod.RuntimeVersion = "v4.0.30319";
            mod.Kind = ModuleKind.Console;
            var asm = new AssemblyDefUser("PointySnakeAssembly", new Version(0, 0, 0, 0), null, "en_us");
            asm.Modules.Add(mod);
            return mod;
        }

        public static MethodDef newfunc(ModuleDefUser mod, String name) {
            var newfunction = new MethodDefUser(name, MethodSig.CreateStatic(mod.CorLibTypes.Int32));
            newfunction.Attributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            newfunction.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            startUpType.Methods.Add(newfunction);
            newfunction.Body = new CilBody();
            return newfunction;
        }

        public static MethodDef findfunc(String name)
        {
            return startUpType.FindMethod(name);
        }

        public static void awaitbutton()
        {
#if DEBUG
            Console.WriteLine("press any key to close...");
            Console.ReadKey();
#endif
        }

        public static void WriteInstruction(String line, ModuleDefUser mod, CilBody epBody)
        {
            String[] splitline = line.Split(new String[] { ", " }, StringSplitOptions.None);
            switch (splitline[0]) {
                case "ldstr":
                    ldstr(epBody, splitline[1]);
                    return;
                case "ldint":
                    ldint(epBody, int.Parse(splitline[1]));
                    return;
                case "ldbool":
                    ldbool(epBody, splitline[1]);
                    return;
                case "call":
                    switch (splitline[1]) {
                        case "print":
                            var consoleRef = new TypeRefUser(mod, "System", "Console", mod.CorLibTypes.AssemblyRef);
                            Instruction previous = epBody.Instructions[epBody.Instructions.Count - 1];
                            TypeSig type = mod.CorLibTypes.Int32;
                            if (previous.OpCode == OpCodes.Ldstr)
                            {
                                type = mod.CorLibTypes.String;
                            }
                            else if (previous.OpCode == OpCodes.Call)
                            {
                                object operand = previous.GetOperand();
                                if (operand != null && operand is MethodDefUser)
                                {
                                    MethodDefUser method = (MethodDefUser)operand;
                                    type = method.ReturnType;
                                }
                            }
                            else if (previous.IsLdloc())
                            {
                                type = previous.GetLocal(epBody.Variables).Type;
                            }
                            var consoleWrite1 = new MemberRefUser(mod, "WriteLine", MethodSig.CreateStatic(mod.CorLibTypes.Void, type), consoleRef);
                            epBody.Instructions.Add(OpCodes.Call.ToInstruction(consoleWrite1));
                            return;
                        case "return":
                            epBody.Instructions.Add(OpCodes.Ret.ToInstruction());
                            return;
                        case "add":
                            epBody.Instructions.Add(OpCodes.Add.ToInstruction());
                            return;
                        case "sub":
                            epBody.Instructions.Add(OpCodes.Sub.ToInstruction());
                            return;
                        case "mult":
                            epBody.Instructions.Add(OpCodes.Mul.ToInstruction());
                            return;
                        case "div":
                            epBody.Instructions.Add(OpCodes.Div_Un.ToInstruction());
                            return;
                        case "eq":
                            epBody.Instructions.Add(OpCodes.Ceq.ToInstruction());
                            return;
                    }
                    epBody.Instructions.Add(OpCodes.Call.ToInstruction(findfunc(splitline[1])));
                    return;
                case "ldvar":
                    ldvar(epBody, splitline[1]);
                    return;
                case "setvar":
                    Local local = null;
                    switch (splitline[2]) {
                        case "int":
                            local = getvar(mod.CorLibTypes.Int32, splitline[1], epBody.Variables);
                            break;
                        case "str":
                            local = getvar(mod.CorLibTypes.String, splitline[1], epBody.Variables);
                            break;
                        case "bool":
                            local = getvar(mod.CorLibTypes.Boolean, splitline[1], epBody.Variables);
                            break;
                        case "var":
                            ldvar(epBody, splitline[3]);
                            local = getvar(mod.CorLibTypes.String, splitline[1], epBody.Variables);
                            break;
                    }
                    epBody.Instructions.Add(OpCodes.Stloc.ToInstruction(epBody.Variables.Add(local)));
                    return;
                case "newfunc":
                    currentfunc = newfunc(mod, splitline[1]);
                    return;
                case "endfunc":
                    currentfunc = mod.EntryPoint;
                    return;
                case "cond":
                    epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
                    int placeholder = epBody.Instructions.Count - 1;
                    epBody.Instructions.Add(OpCodes.Call.ToInstruction(findfunc(splitline[1])));
                    epBody.Instructions.Add(OpCodes.Pop.ToInstruction());
                    //wasted instruction, fix potentially
                    epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
                    int blockend = epBody.Instructions.Count - 1;
                    epBody.Instructions[placeholder] = OpCodes.Brfalse.ToInstruction(epBody.Instructions[blockend]);
                    return;
                case "brtrue":
                    //might want to attach branch type to branch end since this code just adds a placeholder NOP
                    epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
                    manager.pushbranch(int.Parse(splitline[1]), epBody.Instructions.Count - 1);
                    return;
                case "brend":
                    //wasted instruction, fix potentially
                    epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
                    placeholder = epBody.Instructions.Count - 1;
                    epBody.Instructions[manager.getbranch(int.Parse(splitline[1]))] = OpCodes.Brtrue.ToInstruction(epBody.Instructions[placeholder]);
                    return;
            }
            Console.WriteLine("error: unknown intermediate instruction");
            Console.WriteLine(line);
            awaitbutton();
            Environment.Exit(1);
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

        public static void ldbool(CilBody epBody, string boolean)
        {
            if (boolean == "true") {
                epBody.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(1));
            } else {
                epBody.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(0));
            }
            
        }
    }
}
