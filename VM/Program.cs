using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VM
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            VirtualMachine vm = new VirtualMachine(File.ReadAllBytes("vmtest"));
            //Console.WriteLine(String.Join("-", BitConverter.GetBytes((int)120).Select(x => x.ToString("x2"))));
            vm.Run(0);

            Console.WriteLine("");
            Console.WriteLine("---STACK BEGIN---");
            foreach (var s in vm.Stack)
            {
                Console.WriteLine(s.ToString());
            }
            Console.WriteLine("---STACK END---");
            Console.ReadLine();
            
        }
    }

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
            Console.WriteLine("VM Size: " + int.MaxValue);
            Console.WriteLine("Image Size: " + ImageSize);
        }

        private object pop()
        {
            object rez = Stack[Stack.Length - 1];
            Array.Resize(ref Stack, Stack.Length - 1);
            return rez;
        }

        private void push(object val)
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
                        push(Encoding.UTF8.GetString(str));
                        break;
                    }
                    case 0x51: //LdInt
                    {
                        push(Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset));
                        Offset += 4;
                        break;
                    }
                    case 0x52: //LdInt
                    {
                        push(Marshal.ReadByte(MemoryPointer, CodeSectionOffset + Offset));
                        Offset += 1;
                        break;
                    }
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
                        for (int i = 0; i < argcount; i++)
                        {
                            Console.WriteLine(CodeSectionOffset + Offset + 20 + i * 4);
                            temp = new byte[Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset + 20 + i * 4)];
                            Marshal.Copy(
                                IntPtr.Add(MemoryPointer,
                                    CSharpTypesSectionOffset + Marshal.ReadInt32(MemoryPointer,
                                        CodeSectionOffset + Offset + 24 + i * 4)), temp, 0, temp.Length);
                            types[i] = Type.GetType(Encoding.UTF8.GetString(temp));
                            arguments[i] = pop();
                        }

                        var method = Type.GetType(type).GetMethod(voidname, types);

                        var rez = method.Invoke(null, arguments);
                        if (rez != null)
                        {
                            push(rez);
                        }

                        Offset += 20 + argcount * 8;
                        break;
                    }
                    case 0x91: //Pop
                    {
                        pop();
                        break;
                    }
                    case 0x93: //Push Top stack object to Global Variables
                        {
                        int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                        Offset += 4;
                        GlobalVariables[index] = pop();
                        break;
                    }
                    case 0x94: // Load Global Variable to Stack
                        {
                        int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                        Offset += 4;
                        push(GlobalVariables[index]);
                        break;
                    }
                    case 0x95: //Push Top stack object to Local Variables
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            LocalVariables[index] = pop();
                            break;
                        }
                    case 0x96: // Load Local Variable to Stack
                        {
                            int index = Marshal.ReadInt32(MemoryPointer, CodeSectionOffset + Offset);
                            Offset += 4;
                            push(LocalVariables[index]);
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