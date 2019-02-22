using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CSharp_Protector
{
    public static class ControlFlow
    {
        public static void Execute(ModuleDef module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods.Where(x => x.HasBody))
                {
                    Trash(method);
                }
            }
        }

        private static Instruction randomInstruction(ref Random r, MethodDef method)
        {
            Instruction Rez = new Instruction(OpCodes.Ret);
            switch (r.Next(0, 5))
            {
                case 0:
                    Rez = new Instruction(OpCodes.Pop);
                    break;
                case 1:
                    Rez = new Instruction(OpCodes.Brtrue, method.Body.Instructions[r.Next(0, method.Body.Instructions.Count)]);
                    break;
                case 2:
                    Rez = new Instruction(OpCodes.Brfalse, method.Body.Instructions[r.Next(0, method.Body.Instructions.Count)]);
                    break;
                case 3:
                    if (method.DeclaringType.Fields.Count > 0)
                        Rez = new Instruction(OpCodes.Ldfld, method.DeclaringType.Fields[r.Next(0, method.DeclaringType.Fields.Count)]);
                    else
                        Rez = new Instruction(OpCodes.Ldtoken, method.DeclaringType);
                    break;
                case 4:
                   // Rez = new Instruction(OpCodes.Ldloc_0, method.Body.Instructions[r.Next(0, method.Body.Instructions.Count)]);
                    break;
            }
            return Rez;
        }

        public static void Trash(MethodDef method)
        {
            Random r = new Random();
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                method.Body.SimplifyMacros(method.Parameters);
                switch (r.Next(0, 10))
                {
                    case 0: // Br
                        method.Body.Instructions.Insert(i, new Instruction(OpCodes.Br, method.Body.Instructions[i++]));
                        for (int g = 0; g < r.Next(0, 4); g++)
                        {
                            method.Body.Instructions.Insert(++i, randomInstruction(ref r, method));
                        }
                        break;
                }
                method.Body.OptimizeMacros();
            }
        }
    }
}
