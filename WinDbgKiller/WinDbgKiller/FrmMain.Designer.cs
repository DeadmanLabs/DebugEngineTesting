namespace WinDbgKiller
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboSource = new System.Windows.Forms.ComboBox();
            this.btnSelectSource = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioKernelPipe = new System.Windows.Forms.RadioButton();
            this.radioNewProcess = new System.Windows.Forms.RadioButton();
            this.radioRunningProcess = new System.Windows.Forms.RadioButton();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
            this.listView3 = new System.Windows.Forms.ListView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source: ";
            // 
            // comboSource
            // 
            this.comboSource.FormattingEnabled = true;
            this.comboSource.Location = new System.Drawing.Point(75, 10);
            this.comboSource.Name = "comboSource";
            this.comboSource.Size = new System.Drawing.Size(242, 24);
            this.comboSource.Sorted = true;
            this.comboSource.TabIndex = 1;
            this.comboSource.DropDown += new System.EventHandler(this.comboSource_DropDown);
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Location = new System.Drawing.Point(323, 10);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(38, 23);
            this.btnSelectSource.TabIndex = 2;
            this.btnSelectSource.Text = "...";
            this.btnSelectSource.UseVisualStyleBackColor = true;
            this.btnSelectSource.Click += new System.EventHandler(this.btnSelectSource_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioKernelPipe);
            this.groupBox1.Controls.Add(this.radioNewProcess);
            this.groupBox1.Controls.Add(this.radioRunningProcess);
            this.groupBox1.Location = new System.Drawing.Point(1029, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // radioKernelPipe
            // 
            this.radioKernelPipe.AutoSize = true;
            this.radioKernelPipe.Location = new System.Drawing.Point(7, 76);
            this.radioKernelPipe.Name = "radioKernelPipe";
            this.radioKernelPipe.Size = new System.Drawing.Size(97, 20);
            this.radioKernelPipe.TabIndex = 2;
            this.radioKernelPipe.Text = "Kernel Pipe";
            this.radioKernelPipe.UseVisualStyleBackColor = true;
            this.radioKernelPipe.CheckedChanged += new System.EventHandler(this.radioKernelPipe_CheckedChanged);
            // 
            // radioNewProcess
            // 
            this.radioNewProcess.AutoSize = true;
            this.radioNewProcess.Location = new System.Drawing.Point(7, 49);
            this.radioNewProcess.Name = "radioNewProcess";
            this.radioNewProcess.Size = new System.Drawing.Size(108, 20);
            this.radioNewProcess.TabIndex = 1;
            this.radioNewProcess.Text = "New Process";
            this.radioNewProcess.UseVisualStyleBackColor = true;
            this.radioNewProcess.CheckedChanged += new System.EventHandler(this.radioNewProcess_CheckedChanged);
            // 
            // radioRunningProcess
            // 
            this.radioRunningProcess.AutoSize = true;
            this.radioRunningProcess.Checked = true;
            this.radioRunningProcess.Location = new System.Drawing.Point(7, 22);
            this.radioRunningProcess.Name = "radioRunningProcess";
            this.radioRunningProcess.Size = new System.Drawing.Size(130, 20);
            this.radioRunningProcess.TabIndex = 0;
            this.radioRunningProcess.TabStop = true;
            this.radioRunningProcess.Text = "Running Process";
            this.radioRunningProcess.UseVisualStyleBackColor = true;
            this.radioRunningProcess.CheckedChanged += new System.EventHandler(this.radioRunningProcess_CheckedChanged);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(632, 227);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(597, 211);
            this.txtLog.TabIndex = 4;
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(16, 40);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(345, 49);
            this.btnLaunch.TabIndex = 5;
            this.btnLaunch.Text = "Launch";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(1154, 198);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(16, 96);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(235, 176);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // listView2
            // 
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(16, 279);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(235, 209);
            this.listView2.TabIndex = 8;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // listView3
            // 
            this.listView3.HideSelection = false;
            this.listView3.Location = new System.Drawing.Point(258, 279);
            this.listView3.Name = "listView3";
            this.listView3.Size = new System.Drawing.Size(259, 209);
            this.listView3.TabIndex = 9;
            this.listView3.UseCompatibleStateImageBehavior = false;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1239, 500);
            this.Controls.Add(this.listView3);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSelectSource);
            this.Controls.Add(this.comboSource);
            this.Controls.Add(this.label1);
            this.Name = "FrmMain";
            this.Text = "WinDbgSharp";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboSource;
        private System.Windows.Forms.Button btnSelectSource;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioKernelPipe;
        private System.Windows.Forms.RadioButton radioNewProcess;
        private System.Windows.Forms.RadioButton radioRunningProcess;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnLaunch;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ListView listView3;
    }
}

