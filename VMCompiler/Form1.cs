using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VMCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class StringType
        {
            public string String = "";
            public int Index = 0;
            public int Length = 0;

            public static explicit operator string(StringType stringType)
            {
                return stringType.String;
            }

            public StringType(string str, int index, int length)
            {
                String = str;
                Index = index;
                Length = length;
            }
        }

        class StringsContainer
        {
            private int wholelength = 0;

            private StringType[] strings = new StringType[0];

            public StringType this[string str]
            {
                get { return strings.First(x => x.String == str); }
            }

            public StringType Add(string str)
            {
                if (strings.Count(x => x.String == str) == 0)
                {
                    Array.Resize(ref strings, strings.Length + 1);
                    strings[strings.Length - 1] = new StringType(str, wholelength, str.Length);
                    wholelength += str.Length;
                    return strings[strings.Length - 1];
                }
                return strings.First(x => x.String == str);
            }
        }

        enum OpCode
        {
            LdStr, //50
            LdInt, //51
            Call,  //40
            CallC,  //41
            Br,    //60
            Nop,   //90
            Pop,   //91
            Ret    //22
        }

        class Instruction
        {
            public OpCode opCode;
            public object operand;

            public Instruction(OpCode opcode, object oper)
            {
                opCode = opcode;
                operand = oper;
            }
        }

        class VoidType
        {
            // void ([a-zA-Z0-9]{1,})\n{\n(?:\n*.*?\n*){1,}}
            public string Name;
            public int Index = 0;
            public int Length = 0;

            public Instruction[] Instructions = new Instruction[0];

            public VoidType(string name, string v)
            {
                Name = name;
                string[] str = v.Split('\n');
                for (int i = 2; i < str.Length - 1; i++)
                {
                    Array.Resize(ref Instructions, Instructions.Length + 1);
                    OpCode op = OpCode.Nop;
                    if (str[i].Contains(" "))
                        Enum.TryParse<OpCode>(str[i].Substring(0, str[i].IndexOf(" ")), out op);
                    else
                        Enum.TryParse<OpCode>(str[i], out op);
                    switch (op)
                    {
                        case OpCode.LdStr:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 9;
                        }
                            break;
                        case OpCode.LdInt:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, int.Parse(str[i].Substring(str[i].IndexOf(" ") + 1)));
                            Length += 5;
                        }
                            break;
                        case OpCode.Call:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 5;
                        }
                            break;
                        case OpCode.Pop:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, null);
                            Length += 1;
                        }
                            break;
                        case OpCode.Ret:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, null);
                            Length += 1;
                        }
                            break;
                        default:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(OpCode.Nop, null);
                            }
                            break;
                    }
                }
            }
        }

        private void Compile()
        {
            StringsContainer stringsContainer = new StringsContainer();
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
            { //Generate Using
                Regex usings = new Regex(@"using (.*?)=(.*?);");
                foreach (Match m in usings.Matches(richTextBox1.Text))
                {
                    Array.Resize(ref UsingSection, UsingSection.Length + m.Groups[2].Value.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(m.Groups[2].Value), 0, UsingSection, UsingSection.Length - m.Groups[2].Value.Length, m.Groups[2].Value.Length);
                }
            }

            { //Generate Strings
                Regex strings = new Regex("\"(.*?)\"");
                foreach (Match m in strings.Matches(richTextBox1.Text))
                {
                    stringsContainer.Add(m.Groups[1].Value);
                    Array.Resize(ref TextSection, TextSection.Length + m.Groups[1].Value.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(m.Groups[1].Value), 0, TextSection, TextSection.Length - m.Groups[1].Value.Length, m.Groups[1].Value.Length);
                }
            }

            {
                //Parse Voids
                Regex voidstext = new Regex("void ([a-zA-Z0-9]{1,})\\n{\\n(?:\\n*.*?\\n*){1,}}");
                MatchCollection voidstextm = voidstext.Matches(richTextBox1.Text);
                foreach (Match m in voidstextm)
                {
                    VoidType v = new VoidType(m.Groups[1].Value, m.Groups[0].Value);
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
                                int oper = (int)i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x51;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.Call:
                            {
                                string voidname = (string)i.operand;
                                int oper = voids.First(x => x.Name == voidname).Index;
                                i.operand = oper;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x40;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.Pop:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x91;
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

            FileStream stream = new FileStream(textBox2.Text, FileMode.Create);
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

        private void button1_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("text.txt"))
                richTextBox1.Text = File.ReadAllText("text.txt");
            if (File.Exists("save.txt"))
                textBox2.Text = File.ReadAllText("save.txt");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
             File.WriteAllText("text.txt", richTextBox1.Text);
             File.WriteAllText("save.txt", textBox2.Text);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            var start = richTextBox1.SelectionStart;
            richTextBox1.SelectAll();
            richTextBox1.SelectionColor = richTextBox1.ForeColor;

            Highlight(richTextBox1.Text, 0);

            richTextBox1.Select(start, 0);
        }

        private void Highlight(string text, int offset)
        {
            Regex voids = new Regex(@"void");
            MatchCollection voidsm = voids.Matches(text);
            foreach (Match match in voidsm)
            {
                richTextBox1.Select(match.Index + offset, 4);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");
            }

            Regex strings = new Regex("\"(.*?)\"");
            MatchCollection stringsm = strings.Matches(text);
            foreach (Match match in stringsm)
            {
                richTextBox1.Select(match.Index + offset, match.Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#e67e22");
            }

            Regex include = new Regex(@"using (.*?)=(.*?);");
            MatchCollection includem = include.Matches(text);
            foreach (Match match in includem)
            {
                richTextBox1.Select(match.Index + offset, 5);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");

                richTextBox1.Select(match.Groups[1].Index + match.Groups[1].Length + offset, 1);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");
            }

            Regex ints = new Regex(@" {1,}([0-9]{1,})\n*");
            MatchCollection intsm = ints.Matches(text);
            foreach (Match match in intsm)
            {
                richTextBox1.Select(match.Groups[1].Index + offset, match.Groups[1].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#27ae60");
            }

            Regex calls = new Regex(@"Call ([a-zA-Z0-9]{1,})");
            MatchCollection callsm = calls.Matches(text);
            foreach (Match match in callsm)
            {
                richTextBox1.Select(match.Groups[1].Index + offset, match.Groups[1].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");
            }
        }
    }
}
