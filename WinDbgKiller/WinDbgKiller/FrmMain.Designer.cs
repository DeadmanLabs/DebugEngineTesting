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
            this.listRegisters = new System.Windows.Forms.ListView();
            this.columnRegister = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnRegisterValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listStack = new System.Windows.Forms.ListView();
            this.columnFunction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listThreads = new System.Windows.Forms.ListView();
            this.columnThreadId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnError = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listBreakpoints = new System.Windows.Forms.ListView();
            this.columnOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnInstruction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.labelState = new System.Windows.Forms.Label();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source: ";
            // 
            // comboSource
            // 
            this.comboSource.FormattingEnabled = true;
            this.comboSource.Location = new System.Drawing.Point(56, 8);
            this.comboSource.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboSource.Name = "comboSource";
            this.comboSource.Size = new System.Drawing.Size(182, 21);
            this.comboSource.Sorted = true;
            this.comboSource.TabIndex = 1;
            this.comboSource.DropDown += new System.EventHandler(this.comboSource_DropDown);
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Location = new System.Drawing.Point(242, 8);
            this.btnSelectSource.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(28, 19);
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
            this.groupBox1.Location = new System.Drawing.Point(772, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(150, 81);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // radioKernelPipe
            // 
            this.radioKernelPipe.AutoSize = true;
            this.radioKernelPipe.Location = new System.Drawing.Point(5, 62);
            this.radioKernelPipe.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioKernelPipe.Name = "radioKernelPipe";
            this.radioKernelPipe.Size = new System.Drawing.Size(79, 17);
            this.radioKernelPipe.TabIndex = 2;
            this.radioKernelPipe.Text = "Kernel Pipe";
            this.radioKernelPipe.UseVisualStyleBackColor = true;
            this.radioKernelPipe.CheckedChanged += new System.EventHandler(this.radioKernelPipe_CheckedChanged);
            // 
            // radioNewProcess
            // 
            this.radioNewProcess.AutoSize = true;
            this.radioNewProcess.Location = new System.Drawing.Point(5, 40);
            this.radioNewProcess.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioNewProcess.Name = "radioNewProcess";
            this.radioNewProcess.Size = new System.Drawing.Size(88, 17);
            this.radioNewProcess.TabIndex = 1;
            this.radioNewProcess.Text = "New Process";
            this.radioNewProcess.UseVisualStyleBackColor = true;
            this.radioNewProcess.CheckedChanged += new System.EventHandler(this.radioNewProcess_CheckedChanged);
            // 
            // radioRunningProcess
            // 
            this.radioRunningProcess.AutoSize = true;
            this.radioRunningProcess.Checked = true;
            this.radioRunningProcess.Location = new System.Drawing.Point(5, 18);
            this.radioRunningProcess.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioRunningProcess.Name = "radioRunningProcess";
            this.radioRunningProcess.Size = new System.Drawing.Size(106, 17);
            this.radioRunningProcess.TabIndex = 0;
            this.radioRunningProcess.TabStop = true;
            this.radioRunningProcess.Text = "Running Process";
            this.radioRunningProcess.UseVisualStyleBackColor = true;
            this.radioRunningProcess.CheckedChanged += new System.EventHandler(this.radioRunningProcess_CheckedChanged);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 403);
            this.txtLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(909, 187);
            this.txtLog.TabIndex = 4;
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(12, 32);
            this.btnLaunch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(259, 40);
            this.btnLaunch.TabIndex = 5;
            this.btnLaunch.Text = "Launch";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(275, 54);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(68, 19);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // listRegisters
            // 
            this.listRegisters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnRegister,
            this.columnRegisterValue});
            this.listRegisters.HideSelection = false;
            this.listRegisters.Location = new System.Drawing.Point(12, 78);
            this.listRegisters.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listRegisters.Name = "listRegisters";
            this.listRegisters.Size = new System.Drawing.Size(177, 144);
            this.listRegisters.TabIndex = 7;
            this.listRegisters.UseCompatibleStateImageBehavior = false;
            this.listRegisters.View = System.Windows.Forms.View.Details;
            // 
            // columnRegister
            // 
            this.columnRegister.Text = "Register";
            this.columnRegister.Width = 90;
            // 
            // columnRegisterValue
            // 
            this.columnRegisterValue.Text = "Value";
            this.columnRegisterValue.Width = 138;
            // 
            // listStack
            // 
            this.listStack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFunction,
            this.columnParameters});
            this.listStack.HideSelection = false;
            this.listStack.Location = new System.Drawing.Point(12, 227);
            this.listStack.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listStack.Name = "listStack";
            this.listStack.Size = new System.Drawing.Size(458, 171);
            this.listStack.TabIndex = 8;
            this.listStack.UseCompatibleStateImageBehavior = false;
            this.listStack.View = System.Windows.Forms.View.Details;
            // 
            // columnFunction
            // 
            this.columnFunction.Text = "Function";
            this.columnFunction.Width = 91;
            // 
            // columnParameters
            // 
            this.columnParameters.Text = "Parameters";
            this.columnParameters.Width = 402;
            // 
            // listThreads
            // 
            this.listThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnThreadId,
            this.columnEntry,
            this.columnError,
            this.columnStatus});
            this.listThreads.HideSelection = false;
            this.listThreads.Location = new System.Drawing.Point(193, 77);
            this.listThreads.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listThreads.Name = "listThreads";
            this.listThreads.Size = new System.Drawing.Size(278, 145);
            this.listThreads.TabIndex = 9;
            this.listThreads.UseCompatibleStateImageBehavior = false;
            this.listThreads.View = System.Windows.Forms.View.Details;
            // 
            // columnThreadId
            // 
            this.columnThreadId.Text = "ID";
            // 
            // columnEntry
            // 
            this.columnEntry.Text = "Entry";
            this.columnEntry.Width = 110;
            // 
            // columnError
            // 
            this.columnError.Text = "Last Error";
            this.columnError.Width = 113;
            // 
            // columnStatus
            // 
            this.columnStatus.Text = "Status";
            this.columnStatus.Width = 82;
            // 
            // listBreakpoints
            // 
            this.listBreakpoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnOffset,
            this.columnInstruction,
            this.columnAction,
            this.columnComment});
            this.listBreakpoints.HideSelection = false;
            this.listBreakpoints.Location = new System.Drawing.Point(474, 227);
            this.listBreakpoints.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBreakpoints.Name = "listBreakpoints";
            this.listBreakpoints.Size = new System.Drawing.Size(447, 171);
            this.listBreakpoints.TabIndex = 10;
            this.listBreakpoints.UseCompatibleStateImageBehavior = false;
            this.listBreakpoints.View = System.Windows.Forms.View.Details;
            // 
            // columnOffset
            // 
            this.columnOffset.Text = "Offset";
            this.columnOffset.Width = 61;
            // 
            // columnInstruction
            // 
            this.columnInstruction.Text = "Instruction";
            this.columnInstruction.Width = 71;
            // 
            // columnAction
            // 
            this.columnAction.Text = "Action";
            this.columnAction.Width = 146;
            // 
            // columnComment
            // 
            this.columnComment.Text = "Comment";
            this.columnComment.Width = 153;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(275, 11);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "State:";
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Location = new System.Drawing.Point(315, 11);
            this.labelState.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(33, 13);
            this.labelState.TabIndex = 12;
            this.labelState.Text = "Idle...";
            // 
            // btnContinue
            // 
            this.btnContinue.Location = new System.Drawing.Point(276, 32);
            this.btnContinue.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(68, 19);
            this.btnContinue.TabIndex = 13;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(347, 54);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(77, 19);
            this.btnCheck.TabIndex = 14;
            this.btnCheck.Text = "Check Status";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(384, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Status:";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(424, 11);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(33, 13);
            this.labelStatus.TabIndex = 16;
            this.labelStatus.Text = "Idle...";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 599);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.btnContinue);
            this.Controls.Add(this.labelState);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBreakpoints);
            this.Controls.Add(this.listThreads);
            this.Controls.Add(this.listStack);
            this.Controls.Add(this.listRegisters);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSelectSource);
            this.Controls.Add(this.comboSource);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmMain";
            this.Text = "WinDbgSharp";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
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
        private System.Windows.Forms.ListView listRegisters;
        private System.Windows.Forms.ListView listStack;
        private System.Windows.Forms.ListView listThreads;
        private System.Windows.Forms.ColumnHeader columnRegister;
        private System.Windows.Forms.ColumnHeader columnRegisterValue;
        private System.Windows.Forms.ColumnHeader columnFunction;
        private System.Windows.Forms.ColumnHeader columnParameters;
        private System.Windows.Forms.ColumnHeader columnThreadId;
        private System.Windows.Forms.ColumnHeader columnEntry;
        private System.Windows.Forms.ColumnHeader columnError;
        private System.Windows.Forms.ColumnHeader columnStatus;
        private System.Windows.Forms.ListView listBreakpoints;
        private System.Windows.Forms.ColumnHeader columnOffset;
        private System.Windows.Forms.ColumnHeader columnInstruction;
        private System.Windows.Forms.ColumnHeader columnAction;
        private System.Windows.Forms.ColumnHeader columnComment;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelStatus;
    }
}

