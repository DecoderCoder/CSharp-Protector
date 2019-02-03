using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace VM
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            VirtualMachine vm;
            if (args.Length > 0)
                vm = new VirtualMachine(File.ReadAllBytes(args[0]));
            else
                vm = new VirtualMachine(File.ReadAllBytes("vmtest"));
            vm.Run(0);
            Console.WriteLine();
            Console.WriteLine("---STACK BEGIN---");
            foreach (var s in vm.Stack)
            {
                Console.WriteLine(s.ToString());
            }
            Console.WriteLine("---STACK END---");
            Console.ReadLine();
            Console.WriteLine("---STACK END---");
        }
    }
}