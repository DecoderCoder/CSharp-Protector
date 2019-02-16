using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VMCompiler
{
    internal class StringType
    {
        public string String;
        public int Index;
        public int Length;

        public StringType(string str, int index, int length)
        {
            String = str;
            Index = index;
            Length = length;
        }
    }

    internal class StringsContainer
    {
        private int wholelength;

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

    internal enum OpCode
    {
        // Types
        LdStr, //50  //Load String to Stack
        LdByte, //52  //Load Byte to Stack
        LdInt, //51  //Load Integer to Stack
        //LdLong, //53
        Newobj, //54

        //Arrays
        Newarr, //70
        Setarr, //71
        Getarr, //72


        // Voids
        Call, //40  //Call Void
        CallCSharp, //41 //CallCSharp Void
        CallVirtCSharp, //42 //CallVirtCSharp Void

        // Compare
        Ceq, // A1 // ==              -> 1 | 0
        Cgt, // A2 // Value1 > Value2 -> 1 | 0
        Cls, // A3 // Value1 < Value2 -> 1 | 0

        // Arithmetic
        Add, // B1
        Sub, // B2
        Mul, // B3
        Div, // B4

        // Jumps
        Br, //60  //Jump to Instruction
        BrTrue, //61 // == 1
        BrFalse, //62 // == 0

        // Stack & Variables
        Pop, //91  //Remove Top object in Stack
        Dup, //92  //Duplicate top object in Stack
        StGv, //93  //Push Top stack object to Global Variables
        LdGv, //94  //Load Global Variable to Stack
        StLv, //95  //Push Top stack object to local Variables
        LdLv, //96  //Load Local Variable to Stack

        // Other
        Nop, //90
        Ret //22
    }

    internal class Instruction
    {
        public OpCode opCode;
        public dynamic operand;
        public int Index;

        public Instruction(OpCode opcode, object oper)
        {
            opCode = opcode;
            operand = oper;
        }
    }

    internal class VoidType
    {
        public string Name;
        public int Index = 0;
        public int Length = 0;

        public Instruction[] Instructions = new Instruction[0];
        public VariablesContainer localVariables = new VariablesContainer();

        public VoidType(string name, string v, int ofs)
        {
            ofs += 2;
            Name = name;
            Regex vars = new Regex("var ([a-zA-Z0-9]{1,})\n");
            MatchCollection matches = vars.Matches(v);
            var deleted = 0;
            foreach (Match m in matches)
            {
                localVariables.Add(m.Groups[1].Value);
                v = v.Remove(m.Index - deleted, m.Length);
                deleted += m.Length;
                ofs += 1;
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
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 9;
                    }
                        break;
                    case OpCode.LdByte:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, Convert.ToByte(str[i].Substring(str[i].IndexOf(" ") + 3), 16))
                            {
                                Index = Length
                            };
                        Length += 2;
                    }
                        break;
                    case OpCode.LdInt:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, int.Parse(str[i].Substring(str[i].IndexOf(" ") + 1))) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.Newobj:
                    {
                        Instructions[Instructions.Length - 1] = new Instruction(op,
                            Compiler.usingsContainer[str[i].Substring(str[i].IndexOf(" ") + 1)]) {Index = Length};
                        Length += 9;
                    }
                        break;
                    case OpCode.Newarr:
                    {
                        Instructions[Instructions.Length - 1] = new Instruction(op,
                            Compiler.usingsContainer[str[i].Substring(str[i].IndexOf(" ") + 1)]) {Index = Length};
                        Length += 9;
                    }
                        break;
                    case OpCode.Setarr:
                    {
                        Instructions[Instructions.Length - 1] = new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Getarr:
                    {
                        Instructions[Instructions.Length - 1] = new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Call:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.CallVirtCSharp:
                    {
                        string opstr = str[i].Substring(str[i].IndexOf(" ") + 1);
                        CSharpVoid oper = new CSharpVoid();

                        Regex vo = new Regex("([a-zA-Z0-9]{1,})\\((.*)\\)");
                        Match voi = vo.Match(opstr);

                        oper.Type = null;
                        oper.Name = voi.Groups[1].Value;

                        string[] args = voi.Groups[2].Value.Split(',');
                        args = args.Where(x => x != "").ToArray();
                        Array.Resize(ref oper.Arguments, args.Length);
                        for (int a = 0; a < args.Length; a++)
                        {
                            oper.Arguments[a] = Compiler.usingsContainer[args[a]];
                        }
                        oper.Length = 13 + 8 * args.Length;
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, oper) { Index = Length };
                        Length += 13 + 8 * args.Length;
                    }
                        break;
                    case OpCode.CallCSharp:
                    {
                        string opstr = str[i].Substring(str[i].IndexOf(" ") + 1);
                        CSharpVoid oper = new CSharpVoid();

                        Regex vo = new Regex("([a-zA-Z0-9]{1,})\\:([a-zA-Z0-9]{1,})\\((.*)\\)");
                        Match voi = vo.Match(opstr);

                        oper.Type = Compiler.usingsContainer[voi.Groups[1].Value];
                        oper.Name = voi.Groups[2].Value;

                        string[] args = voi.Groups[3].Value.Split(',');
                        args = args.Where(x => x != "").ToArray();
                        Array.Resize(ref oper.Arguments, args.Length);
                        for (int a = 0; a < args.Length; a++)
                        {
                            oper.Arguments[a] = Compiler.usingsContainer[args[a]];
                        }

                        oper.Length = 21 + 8 * args.Length;
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, oper) {Index = Length};
                        Length += 21 + 8 * args.Length;
                    }
                        break;
                    case OpCode.Ceq:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Cgt:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Cls:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Add:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Sub:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Mul:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Div:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Br:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, int.Parse(str[i].Substring(str[i].IndexOf(" ") + 1)) - ofs)
                            {
                                Index = Length
                            };
                        Length += 5;
                    }
                        break;
                    case OpCode.BrTrue:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, int.Parse(str[i].Substring(str[i].IndexOf(" ") + 1)) - ofs)
                            {
                                Index = Length
                            };
                        Length += 5;
                    }
                        break;
                    case OpCode.BrFalse:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, int.Parse(str[i].Substring(str[i].IndexOf(" ") + 1)) - ofs)
                            {
                                Index = Length
                            };
                        Length += 5;
                    }
                        break;
                    case OpCode.Pop:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.Dup:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    case OpCode.StGv:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.LdGv:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.StLv:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.LdLv:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, str[i].Substring(str[i].IndexOf(" ") + 1)) {Index = Length};
                        Length += 5;
                    }
                        break;
                    case OpCode.Ret:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(op, null) {Index = Length};
                        Length += 1;
                    }
                        break;
                    default:
                    {
                        Instructions[Instructions.Length - 1] =
                            new Instruction(OpCode.Nop, null) {Index = Length};
                    }
                        break;
                }
            }
        }
    }

    internal class UsingType
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

    internal class UsingContainer
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
                usings[usings.Length - 1] = new UsingType(wholelength, type.Length, name);
                wholelength += type.Length;
            }

            return usings.First(x => x.Name == name);
        }
    }

    internal class CSharpVoid
    {
        public int Length;
        public UsingType Type;
        public string Name;
        public UsingType[] Arguments;
    }

    internal class VariableType
    {
        public string Name;
        public int Index;

        public VariableType(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }

    internal class VariablesContainer
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
}
