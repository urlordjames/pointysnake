using System;
using System.IO;
using System.Collections.Generic;
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
            string[] file = File.ReadAllLines("precomp.psnbin");
            ModuleDefUser mod = newmod("PointySnakeModule");
            MethodDefUser entryPoint = new MethodDefUser("Main", MethodSig.CreateStatic(mod.CorLibTypes.Int32, new SZArraySig(mod.CorLibTypes.String)));
            startUpType = new TypeDefUser("PointySnake", "Program", mod.CorLibTypes.Object.TypeDefOrRef);
            mod.Types.Add(startUpType);
            entryPoint.Attributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            entryPoint.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed | MethodImplAttributes.AggressiveOptimization;
            entryPoint.ParamDefs.Add(new ParamDefUser("args", 1));
            var epBody = new CilBody();
            entryPoint.Body = epBody;
            startUpType.Attributes = TypeAttributes.Public;
            startUpType.Methods.Add(entryPoint);
            mod.EntryPoint = entryPoint;
            currentfunc = mod.EntryPoint;
            foreach (string line in file) {
                WriteInstruction(line, mod, currentfunc.Body);
            }
            if (epBody.Instructions.Count == 0 || epBody.Instructions[epBody.Instructions.Count - 1].OpCode != OpCodes.Ret)
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

        public static ModuleDefUser newmod(string name) {
            var mod = new ModuleDefUser(name);
            mod.RuntimeVersion = "v4.0.30319";
            mod.Kind = ModuleKind.Console;
            var asm = new AssemblyDefUser("PointySnakeAssembly", new Version(0, 0, 0, 0), null, "en_us");
            asm.Modules.Add(mod);
            return mod;
        }

        public static MethodDef newfunc(ModuleDefUser mod, string name, CorLibTypeSig returntype, TypeSig[] arguments) {
            MethodDefUser newfunction = new MethodDefUser(name, MethodSig.CreateStatic(returntype, arguments));
            newfunction.Attributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            newfunction.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed | MethodImplAttributes.AggressiveOptimization;
            startUpType.Methods.Add(newfunction);
            newfunction.Body = new CilBody();
            return newfunction;
        }

        public static MethodDef findfunc(string name)
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

        public static void WriteInstruction(string line, ModuleDefUser mod, CilBody epBody)
        {
            string[] splitline = line.Split(new string[] { ", " }, StringSplitOptions.None);
            Instruction previous;
            TypeSig type = mod.CorLibTypes.Int32;
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
                    int placeholder;
                    switch (splitline[1]) {
                        case "print":
                            var consoleRef = new TypeRefUser(mod, "System", "Console", mod.CorLibTypes.AssemblyRef);
                            previous = epBody.Instructions[epBody.Instructions.Count - 1];
                            if (previous.OpCode == OpCodes.Ldstr)
                            {
                                type = mod.CorLibTypes.String;
                            }
                            else if (previous.OpCode == OpCodes.Call)
                            {
                                type = getReturnType(previous.GetOperand());
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
                        case "gt":
                            epBody.Instructions.Add(OpCodes.Cgt.ToInstruction());
                            return;
                        case "lt":
                            epBody.Instructions.Add(OpCodes.Clt.ToInstruction());
                            return;
                    }
                    epBody.Instructions.Add(OpCodes.Call.ToInstruction(findfunc(splitline[1])));
                    return;
                case "ldvar":
                    ldvar(epBody, splitline[1]);
                    return;
                case "setvar":
                    Local local = getvar(typeFromString(mod, splitline[2]), splitline[1], epBody.Variables);
                    epBody.Instructions.Add(OpCodes.Stloc.ToInstruction(epBody.Variables.Add(local)));
                    return;
                case "newfunc":
                    List<string> arguments = new List<string>();
                    foreach (string arg in splitline)
                    {
                        arguments.Add(arg);
                    }
                    arguments.RemoveRange(0, 3);
                    currentfunc = newfunc(mod, splitline[1], typeFromString(mod, splitline[2]), argumentsFromStrings(mod, arguments.ToArray()));
                    return;
                case "endfunc":
                    currentfunc = mod.EntryPoint;
                    return;
                case "br":
                    handleBrBegin(epBody, int.Parse(splitline[1]), "br");
                    return;
                case "brtrue":
                    handleBrBegin(epBody, int.Parse(splitline[1]), "brtrue");
                    return;
                case "brfalse":
                    handleBrBegin(epBody, int.Parse(splitline[1]), "brfalse");
                    return;
                case "brend":
                    //wasted instruction, fix potentially
                    epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
                    placeholder = epBody.Instructions.Count - 1;
                    int branch = int.Parse(splitline[1]);
                    if (manager.branchExists(branch))
                    {
                        handleBrEnd(epBody, branch, placeholder, manager.getBranchType(branch));
                    }
                    else
                    {
                        manager.pushBranch(branch, placeholder, "brend");
                    }
                    return;
                case "pop?":
                    previous = epBody.Instructions[epBody.Instructions.Count - 1];
                    if (previous.OpCode != OpCodes.Call) {
                        return;
                    }
                    type = getReturnType(previous.GetOperand());
                    if (type.GetElementType() != ElementType.Void) {
                        epBody.Instructions.Add(OpCodes.Pop.ToInstruction());
                    }
                    return;
                case "arg":
                    epBody.Instructions.Add(OpCodes.Ldarg.ToInstruction(mod.EntryPoint.Parameters[int.Parse(splitline[1])]));
                    return;
            }
            Console.WriteLine("error: unknown intermediate instruction");
            Console.WriteLine(line);
            awaitbutton();
            Environment.Exit(1);
        }

        public static Local getvar(TypeSig type, string name, LocalList variables)
        {
            foreach (Local var in variables) {
                if (var.Name == name) {
                    return var;
                }
            }
            return new Local(type, name);
        }

        public static CorLibTypeSig typeFromString(ModuleDefUser mod, string name) {
            switch (name)
            {
                case "int":
                    return mod.CorLibTypes.Int32;
                case "str":
                    return mod.CorLibTypes.String;
                case "bool":
                    return mod.CorLibTypes.Boolean;
                case "var":
                    return mod.CorLibTypes.String;
                case "void":
                    return mod.CorLibTypes.Void;
            }
            throw new Exception("invalid type");
        }

        public static void ldvar(CilBody epBody, string name)
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

        public static void ldstr(CilBody epBody, string str)
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

        public static TypeSig getReturnType(object operand)
        {
            //there should probably be a common interface for this...
            if (operand is MemberRef)
            {
                return ((MemberRef)operand).ReturnType;
            }
            if (operand is MethodDef)
            {
                return ((MethodDef)operand).ReturnType;
            }
            throw new Exception("operand invalid");
        }

        public static void handleBrEnd(CilBody epBody, int branch, int placeholder, string type) {
            switch (type)
            {
                case "br":
                    epBody.Instructions[manager.getBranch(branch)] = OpCodes.Br.ToInstruction(epBody.Instructions[placeholder]);
                    break;
                case "brtrue":
                    epBody.Instructions[manager.getBranch(branch)] = OpCodes.Brtrue.ToInstruction(epBody.Instructions[placeholder]);
                    break;
                case "brfalse":
                    epBody.Instructions[manager.getBranch(branch)] = OpCodes.Brfalse.ToInstruction(epBody.Instructions[placeholder]);
                    break;
            }
        }

        public static void handleBrBegin(CilBody epBody, int branch, string instruction) {
            epBody.Instructions.Add(OpCodes.Nop.ToInstruction());
            int placeholder = epBody.Instructions.Count - 1;
            if (manager.branchExists(branch))
            {
                epBody.Instructions.Add(OpCodes.Brtrue.ToInstruction(epBody.Instructions[manager.getBranch(branch)]));
            }
            else
            {
                manager.pushBranch(branch, placeholder, instruction);
            }
        }

        public static TypeSig[] argumentsFromStrings(ModuleDefUser mod, string[] names) {
            List<TypeSig> arguments = new List<TypeSig>();
            foreach (string argname in names)
            {
                arguments.Add(typeFromString(mod, argname));
            }
            return arguments.ToArray();
        }
    }
}
