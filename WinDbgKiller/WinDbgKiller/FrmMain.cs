using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Data.Odbc;
using Microsoft.Diagnostics.Runtime.Interop;

using DebugHelp;
using CsDebugScript;
using CsDebugScript.CLR;
using CsDebugScript.Drawing;
using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using CsDebugScript.Engine.Debuggers.DbgEngDllHelpers;
using CsDebugScript.Engine.Native;
using CsDebugScript.Engine.SymbolProviders;
using CsDebugScript.Engine.Utility;
using CsDebugScript.Exceptions;

namespace WinDbgKiller
{
    public partial class FrmMain : Form
    {
        private WinDbgEngine _engine;
        private FormOutputHandler _outputHandler;
        private BreakpointEventHandler _breakpointHandler;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboSource_DropDown(sender, e);
        }

        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            if (radioRunningProcess.Checked)
            {
                MessageBox.Show("Please select the running process using the box on the left.", "Running Processes...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (radioNewProcess.Checked)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.RestoreDirectory = true;
                    ofd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    ofd.Filter = "Executable Files (*.exe)|*.exe";
                    ofd.Title = "Select Application to debug...";
                    ofd.Multiselect = false;
                    ofd.CheckFileExists = true;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        comboSource.Text = ofd.FileName;
                    }
                }
            }
            else if (radioKernelPipe.Checked)
            {
                MessageBox.Show("Please select the kernel pipe using the box on the left.", "Existing Pipes...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (radioRunningProcess.Checked)
            {
                _engine = new WinDbgEngine(uint.Parse(comboSource.Text.Split(' ')[0]));
            }
            else if (radioNewProcess.Checked)
            {
                _engine = new WinDbgEngine(comboSource.Text, true);
            }
            else if (radioKernelPipe.Checked)
            {
                _engine = new WinDbgEngine(comboSource.Text);
            }
            if (_engine != null)
            {
                _outputHandler = new FormOutputHandler(txtLog);
                _breakpointHandler = new BreakpointEventHandler(this);
                _engine.InstallOutputHandler(_outputHandler);
                _engine.InstallBreakpointHandler(_breakpointHandler);
            }
        }

        private void radioRunningProcess_CheckedChanged(object sender, EventArgs e)
        {
            comboSource_DropDown(sender, e);
            if (radioNewProcess.Checked)
            {
                comboSource.Enabled = false;
            }
            else
            {
                comboSource.Enabled = true;
            }
        }

        private void radioNewProcess_CheckedChanged(object sender, EventArgs e)
        {
            comboSource_DropDown(sender, e);
            if (radioNewProcess.Checked)
            {
                comboSource.Enabled = false;
            }
            else
            {
                comboSource.Enabled = true;
            }
        }

        private void radioKernelPipe_CheckedChanged(object sender, EventArgs e)
        {
            comboSource_DropDown(sender, e);
            if (radioNewProcess.Checked)
            {
                comboSource.Enabled = false;
            }
            else
            {
                comboSource.Enabled = true;
            }
        }

        private void comboSource_DropDown(object sender, EventArgs e)
        {
            comboSource.Items.Clear();
            if (radioRunningProcess.Checked)
            {
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcesses())
                {
                    comboSource.Items.Add($"{proc.Id} - {proc.ProcessName}");
                }
                comboSource.Text = comboSource.Items[0].ToString();
            }
            else if (radioNewProcess.Checked)
            {
                comboSource.Text = "";
            }
            else if (radioKernelPipe.Checked)
            {
                foreach (String pipe in Directory.GetFiles(@"\\.\pipe\"))
                {
                    comboSource.Items.Add($"{pipe}");
                }
                comboSource.Text = comboSource.Items[0].ToString();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (_engine != null)
            {
                _engine.Execute("version");
                _engine.Execute("g");
            }
        }
    }

    class FormOutputHandler : IDebugOutputCallbacks
    {
        private TextBox outputTextbox;
        public FormOutputHandler(TextBox txt)
        {
            this.outputTextbox = txt;
        }
        public int Output(DEBUG_OUTPUT Mask, string Text)
        {
            MessageBox.Show(Text);
            outputTextbox.Invoke((MethodInvoker)(() => {
                this.outputTextbox.AppendText(Text + Environment.NewLine);
            }));
            return 0;
        }
    }

    class BreakpointEventHandler : DebugEventCallbacks
    {
        private FrmMain parent;
        private Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint, FrmMain>> hooks;
        public BreakpointEventHandler(FrmMain frm) : base(frm)
        {
            hooks = new Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint, FrmMain>>();
        }

        public override int Breakpoint(IDebugBreakpoint Bp)
        {
            MessageBox.Show("Breakpoint Hit!");
            //Handle
            if (hooks.Keys.Contains(Bp))
            {
                hooks[Bp](Bp, parent);
            }
            return 0;
        }

        public void installHook(IDebugBreakpoint Bp, Action<IDebugBreakpoint, FrmMain> Handler)
        {
            hooks.Add(Bp, Handler);
        }
    }

    class DebugEventCallbacks : IDebugEventCallbacks
    {
        private FrmMain parent;
        public DebugEventCallbacks(FrmMain frm)
        {
            parent = frm;
        }

        public virtual int GetInterestMask(out DEBUG_EVENT dEvent)
        {
            dEvent = DEBUG_EVENT.NONE;
            return 0;
        }

        public virtual int Breakpoint(IDebugBreakpoint breakpoint)
        {
            return 0;
        }

        public virtual int Exception(ref EXCEPTION_RECORD64 ex, uint code)
        {
            return 0;
        }

        public virtual int CreateThread(ulong first, ulong second, ulong third)
        {
            return 0;
        }

        public virtual int ExitThread(uint first)
        {
            return 0;
        }

        public virtual int CreateProcess(ulong first, ulong second, ulong third, uint fourth, string fifth, string sixth, uint seventh, uint eighth, ulong nineth, ulong tenth, ulong eleventh)
        {
            return 0;
        }

        public virtual int ExitProcess(uint first)
        {
            return 0;
        }

        public virtual int LoadModule(ulong first, ulong second, uint third, string fourth, string fifth, uint sixth, uint seventh)
        {
            return 0;
        }

        public virtual int UnloadModule(string path, ulong id)
        {
            return 0;
        }

        public virtual int SystemError(uint first, uint last)
        {
            return 0;
        }

        public virtual int SessionStatus(DEBUG_SESSION session)
        {
            return 0;
        }

        public virtual int ChangeDebuggeeState(DEBUG_CDS cds, ulong last)
        {
            return 0;
        }

        int IDebugEventCallbacks.ChangeEngineState(DEBUG_CES Flags, ulong Argument)
        {
            return 0;
        }

        int IDebugEventCallbacks.ChangeSymbolState(DEBUG_CSS Flags, ulong Argument)
        {
            return 0;
        }
    }
}
