using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VMCompiler;

namespace VMCompiler
{
    internal static class Compiler
    {
        public static StringsContainer stringsContainer;
        public static VariablesContainer globalVariables;
        public static UsingContainer usingsContainer;

        public static void Compile(string codeString, string filename)
        {
            { //Clean codeString
                Regex comments = new Regex(" *\\/\\/.*");
                int deleted = 0;
                foreach(Match m in comments.Matches(codeString))
                {
                    codeString = codeString.Remove(m.Index - deleted, m.Length);
                    deleted += m.Length;
                }
            }

            stringsContainer = new StringsContainer();
            globalVariables = new VariablesContainer();
            usingsContainer = new UsingContainer();

            VoidType[] voids = new VoidType[0];

            int CodeSectionStart = 0;
            int CodeSectionLength = 0;
            int TextSectionStart = 0;
            int TextSectionLength = 0;
            int UsingSectionStart = 0;
            int UsingSectionLength = 0;


            byte[] CodeSection = new byte[0];
            byte[] TextSection = new byte[0];
            byte[] UsingSection = new byte[0];
            {
                //Generate Using
                Regex usings = new Regex(@"using (.*?)=(.*?);");
                foreach (Match m in usings.Matches(codeString))
                {
                    Array.Resize(ref UsingSection, UsingSection.Length + m.Groups[2].Value.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(m.Groups[2].Value), 0, UsingSection,
                        UsingSection.Length - m.Groups[2].Value.Length, m.Groups[2].Value.Length);
                    usingsContainer.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }

            {
                //Generate Strings
                Regex strings = new Regex("\"(.*?)\"");
                foreach (Match m in strings.Matches(codeString))
                {
                    stringsContainer.Add(m.Groups[1].Value);
                    Array.Resize(ref TextSection, TextSection.Length + m.Groups[1].Value.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(m.Groups[1].Value), 0, TextSection,
                        TextSection.Length - m.Groups[1].Value.Length, m.Groups[1].Value.Length);
                }
            }

            {
                //Parse Voids
                Regex voidstext = new Regex("void ([a-zA-Z0-9]{1,})\\n{\\n(?:\\n*.*?\\n*){1,}}");
                MatchCollection voidstextm = voidstext.Matches(codeString);
                foreach (Match m in voidstextm)
                {
                    int ofs = Array.IndexOf(codeString.Split('\n'), "void " + m.Groups[1].Value) + 1;
                    VoidType v = new VoidType(m.Groups[1].Value, m.Groups[0].Value, ofs);
                    Array.Resize(ref voids, voids.Length + 1);
                    voids[voids.Length - 1] = v;
                }

                int whole = 0;

                foreach (var v in voids)
                {
                    v.Index = whole;
                    whole += v.Length;
                }
            }

            {
                //Parse Global Variables
                Regex global = new Regex("Global\\n{\\n(?:\\n*.*?\\n*){1,}}");
                MatchCollection globalsm = global.Matches(codeString);
                foreach (Match g in globalsm)
                {
                    Regex var = new Regex("var ([a-zA-Z0-9]{1,})\n");
                    MatchCollection varsm = var.Matches(g.Value);
                    foreach (Match m in varsm)
                    {
                        globalVariables.Add(m.Groups[1].Value);
                    }
                }
            }

            {
                //Generate 
                foreach (var v in voids)
                {
                    foreach (var i in v.Instructions)
                    {
                        switch (i.opCode)
                        {
                            case OpCode.LdStr:
                            {
                                string str = i.operand.ToString();
                                str = str.Substring(1, str.Length - 2);
                                Array.Resize(ref CodeSection, CodeSection.Length + 9);
                                CodeSection[CodeSection.Length - 9] = 0x50;
                                Array.Copy(BitConverter.GetBytes(stringsContainer[str].Index), 0, CodeSection,
                                    CodeSection.Length - 8, 4);
                                Array.Copy(BitConverter.GetBytes(stringsContainer[str].Length), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.LdInt:
                            {
                                int oper = i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x51;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.LdByte:
                            {
                                byte oper = i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 2);
                                CodeSection[CodeSection.Length - 2] = 0x52;
                                CodeSection[CodeSection.Length - 1] = oper;
                            }
                                break;
                            case OpCode.Newobj:
                            {
                                UsingType oper = i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 9);
                                CodeSection[CodeSection.Length - 9] = 0x54;
                                Array.Copy(BitConverter.GetBytes(oper.Start), 0, CodeSection,
                                    CodeSection.Length - 8, 4);
                                Array.Copy(BitConverter.GetBytes(oper.Length), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.Newarr:
                            {
                                UsingType oper = i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 9);
                                CodeSection[CodeSection.Length - 9] = 0x70;
                                Array.Copy(BitConverter.GetBytes(oper.Start), 0, CodeSection,
                                    CodeSection.Length - 8, 4);
                                Array.Copy(BitConverter.GetBytes(oper.Length), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.Setarr:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x71;
                            }
                                break;
                            case OpCode.Getarr:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x72;
                            }
                                break;
                            case OpCode.Br:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x60;
                                Array.Copy(BitConverter.GetBytes(v.Instructions[i.operand].Index), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.BrTrue:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x61;
                                Array.Copy(BitConverter.GetBytes(v.Instructions[i.operand].Index), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.BrFalse:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x62;
                                Array.Copy(BitConverter.GetBytes(v.Instructions[i.operand].Index), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;

                            case OpCode.Call:
                            {
                                string voidname = i.operand;
                                int oper = voids.First(x => x.Name == voidname).Index;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x40;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;

                            case OpCode.CallVirtCSharp:
                                {
                                    CSharpVoid vd = i.operand;
                                    stringsContainer.Add(vd.Name);
                                    Array.Resize(ref TextSection, TextSection.Length + vd.Name.Length);
                                    Array.Copy(Encoding.UTF8.GetBytes(vd.Name), 0, TextSection,
                                        TextSection.Length - vd.Name.Length, vd.Name.Length);

                                    Array.Resize(ref CodeSection, CodeSection.Length + vd.Length);
                                    CodeSection[CodeSection.Length - vd.Length] = 0x42;
                                    Array.Copy(BitConverter.GetBytes(stringsContainer[vd.Name].Index), 0, CodeSection,
                                        CodeSection.Length - vd.Length + 1, 4);
                                    Array.Copy(BitConverter.GetBytes(stringsContainer[vd.Name].Length), 0, CodeSection,
                                        CodeSection.Length - vd.Length + 5, 4);

                                    Array.Copy(BitConverter.GetBytes(vd.Arguments.Length), 0, CodeSection,
                                        CodeSection.Length - vd.Length + 9, 4);
                                    for (int a = 0; a < vd.Arguments.Length; a++)
                                    {
                                        Array.Copy(BitConverter.GetBytes(vd.Arguments[a].Length), 0, CodeSection,
                                            CodeSection.Length - vd.Length + 13 + a * 8, 4);
                                        Array.Copy(BitConverter.GetBytes(vd.Arguments[a].Start), 0, CodeSection,
                                            CodeSection.Length - vd.Length + 17 + a * 8, 4);
                                    }
                                }
                                break;
                            case OpCode.CallCSharp:
                            {
                                CSharpVoid vd = i.operand;
                                stringsContainer.Add(vd.Name);
                                Array.Resize(ref TextSection, TextSection.Length + vd.Name.Length);
                                Array.Copy(Encoding.UTF8.GetBytes(vd.Name), 0, TextSection,
                                    TextSection.Length - vd.Name.Length, vd.Name.Length);

                                Array.Resize(ref CodeSection, CodeSection.Length + vd.Length);
                                CodeSection[CodeSection.Length - vd.Length] = 0x41;
                                Array.Copy(BitConverter.GetBytes(usingsContainer[vd.Type.Name].Start), 0, CodeSection,
                                    CodeSection.Length - vd.Length + 1, 4);
                                Array.Copy(BitConverter.GetBytes(usingsContainer[vd.Type.Name].Length), 0, CodeSection,
                                    CodeSection.Length - vd.Length + 5, 4);
                                Array.Copy(BitConverter.GetBytes(stringsContainer[vd.Name].Index), 0, CodeSection,
                                    CodeSection.Length - vd.Length + 9, 4);
                                Array.Copy(BitConverter.GetBytes(stringsContainer[vd.Name].Length), 0, CodeSection,
                                    CodeSection.Length - vd.Length + 13, 4);

                                Array.Copy(BitConverter.GetBytes(vd.Arguments.Length), 0, CodeSection,
                                    CodeSection.Length - vd.Length + 17, 4);

                                for (int a = 0; a < vd.Arguments.Length; a++)
                                {
                                    Array.Copy(BitConverter.GetBytes(vd.Arguments[a].Length), 0, CodeSection,
                                        CodeSection.Length - vd.Length + 21 + a * 8, 4);

                                    Array.Copy(BitConverter.GetBytes(vd.Arguments[a].Start), 0, CodeSection,
                                        CodeSection.Length - vd.Length + 25 + a * 8, 4);
                                }
                            }
                                break;
                            case OpCode.Ceq:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xA1;
                            }
                                break;
                            case OpCode.Cgt:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xA2;
                            }
                                break;
                            case OpCode.Cls:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xA3;
                            }
                                break;
                            case OpCode.Add:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xB1;
                            }
                                break;
                            case OpCode.Sub:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xB2;
                            }
                                break;
                            case OpCode.Mul:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xB3;
                            }
                                break;
                            case OpCode.Div:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0xB4;
                            }
                                break;
                            case OpCode.Pop:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x91;
                            }
                                break;
                            case OpCode.Dup:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x92;
                            }
                                break;
                            case OpCode.StGv:
                            {
                                string varname = i.operand;
                                int oper = globalVariables[varname].Index;
                                i.operand = oper;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x93;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.LdGv:
                            {
                                string varname = i.operand;
                                int oper = globalVariables[varname].Index;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x94;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.StLv:
                            {
                                string varname = i.operand;
                                int oper = v.localVariables[varname].Index;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x95;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.LdLv:
                            {
                                string varname = i.operand;
                                int oper = v.localVariables[varname].Index;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x96;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.Nop:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x90;
                            }
                                break;
                            case OpCode.Ret:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x22;
                            }
                                break;
                        }
                    }
                }
            }


            CodeSectionStart = 0;
            CodeSectionLength = CodeSection.Length;
            TextSectionStart = CodeSectionStart + CodeSectionLength;
            TextSectionLength = TextSection.Length;
            UsingSectionStart = TextSectionStart + TextSectionLength;
            UsingSectionLength = UsingSection.Length;

            FileStream stream = new FileStream(filename, FileMode.Create);
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(CodeSectionStart);
                writer.Write(CodeSectionLength);

                writer.Write(TextSectionStart);
                writer.Write(TextSectionLength);

                writer.Write(UsingSectionStart);
                writer.Write(UsingSectionLength);

                writer.Write(CodeSection);
                writer.Write(TextSection);
                writer.Write(UsingSection);
            }
        }
    }
}
