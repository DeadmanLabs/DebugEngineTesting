namespace WinDbgKiller
{
    partial class FrmAlt
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
            this.groupDebuggee = new System.Windows.Forms.GroupBox();
            this.groupRegisters = new System.Windows.Forms.GroupBox();
            this.groupThreads = new System.Windows.Forms.GroupBox();
            this.groupBreakpoints = new System.Windows.Forms.GroupBox();
            this.comboProcesses = new System.Windows.Forms.ComboBox();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnDetach = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.listRegisters = new System.Windows.Forms.ListView();
            this.columnReg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnVal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupStack = new System.Windows.Forms.GroupBox();
            this.groupModules = new System.Windows.Forms.GroupBox();
            this.listBreakpoints = new System.Windows.Forms.ListView();
            this.columnOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnInstruction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listThreads = new System.Windows.Forms.ListView();
            this.columnID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnLastError = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.treeModules = new System.Windows.Forms.TreeView();
            this.columnSelected = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupStatus = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelState = new System.Windows.Forms.Label();
            this.listEngineFlags = new System.Windows.Forms.ListView();
            this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listStack = new System.Windows.Forms.ListView();
            this.columnFunc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupDebuggee.SuspendLayout();
            this.groupRegisters.SuspendLayout();
            this.groupThreads.SuspendLayout();
            this.groupBreakpoints.SuspendLayout();
            this.groupStack.SuspendLayout();
            this.groupModules.SuspendLayout();
            this.groupStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source:";
            // 
            // groupDebuggee
            // 
            this.groupDebuggee.Controls.Add(this.btnDetach);
            this.groupDebuggee.Controls.Add(this.btnAttach);
            this.groupDebuggee.Controls.Add(this.btnLoadFile);
            this.groupDebuggee.Controls.Add(this.comboProcesses);
            this.groupDebuggee.Controls.Add(this.label1);
            this.groupDebuggee.Location = new System.Drawing.Point(12, 12);
            this.groupDebuggee.Name = "groupDebuggee";
            this.groupDebuggee.Size = new System.Drawing.Size(285, 96);
            this.groupDebuggee.TabIndex = 1;
            this.groupDebuggee.TabStop = false;
            this.groupDebuggee.Text = "Debuggee";
            // 
            // groupRegisters
            // 
            this.groupRegisters.Controls.Add(this.listRegisters);
            this.groupRegisters.Location = new System.Drawing.Point(12, 114);
            this.groupRegisters.Name = "groupRegisters";
            this.groupRegisters.Size = new System.Drawing.Size(285, 274);
            this.groupRegisters.TabIndex = 2;
            this.groupRegisters.TabStop = false;
            this.groupRegisters.Text = "Registers";
            // 
            // groupThreads
            // 
            this.groupThreads.Controls.Add(this.listThreads);
            this.groupThreads.Location = new System.Drawing.Point(913, 221);
            this.groupThreads.Name = "groupThreads";
            this.groupThreads.Size = new System.Drawing.Size(386, 167);
            this.groupThreads.TabIndex = 2;
            this.groupThreads.TabStop = false;
            this.groupThreads.Text = "Threads";
            // 
            // groupBreakpoints
            // 
            this.groupBreakpoints.Controls.Add(this.listBreakpoints);
            this.groupBreakpoints.Location = new System.Drawing.Point(620, 394);
            this.groupBreakpoints.Name = "groupBreakpoints";
            this.groupBreakpoints.Size = new System.Drawing.Size(679, 197);
            this.groupBreakpoints.TabIndex = 2;
            this.groupBreakpoints.TabStop = false;
            this.groupBreakpoints.Text = "Breakpoints";
            // 
            // comboProcesses
            // 
            this.comboProcesses.FormattingEnabled = true;
            this.comboProcesses.Location = new System.Drawing.Point(56, 13);
            this.comboProcesses.Name = "comboProcesses";
            this.comboProcesses.Size = new System.Drawing.Size(164, 21);
            this.comboProcesses.TabIndex = 1;
            this.comboProcesses.DropDown += new System.EventHandler(this.comboProcesses_DropDown);
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(9, 40);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(270, 23);
            this.btnLoadFile.TabIndex = 3;
            this.btnLoadFile.Text = "Load From File";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // btnAttach
            // 
            this.btnAttach.Location = new System.Drawing.Point(226, 11);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(53, 23);
            this.btnAttach.TabIndex = 4;
            this.btnAttach.Text = "Attach";
            this.btnAttach.UseVisualStyleBackColor = true;
            // 
            // btnDetach
            // 
            this.btnDetach.Location = new System.Drawing.Point(9, 67);
            this.btnDetach.Name = "btnDetach";
            this.btnDetach.Size = new System.Drawing.Size(270, 23);
            this.btnDetach.TabIndex = 5;
            this.btnDetach.Text = "Debug";
            this.btnDetach.UseVisualStyleBackColor = true;
            this.btnDetach.Click += new System.EventHandler(this.btnDetach_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(12, 394);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(602, 197);
            this.txtOutput.TabIndex = 3;
            // 
            // listRegisters
            // 
            this.listRegisters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnReg,
            this.columnVal});
            this.listRegisters.HideSelection = false;
            this.listRegisters.Location = new System.Drawing.Point(9, 19);
            this.listRegisters.Name = "listRegisters";
            this.listRegisters.Size = new System.Drawing.Size(270, 249);
            this.listRegisters.TabIndex = 0;
            this.listRegisters.UseCompatibleStateImageBehavior = false;
            this.listRegisters.View = System.Windows.Forms.View.Details;
            // 
            // columnReg
            // 
            this.columnReg.Text = "Register";
            this.columnReg.Width = 121;
            // 
            // columnVal
            // 
            this.columnVal.Text = "Value";
            this.columnVal.Width = 145;
            // 
            // groupStack
            // 
            this.groupStack.Controls.Add(this.listStack);
            this.groupStack.Location = new System.Drawing.Point(913, 12);
            this.groupStack.Name = "groupStack";
            this.groupStack.Size = new System.Drawing.Size(386, 203);
            this.groupStack.TabIndex = 3;
            this.groupStack.TabStop = false;
            this.groupStack.Text = "Stack";
            // 
            // groupModules
            // 
            this.groupModules.Controls.Add(this.treeModules);
            this.groupModules.Location = new System.Drawing.Point(304, 12);
            this.groupModules.Name = "groupModules";
            this.groupModules.Size = new System.Drawing.Size(310, 376);
            this.groupModules.TabIndex = 4;
            this.groupModules.TabStop = false;
            this.groupModules.Text = "Modules";
            // 
            // listBreakpoints
            // 
            this.listBreakpoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnOffset,
            this.columnInstruction,
            this.columnAction,
            this.columnEnabled,
            this.columnComment});
            this.listBreakpoints.HideSelection = false;
            this.listBreakpoints.Location = new System.Drawing.Point(7, 20);
            this.listBreakpoints.Name = "listBreakpoints";
            this.listBreakpoints.Size = new System.Drawing.Size(666, 171);
            this.listBreakpoints.TabIndex = 0;
            this.listBreakpoints.UseCompatibleStateImageBehavior = false;
            this.listBreakpoints.View = System.Windows.Forms.View.Details;
            // 
            // columnOffset
            // 
            this.columnOffset.Text = "Offset";
            this.columnOffset.Width = 126;
            // 
            // columnInstruction
            // 
            this.columnInstruction.Text = "Instruction";
            this.columnInstruction.Width = 155;
            // 
            // columnAction
            // 
            this.columnAction.Text = "Action";
            this.columnAction.Width = 77;
            // 
            // columnComment
            // 
            this.columnComment.Text = "Comment";
            this.columnComment.Width = 244;
            // 
            // listThreads
            // 
            this.listThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnSelected,
            this.columnID,
            this.columnEntry,
            this.columnLastError});
            this.listThreads.HideSelection = false;
            this.listThreads.Location = new System.Drawing.Point(7, 20);
            this.listThreads.Name = "listThreads";
            this.listThreads.Size = new System.Drawing.Size(373, 141);
            this.listThreads.TabIndex = 0;
            this.listThreads.UseCompatibleStateImageBehavior = false;
            this.listThreads.View = System.Windows.Forms.View.Details;
            // 
            // columnID
            // 
            this.columnID.Text = "ID";
            this.columnID.Width = 93;
            // 
            // columnEntry
            // 
            this.columnEntry.Text = "Entry";
            this.columnEntry.Width = 100;
            // 
            // columnLastError
            // 
            this.columnLastError.Text = "Last Error";
            this.columnLastError.Width = 127;
            // 
            // treeModules
            // 
            this.treeModules.CheckBoxes = true;
            this.treeModules.Location = new System.Drawing.Point(7, 13);
            this.treeModules.Name = "treeModules";
            this.treeModules.Size = new System.Drawing.Size(297, 357);
            this.treeModules.TabIndex = 0;
            // 
            // columnSelected
            // 
            this.columnSelected.Text = "Active";
            this.columnSelected.Width = 49;
            // 
            // groupStatus
            // 
            this.groupStatus.Controls.Add(this.listEngineFlags);
            this.groupStatus.Controls.Add(this.labelState);
            this.groupStatus.Controls.Add(this.labelStatus);
            this.groupStatus.Controls.Add(this.label4);
            this.groupStatus.Controls.Add(this.label3);
            this.groupStatus.Controls.Add(this.label2);
            this.groupStatus.Location = new System.Drawing.Point(620, 12);
            this.groupStatus.Name = "groupStatus";
            this.groupStatus.Size = new System.Drawing.Size(287, 376);
            this.groupStatus.TabIndex = 5;
            this.groupStatus.TabStop = false;
            this.groupStatus.Text = "Status";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Status: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "State: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Engine:";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(56, 21);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(33, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Idle...";
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Location = new System.Drawing.Point(56, 40);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(33, 13);
            this.labelState.TabIndex = 4;
            this.labelState.Text = "Idle...";
            // 
            // listEngineFlags
            // 
            this.listEngineFlags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnKey,
            this.columnValue});
            this.listEngineFlags.HideSelection = false;
            this.listEngineFlags.Location = new System.Drawing.Point(10, 83);
            this.listEngineFlags.Name = "listEngineFlags";
            this.listEngineFlags.Size = new System.Drawing.Size(271, 105);
            this.listEngineFlags.TabIndex = 5;
            this.listEngineFlags.UseCompatibleStateImageBehavior = false;
            this.listEngineFlags.View = System.Windows.Forms.View.Details;
            // 
            // columnKey
            // 
            this.columnKey.Text = "Flag";
            this.columnKey.Width = 136;
            // 
            // columnValue
            // 
            this.columnValue.Text = "Arugment";
            this.columnValue.Width = 131;
            // 
            // listStack
            // 
            this.listStack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFunc,
            this.columnParameters});
            this.listStack.HideSelection = false;
            this.listStack.Location = new System.Drawing.Point(6, 19);
            this.listStack.Name = "listStack";
            this.listStack.Size = new System.Drawing.Size(374, 178);
            this.listStack.TabIndex = 0;
            this.listStack.UseCompatibleStateImageBehavior = false;
            this.listStack.View = System.Windows.Forms.View.Details;
            // 
            // columnFunc
            // 
            this.columnFunc.Text = "Function";
            this.columnFunc.Width = 108;
            // 
            // columnParameters
            // 
            this.columnParameters.Text = "Parameters";
            this.columnParameters.Width = 260;
            // 
            // columnEnabled
            // 
            this.columnEnabled.Text = "Enabled";
            this.columnEnabled.Width = 58;
            // 
            // FrmAlt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1311, 603);
            this.Controls.Add(this.groupStatus);
            this.Controls.Add(this.groupModules);
            this.Controls.Add(this.groupStack);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.groupBreakpoints);
            this.Controls.Add(this.groupThreads);
            this.Controls.Add(this.groupRegisters);
            this.Controls.Add(this.groupDebuggee);
            this.Name = "FrmAlt";
            this.Text = "DeadDebug";
            this.Load += new System.EventHandler(this.FrmAlt_Load);
            this.groupDebuggee.ResumeLayout(false);
            this.groupDebuggee.PerformLayout();
            this.groupRegisters.ResumeLayout(false);
            this.groupThreads.ResumeLayout(false);
            this.groupBreakpoints.ResumeLayout(false);
            this.groupStack.ResumeLayout(false);
            this.groupModules.ResumeLayout(false);
            this.groupStatus.ResumeLayout(false);
            this.groupStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupDebuggee;
        private System.Windows.Forms.ComboBox comboProcesses;
        private System.Windows.Forms.GroupBox groupRegisters;
        private System.Windows.Forms.GroupBox groupThreads;
        private System.Windows.Forms.GroupBox groupBreakpoints;
        private System.Windows.Forms.Button btnDetach;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.ListView listRegisters;
        private System.Windows.Forms.ColumnHeader columnReg;
        private System.Windows.Forms.ColumnHeader columnVal;
        private System.Windows.Forms.GroupBox groupStack;
        private System.Windows.Forms.GroupBox groupModules;
        private System.Windows.Forms.ListView listBreakpoints;
        private System.Windows.Forms.ColumnHeader columnOffset;
        private System.Windows.Forms.ColumnHeader columnInstruction;
        private System.Windows.Forms.ColumnHeader columnAction;
        private System.Windows.Forms.ColumnHeader columnComment;
        private System.Windows.Forms.ListView listThreads;
        private System.Windows.Forms.ColumnHeader columnID;
        private System.Windows.Forms.ColumnHeader columnEntry;
        private System.Windows.Forms.ColumnHeader columnLastError;
        private System.Windows.Forms.TreeView treeModules;
        private System.Windows.Forms.ColumnHeader columnSelected;
        private System.Windows.Forms.GroupBox groupStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listEngineFlags;
        private System.Windows.Forms.ColumnHeader columnKey;
        private System.Windows.Forms.ColumnHeader columnValue;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListView listStack;
        private System.Windows.Forms.ColumnHeader columnFunc;
        private System.Windows.Forms.ColumnHeader columnParameters;
        private System.Windows.Forms.ColumnHeader columnEnabled;
    }
}