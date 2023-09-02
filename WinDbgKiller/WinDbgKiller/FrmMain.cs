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

namespace WinDbgKiller
{
    public partial class FrmMain : Form
    {
        private Debugger _engine;
        private Process _debuggee;
        private FormOutputHandler _outputHandler;
        private Task _processor;
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

        private async void btnLaunch_Click(object sender, EventArgs e)
        {
            _engine = new Debugger();
            _engine.SetOutputText(true);
            _engine.useCallbacks = true;
            _engine.OnOutput += handleOutput;
            _engine.OnStateChange += handleStateChange;
            _engine.OnBreakpoint += handleBreakpoint;
            if (radioRunningProcess.Checked)
            {
                if (await _engine.AttachTo(int.Parse(comboSource.Text.Split(' ')[0])) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            else if (radioNewProcess.Checked)
            {
                ProcessStartInfo psInfo = new ProcessStartInfo();
                if (comboSource.InvokeRequired)
                {
                    comboSource.Invoke((MethodInvoker)delegate
                    {
                        psInfo.FileName = comboSource.Text;
                    });
                }
                else
                {
                    psInfo.FileName = comboSource.Text;
                }
                psInfo.UseShellExecute = true;
                if (!File.Exists(psInfo.FileName))
                {
                    MessageBox.Show("The path to the binary was not selected or does not exist! Please select a valid binary path.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _debuggee = Process.Start(psInfo);

                if (await _engine.AttachTo(_debuggee.Id) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            else if (radioKernelPipe.Checked)
            {
                if (await _engine.KernelAttachTo(comboSource.Text) == false)
                {
                    MessageBox.Show("Failed to attach!");
                    return;
                }
            }
            if (_engine != null)
            {
                _engine.SetInterrupt();
                await _engine.WaitForEvent();
                txtLog.AppendText($"Successfully Attached To Process!{Environment.NewLine}");
                MessageBox.Show("Successfully attached to the process!", "Success!");
                txtLog.ScrollBars = ScrollBars.Vertical;
                if (await _engine.IsModuleLoaded("ws2_32"))
                {
                    txtLog.AppendText($"Networking Detected!" + Environment.NewLine + "Hooking Recv...");
                    IDebugBreakpoint bpRecv = await _engine.SetBreakAtFunction("ws2_32", "recv");
                    _engine.addCallback(bpRecv, (bp) =>
                    {
                        MessageBox.Show("Recv Fired!");
                    });
                    txtLog.AppendText("Done!" + Environment.NewLine + "Hooking Send...");
                    IDebugBreakpoint bpSend = await _engine.SetBreakAtFunction("ws2_32", "send");
                    _engine.addCallback(bpSend, (bp) =>
                    {
                        MessageBox.Show("Send Fired!");
                    });
                    txtLog.AppendText("Done!" + Environment.NewLine + "Hooking Listen...");
                    IDebugBreakpoint bpListen = await _engine.SetBreakAtFunction("ws2_32", "listen");
                    _engine.addCallback(bpListen, (bp) =>
                    {
                        MessageBox.Show("Listen Fired!");
                    });
                    txtLog.AppendText("Done!" + Environment.NewLine + "Hooking Accept...");
                    IDebugBreakpoint bpAccept = await _engine.SetBreakAtFunction("ws2_32", "accept");
                    _engine.addCallback(bpAccept, (bp) =>
                    {
                        MessageBox.Show("Accept Fired!");
                    });
                    txtLog.AppendText("Done!" + Environment.NewLine + "Hooking Bind...");
                    IDebugBreakpoint bpBind = await _engine.SetBreakAtFunction("ws2_32", "bind");
                    _engine.addCallback(bpBind, (bp) =>
                    {
                        MessageBox.Show("Bind Fired!");
                    });
                    txtLog.AppendText("Done!" + Environment.NewLine);
                    List<string> fncs = await _engine.ListFuncsInDebuggee();
                    await _engine.Execute("bl");
                    //txtLog.AppendText($"Breakpoint Callbacks: {string.Join(", ", _engine.readCallbacks().Keys)}");
                    /*
                    List<String> modules = _engine.GetAllModuleNames();
                    foreach (string module in modules)
                    {
                        MessageBox.Show($"{module}", $"{modules.IndexOf(module)} / {modules.Count}");
                    }
                    Dictionary<string, string[]> funcs = _engine.GetAllFunctionNames();
                    foreach (string module in funcs.Keys)
                    {
                        foreach (string func in funcs[module])
                        {
                            MessageBox.Show($"{module} -> {func}", $"{(funcs[module]).ToList().IndexOf(func)} / {funcs[module].Count()}");
                        }
                    }
                    */
                    txtLog.AppendText($"Continuing Execution...{Environment.NewLine}");
                    await _engine.Execute("g");
                    await _engine.WaitForEvent();
                    MessageBox.Show("Buttholes!");
                    await _engine.WaitForEvent();
                    await _engine.Execute("g");
                    MessageBox.Show("Buttholes 2!");
                    await _engine.WaitForEvent();
                }
                else
                {
                    txtLog.AppendText($"Networking Undetected!");
                }
                //BeginAttack();
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

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (_engine != null)
            {
                _engine.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);
                await _engine.Execute("version");
                await _engine.Execute("K");
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dispose = true;
            if (_engine != null)
            {
                _engine.Detach();
                _engine.OnOutput -= handleOutput;
                _engine.OnStateChange -= handleStateChange;
                _engine.OnBreakpoint -= handleBreakpoint;
            }
            if (_debuggee != null && !_debuggee.HasExited)
            {
                _debuggee.Kill();
            }
        }

        private async void btnContinue_Click(object sender, EventArgs e)
        {
            txtLog.AppendText("Continuing..." + Environment.NewLine);
            await _engine.Execute("g");
        }

        private async void btnCheck_Click(object sender, EventArgs e)
        {
            DEBUG_STATUS status;
            int Result;
            (Result, status) = await _engine.GetExecutionStatus();
            MessageBox.Show(status.ToString(), "Debugger Status");
        }

        private void RefreshStats()
        {
            listBreakpoints.Items.Clear();
            listRegisters.Items.Clear();
            listStack.Items.Clear();
            listThreads.Items.Clear();
            if (_engine != null)
            {
                //Get Threads
                Dictionary<DEBUG_REGISTER_DESCRIPTION, DEBUG_VALUE> registers = getRegisters(_engine);
                foreach (KeyValuePair<DEBUG_REGISTER_DESCRIPTION, DEBUG_VALUE> register in registers)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = register.Key.ToString();
                    //lvi.SubItems.Add(BitConverter.ToString(register.Value.RawBytes).Replace("-", ""));
                    listRegisters.Items.Add(lvi);
                }
                //Get Registers
                List<DEBUG_STACK_FRAME> callstack = getCallstack(_engine);
                foreach (DEBUG_STACK_FRAME call in callstack)
                {
                    ListViewItem lvi = new ListViewItem();
                    StringBuilder nameBuffer = new StringBuilder(512);
                    uint nameSize;
                    ulong displacement;

                    int hr = _engine._symbols.GetNameByOffset(call.InstructionOffset, nameBuffer, nameBuffer.Capacity, out nameSize, out displacement);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to grab name using offset");
                        return;
                    }
                    string name = nameBuffer.ToString();
                    string moduleName = name.Split('!')[0];
                    string functionName = name.Split('!')[1];

                    lvi.Text = name;
                    listStack.Items.Add(lvi);
                }
                //Get Callstack
                List<IDebugBreakpoint> breakpoints = getBreakpoints(_engine);
                foreach (IDebugBreakpoint breakpoint in breakpoints)
                {
                    ListViewItem lvi = new ListViewItem();
                    ulong offset;
                    int hr = breakpoint.GetOffset(out offset);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to get the offset for the breakpoint!");
                        continue;
                    }
                    const int MaxNameSize = 256;
                    StringBuilder nameBuffer = new StringBuilder(MaxNameSize);
                    uint nameSize;
                    ulong displacement;

                    hr = _engine._symbols.GetNameByOffset(offset, nameBuffer, nameBuffer.Capacity, out nameSize, out displacement);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to get the instruction for the offset");
                        continue;
                    }
                    string instruction = nameBuffer.ToString();
                    lvi.Text = offset.ToString();
                    lvi.SubItems.Add(instruction);
                }
                //Get Breakpoints
            }
        }

        private Dictionary<DEBUG_REGISTER_DESCRIPTION, DEBUG_VALUE> getRegisters(Debugger dbg)
        {
            Dictionary<DEBUG_REGISTER_DESCRIPTION, DEBUG_VALUE> registers = new Dictionary<DEBUG_REGISTER_DESCRIPTION, DEBUG_VALUE>();
            uint numRegisters;
            int hr = dbg._registers.GetNumberRegisters(out numRegisters);
            if (hr != 0)
            {
                MessageBox.Show("Failed to grab registers");
                return registers;
            }
            StringBuilder nameBuffer = new StringBuilder(256);
            for (uint i = 0; i < numRegisters; i++)
            {
                uint nameSize;
                DEBUG_REGISTER_DESCRIPTION description;
                hr = dbg._registers.GetDescription(i, nameBuffer, nameBuffer.Capacity, out nameSize, out description);
                if (hr != 0)
                {
                    MessageBox.Show($"Failed to get register {i} description!");
                    continue;
                }
                string registerName = nameBuffer.ToString();
                DEBUG_VALUE registerValue;
                hr = dbg._registers.GetValue(i, out registerValue);
                if (hr != 0)
                {
                    MessageBox.Show($"Failed to get register {i} value");
                    continue;
                }
                registers.Add(description, registerValue);
            }
            return registers;
        }

        private List<DEBUG_STACK_FRAME> getCallstack(Debugger dbg)
        {
            const int MaxFrames = 32;
            DEBUG_STACK_FRAME[] frames = new DEBUG_STACK_FRAME[MaxFrames];
            uint framesFilled;
            int hr = dbg._control.GetStackTrace(0, 0, 0, frames, frames.Length, out framesFilled);
            if (hr != 0)
            {
                MessageBox.Show("Failed to grab the callstack.");
                return null;
            }
            List<DEBUG_STACK_FRAME> callStack = new List<DEBUG_STACK_FRAME>();
            for (uint i = 0; i < framesFilled; i++)
            {
                callStack.Add(frames[i]);
            }
            return callStack;
        }

        private List<IDebugBreakpoint> getBreakpoints(Debugger dbg)
        {
            uint numBreakpoints;
            int hr = dbg._control.GetNumberBreakpoints(out numBreakpoints);
            if (hr != 0)
            {
                MessageBox.Show("Failed to get the number of breakpoints.");
                return null;
            }
            List<IDebugBreakpoint> breakpoints = new List<IDebugBreakpoint>();
            for (uint i = 0; i < numBreakpoints; i++)
            {
                IDebugBreakpoint bp;
                hr = dbg._control.GetBreakpointByIndex(i, out bp);
                if (hr != 0)
                {
                    MessageBox.Show($"Failed to get the breakpoint {i}");
                    continue;
                }
                breakpoints.Add(bp);
            }
            return breakpoints;
        }

        private void getThreads(Debugger dbg) //Unfinished
        {
            uint numThreads;
            int hr = dbg._sysObjects.GetNumberThreads(out numThreads);
            if (hr != 0)
            {
                MessageBox.Show("Failed to get the number of threads");
                return;
            }
            for (uint i = 0; i < numThreads; i++)
            {
                uint threadId;
                //hr = dbg._sysObjects.GetThreadIdsByIndex();
                if (hr != 0)
                {
                    MessageBox.Show($"Failed to grab thread {i}.");
                    continue;
                }

            }
        }

        private async void BeginAttack()
        {
            int hr = await _engine.Execute("lm");
            if (hr != 0)
            {
                throw new Exception($"Execution of \"lm\" failed! Error {hr}");
            }

            hr = await _engine.Execute("bp ws2_32!send");
            hr = await _engine.Execute("bp ws2_32!recv");
            hr = await _engine.Execute("bp ws2_32!listen");
            hr = await _engine.Execute("bp ws2_32!accept");
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //listen
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //accept
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //accept
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //send
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //recv
            hr = await _engine.Execute("g");
            hr = await _engine.WaitForEvent();
            //Install unbreak when dealloc
            string memAddress = "0139debc";
            int breakIndex = 0;
            hr = await _engine.Execute($"bp msvcrt!free \"r {memAddress} = poi(esp+4); .if ({memAddress} == {breakIndex}) {{ bd 0; }} .else {{ gc }} \"");
            hr = await _engine.WaitForEvent();
            //hr = _engine.Execute("ba r 1 eax");
            //hr = _engine.Execute("u @eip L1");
            //Get Module List
            //Install Breaks
            //Install Callbacks
            //Go
        }

        #region Debug Handlers

        private void handleOutput(object sender, OutputEventArgs e)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate
                {
                    txtLog.AppendText(e.Message.Replace("\n", Environment.NewLine));
                });
            }
            else
            {
                txtLog.AppendText(e.Message.Replace("\n", Environment.NewLine));
            }
        }

