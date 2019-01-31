using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
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
            // Types
            LdStr, //50  //Load String to Stack
            LdInt, //51  //Load Integer to Stack
            LdByte,//52  //Load Byte to Stack
            // Voids
            Call,  //40  //Call Void
            CallCSharp, //41 //CallCSharp Void
            // Jumps
            Br,    //60  //Jump to Instruction
            // Stack & Variables
            Pop,   //91  //Remove Top object in Stack
            Dup,   //92  //Duplicate top object in Stack
            StGv,  //93  //Push Top stack object to Global Variables
            LdGv,  //94  //Load Global Variable to Stack
            StLv,  //95  //Push Top stack object to local Variables
            LdLv,  //96  //Load Local Variable to Stack
            // Other
            Nop,   //90
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
            public string Name;
            public int Index = 0;
            public int Length = 0;

            public Instruction[] Instructions = new Instruction[0];
            public VariablesContainer localVariables = new VariablesContainer();

            public VoidType(string name, string v)
            {
                Name = name;
                Regex vars = new Regex("var ([a-zA-Z0-9]{1,})\n");
                MatchCollection matches = vars.Matches(v);
                var deleted = 0;
                foreach (Match m in matches)
                {
                    localVariables.Add(m.Groups[1].Value);
                    v = v.Remove(m.Index - deleted, m.Length);
                    deleted += m.Length;
                }
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
                        case OpCode.LdByte:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, Convert.ToByte(str[i].Substring(str[i].IndexOf(" ") + 3), 16));
                            Length += 2;
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

                        case OpCode.CallCSharp:
                        {
                            string opstr = str[i].Substring(str[i].IndexOf(" ") + 1);
                            CSharpVoid oper = new CSharpVoid();

                            Regex vo = new Regex("([a-zA-Z0-9]{1,})\\:([a-zA-Z0-9]{1,})\\((.*)\\)");
                            Match voi = vo.Match(opstr);

                            oper.Type = usingsContainer[voi.Groups[1].Value];
                            oper.Name = voi.Groups[2].Value;

                            string[] args = voi.Groups[3].Value.Split(',');
                            Array.Resize(ref oper.Arguments, args.Length);

                            for (int a = 0; a < args.Length; a++)
                            {
                                oper.Arguments[a] = usingsContainer[args[a]];
                            }

                            oper.Length = 21 + 8 * args.Length;

                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, oper);

                            Length += 21 + 8 * args.Length;
                        }
                            break;

                        case OpCode.Pop:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, null);
                            Length += 1;
                        }
                            break;

                        case OpCode.StGv:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 5;
                        }
                            break;
                        case OpCode.LdGv:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 5;
                        }
                            break;
                        case OpCode.StLv:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 5;
                        }
                            break;
                        case OpCode.LdLv:
                        {
                            Instructions[Instructions.Length - 1] =
                                new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1));
                            Length += 5;
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

        class UsingType
        {
            public int Start;
            public int Length;
            public string Name;

            public UsingType(int start, int length, string name)
            {
                Start = start;
                Length = length;
                Name = name;
            }
        }

        class UsingContainer
        {
            private int wholelength = 0;
            private UsingType[] usings = new UsingType[0];

            public UsingType this[string name]
            {
                get { return usings.First(x => x.Name == name); }
            }

            public UsingType Add(string name, string type)
            {
                if (usings.Count(x => x.Name == name) == 0)
                {
                    Array.Resize(ref usings, usings.Length + 1);
                    usings[usings.Length - 1] = new UsingType(wholelength, type.Length , name);
                    wholelength += type.Length;
                }
                return usings.First(x => x.Name == name);
            }
        }

        class CSharpVoid
        {
            public int Length;
            public UsingType Type;
            public string Name;
            public UsingType[] Arguments;
        }

        class VariableType
        {
            public string Name;
            public int Index;

            public VariableType(string name, int index)
            {
                Name = name;
                Index = index;
            }
        }

        class VariablesContainer
        {
            private VariableType[] variables = new VariableType[0];

            public VariableType this[string name]
            {
                get { return variables.First(x => x.Name == name); }
            }

            public VariableType Add(string name)
            {
                if (variables.Count(x => x.Name == name) == 0)
                {
                    Array.Resize(ref variables, variables.Length + 1);
                    variables[variables.Length - 1] = new VariableType(name, variables.Length);
                }
                return variables.First(x => x.Name == name);
            }
        }

        static StringsContainer stringsContainer;
        static VariablesContainer globalVariables;
        static UsingContainer usingsContainer;

        private void Compile()
        {
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
            { //Generate Using
                Regex usings = new Regex(@"using (.*?)=(.*?);");
                foreach (Match m in usings.Matches(richTextBox1.Text))
                {
                    Array.Resize(ref UsingSection, UsingSection.Length + m.Groups[2].Value.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(m.Groups[2].Value), 0, UsingSection, UsingSection.Length - m.Groups[2].Value.Length, m.Groups[2].Value.Length);
                    usingsContainer.Add(m.Groups[1].Value, m.Groups[2].Value);
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

            { //Parse Global Variables
                Regex global = new Regex("Global\\n{\\n(?:\\n*.*?\\n*){1,}}");
                MatchCollection globalsm = global.Matches(richTextBox1.Text);
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
                                int oper = (int)i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x51;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.LdByte:
                            {
                                int oper = (byte) i.operand;
                                Array.Resize(ref CodeSection, CodeSection.Length + 2);
                                CodeSection[CodeSection.Length - 2] = 0x52;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 1, 1);
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
                            case OpCode.CallCSharp:
                            {
                                CSharpVoid vd = (CSharpVoid) i.operand;
                                stringsContainer.Add(vd.Name);
                                Array.Resize(ref TextSection, TextSection.Length + vd.Name.Length);
                                Array.Copy(Encoding.UTF8.GetBytes(vd.Name), 0, TextSection, TextSection.Length - vd.Name.Length, vd.Name.Length);

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
                            case OpCode.Pop:
                            {
                                Array.Resize(ref CodeSection, CodeSection.Length + 1);
                                CodeSection[CodeSection.Length - 1] = 0x91;
                            }
                                break;

                            case OpCode.StGv:
                            {
                                string varname = (string) i.operand;
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
                                string varname = (string)i.operand;
                                int oper = globalVariables[varname].Index;
                                i.operand = oper;
                                Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                CodeSection[CodeSection.Length - 5] = 0x94;
                                Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                    CodeSection.Length - 4, 4);
                            }
                                break;
                            case OpCode.StLv:
                                {
                                    string varname = (string)i.operand;
                                    int oper = v.localVariables[varname].Index;
                                    i.operand = oper;
                                    Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                    CodeSection[CodeSection.Length - 5] = 0x95;
                                    Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                        CodeSection.Length - 4, 4);
                                }
                                break;
                            case OpCode.LdLv:
                                {
                                    string varname = (string)i.operand;
                                    int oper = v.localVariables[varname].Index;
                                    i.operand = oper;
                                    Array.Resize(ref CodeSection, CodeSection.Length + 5);
                                    CodeSection[CodeSection.Length - 5] = 0x96;
                                    Array.Copy(BitConverter.GetBytes(oper), 0, CodeSection,
                                        CodeSection.Length - 4, 4);
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
            Regex voids = new Regex("void ([a-zA-Z0-9]{1,})\\n{\\n(?:\\n*.*?\\n*){1,}}");
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

                richTextBox1.Select(match.Groups[1].Index + offset, match.Groups[1].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#2ecc71");

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

            Regex vars = new Regex(@"var ([a-zA-Z0-9]{1,})\n");
            MatchCollection varsm = vars.Matches(text);
            foreach (Match match in varsm)
            {
                richTextBox1.Select(match.Index + offset, 3);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");
            }

            Regex globaldef = new Regex(@"Global\n{");
            MatchCollection globaldefm = globaldef.Matches(text);
            foreach (Match match in globaldefm)
            {
                richTextBox1.Select(match.Index + offset, 6);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#3498db");
            }

            Regex callcsharp = new Regex(@"([a-zA-Z0-9]{1,})\:(.*?)\((.*)\)");
            MatchCollection callcsharpm = callcsharp.Matches(text);
            foreach (Match match in callcsharpm)
            {
                richTextBox1.Select(match.Groups[1].Index + offset, match.Groups[1].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#27ae60");

                richTextBox1.Select(match.Groups[2].Index + offset, match.Groups[2].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#e67e22");

                richTextBox1.Select(match.Groups[3].Index + offset, match.Groups[3].Length);
                richTextBox1.SelectionColor = ColorTranslator.FromHtml("#27ae60");
            }
        }

        private void getTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GetTypeName().Show();
        }

        private void getMethodsInTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GetTypeMethods().Show();
        }
    }
}
