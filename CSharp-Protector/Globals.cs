using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dnlib.DotNet;

namespace CSharp_Protector
{
    static class Globals
    {
        private static string _assemblyPath;
        private static AssemblyDef _assembly;
        private static TextBox _output;

        public static string AssemblyPath => _assemblyPath;
        public static AssemblyDef Assembly => _assembly;

        public static bool Load(string path)
        {
            try
            {
                _assemblyPath = path;
                _assembly = AssemblyDef.Load(path);
                return true;
            }
            catch (Exception)
            {
                _assemblyPath = String.Empty;
                _assembly = null;
                return false;
            }
        }

        public static void SetOutput(ref TextBox output)
        {
            _output = output;
        }

        public static void LogClear()
        {
            _output.Invoke(new Action(() => _output.Text = String.Empty ));
        }

        public static void Log(string text = default(string), bool endl = true, bool replace = false)
        {
            _output.Invoke(new Action(() =>
            {
                if (replace)
                    _output.Text = text;
                else
                    _output.Text += text + (endl ? "\r\n" : String.Empty);
            }));
        }
    }
}