        private void handleStateChange(object sender, DebuggeeStateEventArgs e)
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.Invoke((MethodInvoker)delegate
                {
                    labelStatus.Text = e.Flags.ToString();
                });
            }
            else
            {
                labelStatus.Text = e.Flags.ToString();
            }
        }

        private void handleBreakpoint(object sender, BreakpointEventArgs e)
        {
            uint expressionSize;
            StringBuilder expression = new StringBuilder(512);
            int hr = e.Breakpoint.GetOffsetExpression(expression, expression.Capacity, out expressionSize);
            if (hr != 0)
            {
                MessageBox.Show("Failed to grab expression");
            }
            string formattedExpression = (expression == new StringBuilder(512)) ? "" : $" @ {expression.ToString()}";
            MessageBox.Show($"Breakpoint Hit{formattedExpression}!");
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate
                {
                    txtLog.AppendText($"Breakpoint Hit{formattedExpression}!" + Environment.NewLine);
                });
            }
            else
            {
                txtLog.AppendText($"Breakpoint Hit{formattedExpression}!" + Environment.NewLine);
            }
        }

        #endregion
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
            outputTextbox.Invoke((MethodInvoker)(() =>
            {
                this.outputTextbox.AppendText(Text + Environment.NewLine);
            }));
            return 0;
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
