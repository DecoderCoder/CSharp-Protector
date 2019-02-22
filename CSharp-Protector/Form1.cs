using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using dnlib.DotNet.Emit;

namespace CSharp_Protector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Globals.SetOutput(ref outputTextBox);
            //Methods.Initialize();
        }

        private void Log(string text = default(string))
        {
            Globals.Log(text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openAssemblyDlg = new OpenFileDialog();
            openAssemblyDlg.Filter = "EXE|*.exe|DLL|*.dll|ALL|*.*";
            if (openAssemblyDlg.ShowDialog() == DialogResult.OK)
            {
                assemblyPathTextBox.Text = openAssemblyDlg.FileName;
            }
        }

        private void assemblyPathTextBox_TextChanged(object sender, EventArgs e)
        {
            addAssemblyButton.Enabled = File.Exists(assemblyPathTextBox.Text);
        }

        private void addAssemblyButton_Click(object sender, EventArgs e)
        {
            Globals.LogClear();
            if(Globals.Load(assemblyPathTextBox.Text))
            {
                Log("Assembly (" + Globals.Assembly + ")");
                Log();
                foreach (ModuleDef module in Globals.Assembly.Modules)
                {
                    Globals.Log("Module: " + module.Name);
                }
                Log();
                protectButton.Enabled = true;
                
            }
            else
            {
                Log("Failed to open assembly");
            }
        }

        private void protectButton_Click_1(object sender, EventArgs e)
        {
            Globals.LogClear();
            Protector protect = new Protector();

            protect.AntiDebugNativeDef = antiDebugNativeCheckBox.Checked;
            protect.AntiDebugManagedDef = antiManagedDebugCheckBox.Checked;
            protect.AntiDumpDef = antiDumpCheckBox.Checked;
            protect.MethodEncryptionDef = methodEncryptionCheckBox.Checked;
            protect.Crc32CheckDef = CRC32CheckBox.Checked;

            protect.ControlFlowB = controlFlowCheckBox.Checked;
            protect.Protect(x64CheckBox.Checked, (long)AntiManagedDebugNumericUpDown.Value);
        }
    }
}
