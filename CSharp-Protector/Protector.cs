using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CSharp_Protector
{
    [Flags]
    public enum ProtectFlag
    {
        None, AntiDebugNativeDef, AntiDebugManagedDef, AntiDumpDef, MethodEncryptionDef, CRC32CheckDef
    }
    
    public class Protector
    {
        // TRUE - x64 | FALSE - x32
        public bool Platform = true;
        public long AntiDebugManagedDelay = 1000000; // 1000000 = 100ms, Delay / 10000 = Miliseconds

        public void Protect(ProtectFlag flags, bool platform = true, long admdelay = 1000000)
        {
            Platform = platform;
            AntiDebugManagedDelay = admdelay;
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

            Globals.Log("#define AntiDebugNativeDef " + (protect.HasFlag(ProtectFlag.AntiDebugNativeDef) ? "1" : "0"));
            Globals.Log("#define AntiDebugManagedDef " + (protect.HasFlag(ProtectFlag.AntiDebugManagedDef) ? "1" : "0"));
            Globals.Log("#define AntiDumpDef " + (protect.HasFlag(ProtectFlag.AntiDumpDef) ? "1" : "0"));
            Globals.Log("#define MethodEncryptionDef " + (protect.HasFlag(ProtectFlag.MethodEncryptionDef) ? "1" : "0"));
            Globals.Log("#define CRC32CheckDef " + (protect.HasFlag(ProtectFlag.CRC32CheckDef) ? "1" : "0"));

            if (protect.HasFlag(ProtectFlag.MethodEncryptionDef))
            {
                MethodEncryption.AddVoids(this, EntryType, Globals.Assembly.Modules[0].EntryPoint);
                opts.Listener = new MethodEncryption() { methods = MethodsToProtect };
            }


            Directory.CreateDirectory(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\");
            Globals.Assembly.Write(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\" + Path.GetFileName(Globals.AssemblyPath), opts);

            byte[] asm = File.ReadAllBytes(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\" +
                                           Path.GetFileName(Globals.AssemblyPath));

            Globals.Log();
            Globals.Log("#define CRC32 " + Crc32.Get(asm));
            Globals.Log("#define AssemblySize " + asm.Length);
            Globals.Log();
            Globals.Log("#define AntiDebugManagedDelay " + AntiDebugManagedDelay);
            Globals.Log();
            Globals.Log("Protect complete");

            File.WriteAllText(Path.GetDirectoryName(Globals.AssemblyPath) + "\\Protected\\" +
                              Path.GetFileNameWithoutExtension(Globals.AssemblyPath) + "_log.txt", Globals.GetLog());
        }
    }
}
