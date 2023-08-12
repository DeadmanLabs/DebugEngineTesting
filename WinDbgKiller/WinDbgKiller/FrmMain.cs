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
using System.CodeDom;

namespace WinDbgKiller
{
    public partial class FrmMain : Form
    {
        private Debugger _engine;
        private Process _debuggee;
        private FormOutputHandler _outputHandler;
        private BreakpointEventHandler _breakpointHandler;
        private bool _dispose = false;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = this.Text + $" - {(Environment.Is64BitProcess ? "x64" : "x86")}";
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
            _engine = new Debugger(new Action<string>((text) =>
            {
                if (txtLog.InvokeRequired)
                {
                    txtLog.BeginInvoke((MethodInvoker)delegate
                    {
                        txtLog.AppendText(text.Replace("\n", Environment.NewLine));
                    });
                }
                else
                {
                    txtLog.AppendText(text.Replace("\n", Environment.NewLine));
                }
            }));
            _engine.SetOutputText(true);
            if (radioRunningProcess.Checked)
            {
                if (_engine.AttachTo(int.Parse(comboSource.Text.Split(' ')[0])) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            else if (radioNewProcess.Checked)
            {
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = comboSource.Text;
                psInfo.UseShellExecute = true;
                _debuggee = Process.Start(psInfo);

                if (_engine.AttachTo(_debuggee.Id) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            else if (radioKernelPipe.Checked)
            {
                if (_engine.KernelAttachTo(comboSource.Text) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            if (_engine != null)
            {
                MessageBox.Show("Breaking...");
                _engine.SetInterrupt();
                _engine.WaitForEvent();
                txtLog.AppendText($"Successfully Attached To Process!{Environment.NewLine}");
                txtLog.ScrollBars = ScrollBars.Vertical;
                BeginAttack();
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
                _engine.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);
                _engine.Execute("version");
                _engine.Execute("K");
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dispose = true;
            _engine.Detach();
            _debuggee.Kill();
        }

        private void RefreshStats()
        {
            if (_engine != null)
            {
                int a;
                //Get Threads
                //Get Registers
                //Get Callstack
                //Get Breakpoints
            }
        }

        private void BeginAttack()
        {
            int hr = _engine.Execute("lm");
            if (hr != 0)
            {
                throw new Exception($"Execution of \"lm\" failed! Error {hr}");
            }

            hr = _engine.Execute("bp ws2_32!send");
            hr = _engine.Execute("bp ws2_32!recv");
            hr = _engine.Execute("bp ws2_32!listen");
            hr = _engine.Execute("bp ws2_32!accept");
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //listen
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //accept
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //accept
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //send
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //recv
            hr = _engine.Execute("g");
            hr = _engine.WaitForEvent();
            //Install unbreak when dealloc
            string memAddress = "0139debc";
            int breakIndex = 0;
            hr = _engine.Execute($"bp msvcrt!free \"r {memAddress} = poi(esp+4); .if ({memAddress} == {breakIndex}) {{ bd 0; }} .else {{ gc }} \"");
            hr = _engine.WaitForEvent();
            //hr = _engine.Execute("ba r 1 eax");
            //hr = _engine.Execute("u @eip L1");
            //Get Module List
            //Install Breaks
            //Install Callbacks
            //Go
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
            MessageBox.Show("GetInterestMask()");
            return 0;
        }

        public virtual int Breakpoint(IDebugBreakpoint breakpoint)
        {
            MessageBox.Show("Breakpoint()");
            return 0;
        }

        public virtual int Exception(ref EXCEPTION_RECORD64 ex, uint code)
        {
            MessageBox.Show("Exception()");
            return 0;
        }

        public virtual int CreateThread(ulong first, ulong second, ulong third)
        {
            MessageBox.Show("CreateThread()");
            return 0;
        }

        public virtual int ExitThread(uint first)
        {
            MessageBox.Show("ExitThread()");
            return 0;
        }

        public virtual int CreateProcess(ulong first, ulong second, ulong third, uint fourth, string fifth, string sixth, uint seventh, uint eighth, ulong nineth, ulong tenth, ulong eleventh)
        {
            MessageBox.Show("CreateProcess()");
            return 0;
        }

        public virtual int ExitProcess(uint first)
        {
            MessageBox.Show("ExitProcess()");
            return 0;
        }

        public virtual int LoadModule(ulong first, ulong second, uint third, string fourth, string fifth, uint sixth, uint seventh)
        {
            MessageBox.Show("LoadModule()");
            return 0;
        }

        public virtual int UnloadModule(string path, ulong id)
        {
            MessageBox.Show("UnloadModule()");
            return 0;
        }

        public virtual int SystemError(uint first, uint last)
        {
            MessageBox.Show("SystemError()");
            return 0;
        }

        public virtual int SessionStatus(DEBUG_SESSION session)
        {
            MessageBox.Show("SessionStatus()");
            return 0;
        }

        public virtual int ChangeDebuggeeState(DEBUG_CDS cds, ulong last)
        {
            MessageBox.Show("ChangeDebuggeeState()");
            return 0;
        }

        int IDebugEventCallbacks.ChangeEngineState(DEBUG_CES Flags, ulong Argument)
        {
            MessageBox.Show("ChangeEngineState()");
            return 0;
        }

        int IDebugEventCallbacks.ChangeSymbolState(DEBUG_CSS Flags, ulong Argument)
        {
            MessageBox.Show("ChangeSymbolState()");
            return 0;
        }
    }
}
