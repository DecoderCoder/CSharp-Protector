using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VM
{
    internal unsafe class VirtualMachine
    {
        public object[] Stack = new object[0];
        public IntPtr MemoryPointer { get; set; }
        public int ImageSize { get; set; }
        public int CodeSectionOffset { get; set; }
        public int TextSectionOffset { get; set; }
        public int CSharpTypesSectionOffset { get; set; }
        public int ClearSectionOffset { get; set; }
        public object[] GlobalVariables = new object[512];

        public VirtualMachine(byte[] b)
        {
            ImageSize = b.Length;
            MemoryPointer = Marshal.AllocHGlobal(int.MaxValue);
            Marshal.Copy(b, 0, MemoryPointer, b.Length);

            CodeSectionOffset = Marshal.ReadInt32(MemoryPointer, 0);
            TextSectionOffset = Marshal.ReadInt32(MemoryPointer, 8);
            CSharpTypesSectionOffset = Marshal.ReadInt32(MemoryPointer, 16);
            ClearSectionOffset = CodeSectionOffset + TextSectionOffset + CSharpTypesSectionOffset;
            MemoryPointer = IntPtr.Add(MemoryPointer, 24);
        }

        private object Pop()
        {
            if (Stack.Length > 0)
            {
                object rez = Stack[Stack.Length - 1];
                Array.Resize(ref Stack, Stack.Length - 1);
                return rez;
            }
            return null;
        }

        public void Push(object val)
        {
            Array.Resize(ref Stack, Stack.Length + 1);
            Stack[Stack.Length - 1] = val;
        }

        public void Run(int Offset)
        {
            object[] LocalVariables = new object[512];
            while (true)
            {
                byte curByte = Marshal.ReadByte(IntPtr.Add(MemoryPointer, CodeSectionOffset + Offset++));

                switch (curByte)
                {
                    case 0x50: //LdStr
                        {
                            byte[] str = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 4)];
                            Marshal.Copy(
                                IntPtr.Add(MemoryPointer,
                                    TextSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset)), str,
                                0, str.Length);
                            Offset += 8;
                            Push(Encoding.UTF8.GetString(str));
                            break;
                        }
                    case 0x51: //LdInt
                        {
                            Push(Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset));
                            Offset += 4;
                            break;
                        }
                    case 0x52: //LdByte
                    {
                        Push(Marshal.ReadByte(MemoryPointer, CodeSectionOffset + Offset));
                        Offset += 1;
                        break;
                    }
                    case 0x54: //Newobj
                    {
                        byte[] temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 4)];
                        Marshal.Copy(
                            IntPtr.Add(MemoryPointer,
                                CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer, Offset)), temp, 0,
                            temp.Length);
                        Type objType = Type.GetType(Encoding.UTF8.GetString(temp));
                        Push(Activator.CreateInstance(objType));
                        Offset += 8;
                        break;
                    }
                    case 0x70: //Newarr
                    {
                        int length = (int)Pop();
                        byte[] temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 4)];
                        Marshal.Copy(
                            IntPtr.Add(MemoryPointer,
                                CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer, Offset)), temp, 0,
                            temp.Length);
                        Type arrType = Type.GetType(Encoding.UTF8.GetString(temp)).MakeArrayType();
                        Push(Activator.CreateInstance(arrType, length));
                        Offset += 8;
                        break;
                    }
                    case 0x71: //Setarr
                    {
                        object value = Pop();
                        int index = (int)Pop();
                        object[] arr = (object[])Pop();
                        arr[index] = value;
                        Push(arr);
                        break;
                    }
                    case 0x72: //Getarr
                    {
                        int index = (int)Pop();
                        object[] arr = (object[])Pop();
                        Push(arr[index]);
                        break;
                    }
                    case 0x60: // Br
                        {
                            Offset = CodeSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                        }
                        break;
                    case 0x61: // BrTrue
                        {
                            if ((int)Pop() == 1)
                                Offset = CodeSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                        }
                        break;
                    case 0x62: // BrFalse
                        {
                            if ((int)Pop() == 0)
                                Offset = CodeSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                        }
                        break;
                    case 0x40: //Call C# Method
                        {
                            Run(Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset));
                            Offset += 4;
                            break;
                        }
                    case 0x41: //Call C# Method
                        {
                            byte[] temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 4)];
                            Marshal.Copy(
                                IntPtr.Add(MemoryPointer,
                                    CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer, Offset)), temp, 0,
                                temp.Length);
                            string type = Encoding.UTF8.GetString(temp);
                            temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 12)];
                            Marshal.Copy(
                                IntPtr.Add(MemoryPointer,
                                    TextSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 8)),
                                temp, 0, temp.Length);
                            string voidname = Encoding.UTF8.GetString(temp);
                            int argcount = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 16);
                            Type[] types = new Type[argcount];
                            object[] arguments = new object[argcount];
                            for (int i = argcount - 1; i > -1; i--)
                            {
                                temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 20 + i * 8)];

                                Marshal.Copy(
                                    IntPtr.Add(MemoryPointer,
                                        CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer,
                                            CodeSectionOffset + Offset + 24 + i * 8)), temp, 0, temp.Length);
                                types[i] = Type.GetType(Encoding.UTF8.GetString(temp));
                                arguments[i] = Pop();
                            }

                            var method = Type.GetType(type).GetMethod(voidname, types);

                            var rez = method.Invoke(null, arguments);
                            if (rez != null)
                            {
                                Push(rez);
                            }

                            Offset += 20 + argcount * 8;
                            break;
                        }
                    case 0x42: //CallVirt C# Method
                    {
                        object obj = Pop();
                        Type type = obj.GetType();
                        byte[] temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 4)];
                        Marshal.Copy(
                            IntPtr.Add(MemoryPointer,
                                TextSectionOffset + Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset)), temp, 0, temp.Length);
                        string voidname = Encoding.UTF8.GetString(temp);
                        int argcount = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 8);
                        Type[] types = new Type[argcount];
                        object[] arguments = new object[argcount];
                        for (int i = argcount - 1; i > -1; i--)
                        {
                            temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 12 + i * 8)];

                            Marshal.Copy(
                                IntPtr.Add(MemoryPointer,
                                    CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer,
                                        CodeSectionOffset + Offset + 16 + i * 8)), temp, 0, temp.Length);
                            types[i] = Type.GetType(Encoding.UTF8.GetString(temp));
                            arguments[i] = Pop();
                        }

                        var method = type.GetMethod(voidname, types);

                        var rez = method.Invoke(obj, arguments);
                        if (rez != null)
                        {
                            Push(rez);
                        }

                        Offset += 12 + argcount * 8;
                        break;
                    }
                    case 0xA1: // Ceq
                    {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            if (val1 == null || val2 == null)
                                Push(0);
                            if (val1 == val2)
                                Push(1);
                            else
                                Push(0);
                            break;
                        }
                    case 0xA2: // Cgt
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = (float)Pop();
                            if (val1 > val2)
                                Push(1);
                            else
                                Push(0);
                            break;
                        }
                    case 0xA3: // Cls
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            if (val1 < val2)
                                Push(1);
                            else
                                Push(0);
                            break;
                        }
                    case 0xB1: // Add
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            Push(val1 + val2);
                            break;
                        }
                    case 0xB2: // Sub
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            Push(val1 - val2);
                            break;
                        }
                    case 0xB3: // Mul
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            Push(val1 * val2);
                            break;
                        }
                    case 0xB4: // Div
                        {
                            dynamic val2 = Pop();
                            dynamic val1 = Pop();
                            Push(val1 / val2);
                            break;
                        }
                    case 0x91: //Pop
                        {
                            Pop();
                            break;
                        }
                    case 0x92: //Dup
                        {
                            Push(Stack[Stack.Length - 1]);
                            break;
                        }
                    case 0x93: //Push Top stack object to Global Variables
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            GlobalVariables[index] = Pop();
                            break;
                        }
                    case 0x94: // Load Global Variable to Stack
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            Push(GlobalVariables[index]);
                            break;
                        }
                    case 0x95: //Push Top stack object to Local Variables
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            LocalVariables[index] = Pop();
                            break;
                        }
                    case 0x96: // Load Local Variable to Stack
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            Push(LocalVariables[index]);
                            break;
                        }
                    case 0x22:
                        {
                            return;
                            break;
                        }
                }
            }
        }
    }
}
