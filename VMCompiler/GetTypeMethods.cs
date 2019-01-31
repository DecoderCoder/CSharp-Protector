using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VMCompiler
{
    public partial class GetTypeMethods : Form
    {
        public GetTypeMethods()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            Type type = Type.GetType(textBox1.Text);
            foreach (var method in type.GetMethods())
            {
                if (method.Name.Contains(textBox3.Text))
                {
                    textBox2.Text += type.Name + ":" + method.Name + "(" + String.Join(",", method.GetParameters().Select(x => x.ParameterType.Name)) + ")" + "\r\n";
                }
            }
        }
    }
}
