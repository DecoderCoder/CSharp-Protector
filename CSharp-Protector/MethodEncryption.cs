using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Confuser.Core.Helpers;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using CallingConvention = dnlib.DotNet.CallingConvention;

namespace CSharp_Protector
{
    public class MethodEncryption : IModuleWriterListener
    {
        public void OnWriterEvent(ModuleWriterBase writer, ModuleWriterEvent e)
        {
            if (e == ModuleWriterEvent.MDEndCreateTables)
            {
                CreateSections(writer);
            }
            else if (e == ModuleWriterEvent.BeginStrongNameSign)
            {
                EncryptSection(writer);
            }
        }

        public List<MethodDef> methods = new List<MethodDef>();

        static Random random = new Random();

        private static uint NextUInt32()
        {
            
            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;
            return fullRange;
        }

        private static byte[] GenerateKEY(int length = 16)
        {
            byte[] rez = new byte[length];
            random.NextBytes(rez);
            return rez;
        }

        uint name1 = NextUInt32();
        uint name2 = NextUInt32();
        private byte[] AESKey = GenerateKEY(32);
        private byte[] AESIV = GenerateKEY();

        private void CreateSections(ModuleWriterBase writer)
        {
            Globals.Log("Creating sections... ", false);

            var nameBuffer = new byte[8];

            nameBuffer[0] = (byte)(name1 >> 0);
            nameBuffer[1] = (byte)(name1 >> 8);
            nameBuffer[2] = (byte)(name1 >> 16);
            nameBuffer[3] = (byte)(name1 >> 24);
            nameBuffer[4] = (byte)(name2 >> 0);
            nameBuffer[5] = (byte)(name2 >> 8);
            nameBuffer[6] = (byte)(name2 >> 16);
            nameBuffer[7] = (byte)(name2 >> 24);

            var newSection = new PESection(Encoding.ASCII.GetString(nameBuffer), 0xE0000040);
            writer.Sections.Insert(0, newSection);

            uint alignment;
            alignment = writer.TextSection.Remove(writer.Metadata).Value;
            writer.TextSection.Add(writer.Metadata, alignment);
            alignment = writer.TextSection.Remove(writer.NetResources).Value;
            writer.TextSection.Add(writer.NetResources, alignment);
            alignment = writer.TextSection.Remove(writer.Constants).Value;
            newSection.Add(writer.Constants, alignment);
            var encryptedChunk = new MethodBodyChunks(writer.TheOptions.ShareMethodBodies);
            newSection.Add(encryptedChunk, 4);
            foreach (var method in methods)
            {
                if (!method.HasBody)
                    continue;
                var body = writer.Metadata.GetMethodBody(method);
                var ok = writer.MethodBodies.Remove(body); // <<< Need FIX
                encryptedChunk.Add(body);
            }

            newSection.Add(new ByteArrayChunk(new byte[4]), 4);
            Globals.Log("OK");
        }

        public static int IndexInArray(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        private void EncryptSection(ModuleWriterBase writer)
        {
            Globals.Log("Encrypting section... ", false);
            Stream stream = writer.DestinationStream;
            uint offset = (uint)writer.Sections[0].FileOffset;
            uint slength = writer.Sections[0].GetFileLength();
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter bwriter = new BinaryWriter(stream);
            stream.Position = 0;
            stream.Position = offset;

            byte[] section = new byte[slength];
            for (var i = 0; i < slength; i++)
            {
                section[i] = reader.ReadByte();
            }

            byte[] encrypted;

            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.KeySize = 256;
                rijAlg.Key = AESKey;
                rijAlg.IV = AESIV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                rijAlg.Mode = CipherMode.CBC;
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                        {
                            swEncrypt.Write(section);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            stream.Position = offset;
            for (var i = 0; i < encrypted.Length; i++)
            {
                bwriter.Write(encrypted[i]);
            }

            stream.Position = 0;
            //byte[] dump = new byte[stream.Length];
            //byte[] inttoreplace = new byte[] { 0x64, 0x03, 0x00, 0x00 };
            //for (int i = 0; i < dump.Length; i++)
            //{
            //    dump[i] = reader.ReadByte();
            //}
            //int replaceoffset = IndexInArray(dump, inttoreplace) + 0;
            //stream.Position = replaceoffset;
            //bwriter.Write(BitConverter.GetBytes((uint)slength));

            Globals.Log("OK");
            Globals.Log("Section length: " + encrypted.Length);
            Globals.Log();

            Globals.Log("AES Mode: CBC");
            Globals.Log("AES IV: " + String.Join(",0x", AESIV.Select(x => x.ToString("x2"))).ToUpper());
            Globals.Log("AES Key size: 256");
            Globals.Log("AES Key: " + String.Join(",0x", AESKey.Select(x => x.ToString("x2"))).ToUpper());

            //Globals.Log("Random byte: " + randomByte[0].ToString("x2").ToUpper());
        }

        public static void AddVoids(TypeDef typeDef, MethodDef methodDef)
        {
            ModuleDefMD module = ModuleDefMD.Load(typeof(Methods).Module);
            TypeDef type = module.ResolveTypeDef(MDToken.ToRID(typeof(Methods).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(type, module.GlobalType, module);
            MethodDef ProtectMethod = (MethodDef) members.Single(method => method.Name == "Protect64");
            ProtectMethod.DeclaringType = null;
            typeDef.Methods.Add(ProtectMethod);
            if (methodDef.Body == null)
                methodDef.Body = new CilBody();
            methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, ProtectMethod));
           // methodDef.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Ret));
        }
    }
}
