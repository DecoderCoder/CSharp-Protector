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

        private void button1_Click(object sender, EventArgs e)
        {
            Compiler.Compile(richTextBox1.Text, textBox2.Text);
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

            Regex stldvar = new Regex(@"(?:Ld..|St..) (.*)\n");
            MatchCollection stldvarm = stldvar.Matches(text);
            foreach (Match match in stldvarm)
            {
                richTextBox1.Select(match.Groups[1].Index, match.Groups[1].Length);
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
