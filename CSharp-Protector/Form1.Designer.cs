namespace CSharp_Protector
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.protectButton = new System.Windows.Forms.Button();
            this.addAssemblyButton = new System.Windows.Forms.Button();
            this.selectAssemblyButton = new System.Windows.Forms.Button();
            this.assemblyPathTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.AntiManagedDebugNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.CRC32CheckBox = new System.Windows.Forms.CheckBox();
            this.antiManagedDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.antiDebugNativeCheckBox = new System.Windows.Forms.CheckBox();
            this.antiDumpCheckBox = new System.Windows.Forms.CheckBox();
            this.methodEncryptionCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.x64CheckBox = new System.Windows.Forms.RadioButton();
            this.x32CheckBox = new System.Windows.Forms.RadioButton();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntiManagedDebugNumericUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(673, 589);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.outputTextBox);
            this.tabPage1.Controls.Add(this.protectButton);
            this.tabPage1.Controls.Add(this.addAssemblyButton);
            this.tabPage1.Controls.Add(this.selectAssemblyButton);
            this.tabPage1.Controls.Add(this.assemblyPathTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(665, 563);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Assemblies";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Location = new System.Drawing.Point(6, 62);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(653, 469);
            this.outputTextBox.TabIndex = 3;
            this.outputTextBox.WordWrap = false;
            // 
            // protectButton
            // 
            this.protectButton.Enabled = false;
            this.protectButton.Location = new System.Drawing.Point(6, 537);
            this.protectButton.Name = "protectButton";
            this.protectButton.Size = new System.Drawing.Size(653, 23);
            this.protectButton.TabIndex = 2;
            this.protectButton.Text = "Protect";
            this.protectButton.UseVisualStyleBackColor = true;
            this.protectButton.Click += new System.EventHandler(this.protectButton_Click);
            // 
            // addAssemblyButton
            // 
            this.addAssemblyButton.Enabled = false;
            this.addAssemblyButton.Location = new System.Drawing.Point(6, 33);
            this.addAssemblyButton.Name = "addAssemblyButton";
            this.addAssemblyButton.Size = new System.Drawing.Size(653, 23);
            this.addAssemblyButton.TabIndex = 2;
            this.addAssemblyButton.Text = "Open assembly";
            this.addAssemblyButton.UseVisualStyleBackColor = true;
            this.addAssemblyButton.Click += new System.EventHandler(this.addAssemblyButton_Click);
            // 
            // selectAssemblyButton
            // 
            this.selectAssemblyButton.Location = new System.Drawing.Point(621, 6);
            this.selectAssemblyButton.Name = "selectAssemblyButton";
            this.selectAssemblyButton.Size = new System.Drawing.Size(38, 23);
            this.selectAssemblyButton.TabIndex = 2;
            this.selectAssemblyButton.Text = "...";
            this.selectAssemblyButton.UseVisualStyleBackColor = true;
            this.selectAssemblyButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // assemblyPathTextBox
            // 
            this.assemblyPathTextBox.Location = new System.Drawing.Point(6, 8);
            this.assemblyPathTextBox.Name = "assemblyPathTextBox";
            this.assemblyPathTextBox.Size = new System.Drawing.Size(609, 20);
            this.assemblyPathTextBox.TabIndex = 1;
            this.assemblyPathTextBox.TextChanged += new System.EventHandler(this.assemblyPathTextBox_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Controls.Add(this.AntiManagedDebugNumericUpDown);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.CRC32CheckBox);
            this.tabPage2.Controls.Add(this.antiManagedDebugCheckBox);
            this.tabPage2.Controls.Add(this.antiDebugNativeCheckBox);
            this.tabPage2.Controls.Add(this.antiDumpCheckBox);
            this.tabPage2.Controls.Add(this.methodEncryptionCheckBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(665, 563);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // AntiManagedDebugNumericUpDown
            // 
            this.AntiManagedDebugNumericUpDown.Location = new System.Drawing.Point(136, 87);
            this.AntiManagedDebugNumericUpDown.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.AntiManagedDebugNumericUpDown.Name = "AntiManagedDebugNumericUpDown";
            this.AntiManagedDebugNumericUpDown.Size = new System.Drawing.Size(105, 20);
            this.AntiManagedDebugNumericUpDown.TabIndex = 2;
            this.AntiManagedDebugNumericUpDown.Value = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Native methods";
            // 
            // CRC32CheckBox
            // 
            this.CRC32CheckBox.AutoSize = true;
            this.CRC32CheckBox.Location = new System.Drawing.Point(6, 111);
            this.CRC32CheckBox.Name = "CRC32CheckBox";
            this.CRC32CheckBox.Size = new System.Drawing.Size(94, 17);
            this.CRC32CheckBox.TabIndex = 0;
            this.CRC32CheckBox.Text = "CRC32 Check";
            this.CRC32CheckBox.UseVisualStyleBackColor = true;
            // 
            // antiManagedDebugCheckBox
            // 
            this.antiManagedDebugCheckBox.AutoSize = true;
            this.antiManagedDebugCheckBox.Location = new System.Drawing.Point(6, 88);
            this.antiManagedDebugCheckBox.Name = "antiManagedDebugCheckBox";
            this.antiManagedDebugCheckBox.Size = new System.Drawing.Size(124, 17);
            this.antiManagedDebugCheckBox.TabIndex = 0;
            this.antiManagedDebugCheckBox.Text = "Anti managed debug";
            this.antiManagedDebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // antiDebugNativeCheckBox
            // 
            this.antiDebugNativeCheckBox.AutoSize = true;
            this.antiDebugNativeCheckBox.Location = new System.Drawing.Point(6, 65);
            this.antiDebugNativeCheckBox.Name = "antiDebugNativeCheckBox";
            this.antiDebugNativeCheckBox.Size = new System.Drawing.Size(109, 17);
            this.antiDebugNativeCheckBox.TabIndex = 0;
            this.antiDebugNativeCheckBox.Text = "Anti native debug";
            this.antiDebugNativeCheckBox.UseVisualStyleBackColor = true;
            // 
            // antiDumpCheckBox
            // 
            this.antiDumpCheckBox.AutoSize = true;
            this.antiDumpCheckBox.Location = new System.Drawing.Point(6, 42);
            this.antiDumpCheckBox.Name = "antiDumpCheckBox";
            this.antiDumpCheckBox.Size = new System.Drawing.Size(73, 17);
            this.antiDumpCheckBox.TabIndex = 0;
            this.antiDumpCheckBox.Text = "Anti dump";
            this.antiDumpCheckBox.UseVisualStyleBackColor = true;
            // 
            // methodEncryptionCheckBox
            // 
            this.methodEncryptionCheckBox.AutoSize = true;
            this.methodEncryptionCheckBox.Location = new System.Drawing.Point(6, 19);
            this.methodEncryptionCheckBox.Name = "methodEncryptionCheckBox";
            this.methodEncryptionCheckBox.Size = new System.Drawing.Size(115, 17);
            this.methodEncryptionCheckBox.TabIndex = 0;
            this.methodEncryptionCheckBox.Text = "Method Encryption";
            this.methodEncryptionCheckBox.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.x64CheckBox);
            this.panel1.Controls.Add(this.x32CheckBox);
            this.panel1.Location = new System.Drawing.Point(125, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(120, 17);
            this.panel1.TabIndex = 3;
            // 
            // x64CheckBox
            // 
            this.x64CheckBox.AutoSize = true;
            this.x64CheckBox.Checked = true;
            this.x64CheckBox.Location = new System.Drawing.Point(69, 0);
            this.x64CheckBox.Name = "x64CheckBox";
            this.x64CheckBox.Size = new System.Drawing.Size(51, 17);
            this.x64CheckBox.TabIndex = 4;
            this.x64CheckBox.TabStop = true;
            this.x64CheckBox.Text = "64-bit";
            this.x64CheckBox.UseVisualStyleBackColor = true;
            // 
            // x32CheckBox
            // 
            this.x32CheckBox.AutoSize = true;
            this.x32CheckBox.Location = new System.Drawing.Point(0, 0);
            this.x32CheckBox.Name = "x32CheckBox";
            this.x32CheckBox.Size = new System.Drawing.Size(51, 17);
            this.x32CheckBox.TabIndex = 5;
            this.x32CheckBox.Text = "32-bit";
            this.x32CheckBox.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 613);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "CSharp Protector";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntiManagedDebugNumericUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button addAssemblyButton;
        private System.Windows.Forms.Button selectAssemblyButton;
        private System.Windows.Forms.TextBox assemblyPathTextBox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button protectButton;
        private System.Windows.Forms.CheckBox methodEncryptionCheckBox;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox antiDumpCheckBox;
        private System.Windows.Forms.CheckBox CRC32CheckBox;
        private System.Windows.Forms.CheckBox antiDebugNativeCheckBox;
        private System.Windows.Forms.NumericUpDown AntiManagedDebugNumericUpDown;
        private System.Windows.Forms.CheckBox antiManagedDebugCheckBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton x64CheckBox;
        private System.Windows.Forms.RadioButton x32CheckBox;
    }
}

