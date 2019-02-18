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

        private void protectButton_Click(object sender, EventArgs e)
        {
            Protector protect = new Protector();
            ProtectFlag flags= ProtectFlag.None;

            if (antiDebugNativeCheckBox.Checked)
                flags |= ProtectFlag.AntiDebugNativeDef;
            if (antiManagedDebugCheckBox.Checked)
                flags |= ProtectFlag.AntiDebugManagedDef;
            if (antiDumpCheckBox.Checked)
                flags |= ProtectFlag.AntiDumpDef;
            if (methodEncryptionCheckBox.Checked)
                flags |= ProtectFlag.MethodEncryptionDef;
            if (CRC32CheckBox.Checked)
                flags |= ProtectFlag.CRC32CheckDef;
            protect.Protect(flags, x64CheckBox.Checked, (long)AntiManagedDebugNumericUpDown.Value);
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
    }
}
