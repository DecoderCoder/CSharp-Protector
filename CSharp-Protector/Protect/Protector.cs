using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CSharp_Protector
{
    public class Protector
    {
        public bool AntiDebugNativeDef = false;
        public bool AntiDebugManagedDef = false;
        public bool AntiDumpDef = false;
        public bool MethodEncryptionDef = false;
        public bool Crc32CheckDef = false;
        public bool ControlFlowB = false;

        // TRUE - x64 | FALSE - x32
        public bool Platform = true;
        public long AntiDebugManagedDelay = 1000000; // 1000000 = 100ms, Delay / 10000 = Miliseconds

        public void Protect(bool platform = true, long admdelay = 1000000)
        {
            Platform = platform;
            AntiDebugManagedDelay = admdelay;
            Globals.Log("AntiDebugNativeDef: " + AntiDebugNativeDef);
            Globals.Log("AntiDebugManagedDef: " + AntiDebugManagedDef);
            Globals.Log("AntiDumpDef: " + AntiDumpDef);
            Globals.Log("MethodEncryptionDef: " + MethodEncryptionDef);
            Globals.Log("Crc32CheckDef: " + Crc32CheckDef);
            Globals.Log("ControlFlow: " + ControlFlowB);
            Globals.Log();
            ProtectThread();
        }

        private List<MethodDef> MethodsToProtect = new List<MethodDef>();

        private TypeDef EntryType;

        private void ProtectThread()
        {
            EntryType = Globals.Assembly.Modules[0].Types[0];
            //MethodDefUser EntryMethod = new MethodDefUser("DecoderCoder", new MethodSig(CallingConvention.StdCall, 0, Globals.Assembly.Modules[0].CorLibTypes.Void), MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //EntryType.Methods.Add(EntryMethod);

            var opts = new ModuleWriterOptions(Globals.Assembly.Modules[0]);
            opts.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;

            foreach (var type in Globals.Assembly.Modules[0].Types)
            {
                foreach (var method in type.Methods)
                {
                    if(method.HasBody)
                        MethodsToProtect.Add(method);
                }
                
            }

            MethodsToProtect.Remove(Globals.Assembly.Modules[0].EntryPoint);

            Globals.Log("#define AntiDebugNativeDef " + (AntiDebugNativeDef ? "1" : "0"));
            Globals.Log("#define AntiDebugManagedDef " + (AntiDebugManagedDef ? "1" : "0"));
            Globals.Log("#define AntiDumpDef " + (AntiDumpDef ? "1" : "0"));
            Globals.Log("#define MethodEncryptionDef " + (MethodEncryptionDef ? "1" : "0"));
            Globals.Log("#define CRC32CheckDef " + (Crc32CheckDef ? "1" : "0"));

            if (ControlFlowB)
            {
                ControlFlow.Execute(Globals.Assembly.Modules[0]);
            }

            if (MethodEncryptionDef)
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
