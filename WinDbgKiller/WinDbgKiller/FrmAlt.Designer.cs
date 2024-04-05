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
            this.btnDetach = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.comboProcesses = new System.Windows.Forms.ComboBox();
            this.groupRegisters = new System.Windows.Forms.GroupBox();
            this.listRegisters = new System.Windows.Forms.ListView();
            this.columnReg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnVal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupThreads = new System.Windows.Forms.GroupBox();
            this.listThreads = new System.Windows.Forms.ListView();
            this.columnSelected = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnLastError = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBreakpoints = new System.Windows.Forms.GroupBox();
            this.listBreakpoints = new System.Windows.Forms.ListView();
            this.columnOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnInstruction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.groupStack = new System.Windows.Forms.GroupBox();
            this.listStack = new System.Windows.Forms.ListView();
            this.columnFunc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupModules = new System.Windows.Forms.GroupBox();
            this.treeModules = new System.Windows.Forms.TreeView();
            this.groupStatus = new System.Windows.Forms.GroupBox();
            this.btnBreak = new System.Windows.Forms.Button();
            this.btnContinue = new System.Windows.Forms.Button();
            this.listEngineFlags = new System.Windows.Forms.ListView();
            this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelState = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
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
            this.label1.Location = new System.Drawing.Point(8, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 16);
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
            this.groupDebuggee.Location = new System.Drawing.Point(16, 15);
            this.groupDebuggee.Margin = new System.Windows.Forms.Padding(4);
            this.groupDebuggee.Name = "groupDebuggee";
            this.groupDebuggee.Padding = new System.Windows.Forms.Padding(4);
            this.groupDebuggee.Size = new System.Drawing.Size(380, 118);
            this.groupDebuggee.TabIndex = 1;
            this.groupDebuggee.TabStop = false;
            this.groupDebuggee.Text = "Debuggee";
            // 
            // btnDetach
            // 
            this.btnDetach.Location = new System.Drawing.Point(12, 82);
            this.btnDetach.Margin = new System.Windows.Forms.Padding(4);
            this.btnDetach.Name = "btnDetach";
            this.btnDetach.Size = new System.Drawing.Size(360, 28);
            this.btnDetach.TabIndex = 5;
            this.btnDetach.Text = "Debug";
            this.btnDetach.UseVisualStyleBackColor = true;
            this.btnDetach.Click += new System.EventHandler(this.btnDetach_Click);
            // 
            // btnAttach
            // 
            this.btnAttach.Location = new System.Drawing.Point(301, 14);
            this.btnAttach.Margin = new System.Windows.Forms.Padding(4);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(71, 28);
            this.btnAttach.TabIndex = 4;
            this.btnAttach.Text = "Attach";
            this.btnAttach.UseVisualStyleBackColor = true;
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(12, 49);
            this.btnLoadFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(360, 28);
            this.btnLoadFile.TabIndex = 3;
            this.btnLoadFile.Text = "Load From File";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // comboProcesses
            // 
            this.comboProcesses.FormattingEnabled = true;
            this.comboProcesses.Location = new System.Drawing.Point(75, 16);
            this.comboProcesses.Margin = new System.Windows.Forms.Padding(4);
            this.comboProcesses.Name = "comboProcesses";
            this.comboProcesses.Size = new System.Drawing.Size(217, 24);
            this.comboProcesses.TabIndex = 1;
            this.comboProcesses.DropDown += new System.EventHandler(this.comboProcesses_DropDown);
            // 
            // groupRegisters
            // 
            this.groupRegisters.Controls.Add(this.listRegisters);
            this.groupRegisters.Location = new System.Drawing.Point(16, 140);
            this.groupRegisters.Margin = new System.Windows.Forms.Padding(4);
            this.groupRegisters.Name = "groupRegisters";
            this.groupRegisters.Padding = new System.Windows.Forms.Padding(4);
            this.groupRegisters.Size = new System.Drawing.Size(380, 337);
            this.groupRegisters.TabIndex = 2;
            this.groupRegisters.TabStop = false;
            this.groupRegisters.Text = "Registers";
            // 
            // listRegisters
            // 
            this.listRegisters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnReg,
            this.columnVal});
            this.listRegisters.HideSelection = false;
            this.listRegisters.Location = new System.Drawing.Point(12, 23);
            this.listRegisters.Margin = new System.Windows.Forms.Padding(4);
            this.listRegisters.Name = "listRegisters";
            this.listRegisters.Size = new System.Drawing.Size(359, 306);
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
            // groupThreads
            // 
            this.groupThreads.Controls.Add(this.listThreads);
            this.groupThreads.Location = new System.Drawing.Point(1217, 272);
            this.groupThreads.Margin = new System.Windows.Forms.Padding(4);
            this.groupThreads.Name = "groupThreads";
            this.groupThreads.Padding = new System.Windows.Forms.Padding(4);
            this.groupThreads.Size = new System.Drawing.Size(515, 206);
            this.groupThreads.TabIndex = 2;
            this.groupThreads.TabStop = false;
            this.groupThreads.Text = "Threads";
            // 
            // listThreads
            // 
            this.listThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnID,
            this.columnSelected,
            this.columnEntry,
            this.columnLastError});
            this.listThreads.HideSelection = false;
            this.listThreads.Location = new System.Drawing.Point(9, 25);
            this.listThreads.Margin = new System.Windows.Forms.Padding(4);
            this.listThreads.Name = "listThreads";
            this.listThreads.Size = new System.Drawing.Size(496, 173);
            this.listThreads.TabIndex = 0;
            this.listThreads.UseCompatibleStateImageBehavior = false;
            this.listThreads.View = System.Windows.Forms.View.Details;
            // 
            // columnSelected
            // 
            this.columnSelected.Text = "Active";
            this.columnSelected.Width = 49;
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
            // groupBreakpoints
            // 
            this.groupBreakpoints.Controls.Add(this.listBreakpoints);
            this.groupBreakpoints.Location = new System.Drawing.Point(827, 485);
            this.groupBreakpoints.Margin = new System.Windows.Forms.Padding(4);
            this.groupBreakpoints.Name = "groupBreakpoints";
            this.groupBreakpoints.Padding = new System.Windows.Forms.Padding(4);
            this.groupBreakpoints.Size = new System.Drawing.Size(905, 242);
            this.groupBreakpoints.TabIndex = 2;
            this.groupBreakpoints.TabStop = false;
            this.groupBreakpoints.Text = "Breakpoints";
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
            this.listBreakpoints.Location = new System.Drawing.Point(9, 25);
            this.listBreakpoints.Margin = new System.Windows.Forms.Padding(4);
            this.listBreakpoints.Name = "listBreakpoints";
            this.listBreakpoints.Size = new System.Drawing.Size(887, 210);
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
            // columnEnabled
            // 
            this.columnEnabled.Text = "Enabled";
            this.columnEnabled.Width = 58;
            // 
            // columnComment
            // 
            this.columnComment.Text = "Comment";
            this.columnComment.Width = 244;
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(16, 485);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(4);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(801, 242);
            this.txtOutput.TabIndex = 3;
            // 
            // groupStack
            // 
            this.groupStack.Controls.Add(this.listStack);
            this.groupStack.Location = new System.Drawing.Point(1217, 15);
            this.groupStack.Margin = new System.Windows.Forms.Padding(4);
            this.groupStack.Name = "groupStack";
            this.groupStack.Padding = new System.Windows.Forms.Padding(4);
            this.groupStack.Size = new System.Drawing.Size(515, 250);
            this.groupStack.TabIndex = 3;
            this.groupStack.TabStop = false;
            this.groupStack.Text = "Stack";
            // 
            // listStack
            // 
            this.listStack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFunc,
            this.columnParameters});
            this.listStack.HideSelection = false;
            this.listStack.Location = new System.Drawing.Point(8, 23);
            this.listStack.Margin = new System.Windows.Forms.Padding(4);
            this.listStack.Name = "listStack";
            this.listStack.Size = new System.Drawing.Size(497, 218);
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
            // groupModules
            // 
            this.groupModules.Controls.Add(this.treeModules);
            this.groupModules.Location = new System.Drawing.Point(405, 15);
            this.groupModules.Margin = new System.Windows.Forms.Padding(4);
            this.groupModules.Name = "groupModules";
            this.groupModules.Padding = new System.Windows.Forms.Padding(4);
            this.groupModules.Size = new System.Drawing.Size(413, 463);
            this.groupModules.TabIndex = 4;
            this.groupModules.TabStop = false;
            this.groupModules.Text = "Modules";
            // 
            // treeModules
            // 
            this.treeModules.CheckBoxes = true;
            this.treeModules.Location = new System.Drawing.Point(9, 16);
            this.treeModules.Margin = new System.Windows.Forms.Padding(4);
            this.treeModules.Name = "treeModules";
            this.treeModules.Size = new System.Drawing.Size(395, 438);
            this.treeModules.TabIndex = 0;
            // 
            // groupStatus
            // 
            this.groupStatus.Controls.Add(this.btnBreak);
            this.groupStatus.Controls.Add(this.btnContinue);
            this.groupStatus.Controls.Add(this.listEngineFlags);
            this.groupStatus.Controls.Add(this.labelState);
            this.groupStatus.Controls.Add(this.labelStatus);
            this.groupStatus.Controls.Add(this.label4);
            this.groupStatus.Controls.Add(this.label3);
            this.groupStatus.Controls.Add(this.label2);
            this.groupStatus.Location = new System.Drawing.Point(827, 15);
            this.groupStatus.Margin = new System.Windows.Forms.Padding(4);
            this.groupStatus.Name = "groupStatus";
            this.groupStatus.Padding = new System.Windows.Forms.Padding(4);
            this.groupStatus.Size = new System.Drawing.Size(383, 463);
            this.groupStatus.TabIndex = 5;
            this.groupStatus.TabStop = false;
            this.groupStatus.Text = "Status";
            // 
            // btnBreak
            // 
            this.btnBreak.Location = new System.Drawing.Point(199, 429);
            this.btnBreak.Name = "btnBreak";
            this.btnBreak.Size = new System.Drawing.Size(174, 27);
            this.btnBreak.TabIndex = 7;
            this.btnBreak.Text = "Break";
            this.btnBreak.UseVisualStyleBackColor = true;
            this.btnBreak.Click += new System.EventHandler(this.btnBreak_Click);
            // 
            // btnContinue
            // 
            this.btnContinue.Location = new System.Drawing.Point(7, 429);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(174, 27);
            this.btnContinue.TabIndex = 6;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // listEngineFlags
            // 
            this.listEngineFlags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnKey,
            this.columnValue});
            this.listEngineFlags.HideSelection = false;
            this.listEngineFlags.Location = new System.Drawing.Point(13, 102);
            this.listEngineFlags.Margin = new System.Windows.Forms.Padding(4);
            this.listEngineFlags.Name = "listEngineFlags";
            this.listEngineFlags.Size = new System.Drawing.Size(360, 320);
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
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Location = new System.Drawing.Point(75, 49);
            this.labelState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(38, 16);
            this.labelState.TabIndex = 4;
            this.labelState.Text = "Idle...";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(75, 26);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(38, 16);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Idle...";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 82);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Engine:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 49);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "State: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 25);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Status: ";
            // 
            // FrmAlt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1748, 742);
            this.Controls.Add(this.groupStatus);
            this.Controls.Add(this.groupModules);
            this.Controls.Add(this.groupStack);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.groupBreakpoints);
            this.Controls.Add(this.groupThreads);
            this.Controls.Add(this.groupRegisters);
            this.Controls.Add(this.groupDebuggee);
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Button btnBreak;
        private System.Windows.Forms.Button btnContinue;
    }
}