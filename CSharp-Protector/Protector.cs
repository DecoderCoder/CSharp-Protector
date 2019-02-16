using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CSharp_Protector
{
    [Flags]
    enum ProtectFlag
    {
        MethodEncoding
    }

    class Protector
    {
        public void Protect(ProtectFlag flags)
        {
            Globals.Log("Protect: " + flags.ToString());
            Globals.Log();
            ProtectThread(flags);
        }

        private List<MethodDef> MethodsToProtect = new List<MethodDef>();

        private TypeDef EntryType;

        private void ProtectThread(ProtectFlag protect)
        {
            var opts = new ModuleWriterOptions(Globals.Assembly.Modules[0]);
            opts.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            EntryType = Globals.Assembly.Modules[0].Types[0];
            //MethodDefUser EntryMethod = new MethodDefUser("DecoderCoder", new MethodSig(CallingConvention.StdCall, 0, Globals.Assembly.Modules[0].CorLibTypes.Void), MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //EntryType.Methods.Add(EntryMethod);

            foreach (var type in Globals.Assembly.Modules[0].Types)
            {
                foreach (var method in type.Methods)
                {
                    if(method.HasBody)
                        MethodsToProtect.Add(method);
                }
                
            }

            MethodsToProtect.Remove(Globals.Assembly.Modules[0].EntryPoint);

            if (protect.HasFlag(ProtectFlag.MethodEncoding))
            {
                MethodEncryption.AddVoids(EntryType, Globals.Assembly.Modules[0].EntryPoint);
                opts.Listener = new MethodEncryption() { methods = MethodsToProtect };
            }
            
            
            Directory.CreateDirectory(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\");
            Globals.Assembly.Write(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\" + Path.GetFileName(Globals.AssemblyPath), opts);

            Globals.Log();
            Globals.Log("Protect complete");
        }
    }
}
