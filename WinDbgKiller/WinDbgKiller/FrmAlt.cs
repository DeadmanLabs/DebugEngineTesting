using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Data.Odbc;
using Microsoft.Diagnostics.Runtime.Interop;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using WinDbgKiller;
using WinDbgKiller.Extensions;

namespace WinDbgKiller
{
    public partial class FrmAlt : Form
    {
        private Debugger _engine;
        private String _debuggeePath;
        private Process _debuggee;
        private bool _attached;

        private ulong pointerSize = 4;
        public FrmAlt()
        {
            InitializeComponent();
        }

        private void FrmAlt_Load(object sender, EventArgs e)
        {
            txtOutput.ScrollBars = ScrollBars.Vertical;
            this.Text = this.Text + $" - {(Environment.Is64BitProcess ? "x64" : "x86")}";
            if (Environment.Is64BitProcess)
            {
                this.pointerSize += 4;
            }
            comboProcesses_DropDown(sender, e);
            frmEnabled_Cycle();
        }

        private void frmEnabled_Cycle()
        {
            if (_attached)
            {
                btnDetach.Text = "Detach";
                comboProcesses.Enabled = false;
                btnAttach.Enabled = false;
                btnLoadFile.Enabled = false;
                listRegisters.Enabled = true;
                listBreakpoints.Enabled = true;
                listThreads.Enabled = true;
                listEngineFlags.Enabled = true;
                treeModules.Enabled = true;
                btnContinue.Enabled = true;
                btnBreak.Enabled = true;
            }
            else
            {
                btnDetach.Text = "Debug";
                comboProcesses.Enabled = !false;
                btnAttach.Enabled = !false;
                btnLoadFile.Enabled = !false;
                listRegisters.Enabled = !true;
                listBreakpoints.Enabled = !true;
                listThreads.Enabled = !true;
                listEngineFlags.Enabled = !true;
                treeModules.Enabled = !true;
                btnContinue.Enabled = !true;
                btnBreak.Enabled = !true;
            }
        }

        private async void frmRefresh_Stats()
        {
            //Refresh Stats
            if (_attached && _engine != null)
            {
                (int Result, DEBUG_STATUS Status) = await _engine.GetExecutionStatus();
                bool running = !(Status == DEBUG_STATUS.BREAK);
                if (running) { await _engine.Break(true); }
                Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                listRegisters.SafeOperation(() =>
                {
                    foreach (KeyValuePair<DbgEngRegister, ulong> register in registers)
                    {
                        listRegisters.UpdateOrAdd(register.Key.ToString(), $"0x{register.Value:X8}");
                    }
                });
                Dictionary<DEBUG_STACK_FRAME, string> stack = await _engine.listStack();
                listStack.SafeOperation(() =>
                {
                    if (stack != null)
                    {
                        listStack.Items.Clear();
                        foreach (KeyValuePair<DEBUG_STACK_FRAME, string> frame in stack)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = frame.Value;
                            lvi.SubItems.Add(frame.Key.Virtual.ToString());
                            listStack.Items.Add(lvi);
                        }
                    }
                });
                List<ThreadDetails> threads = await _engine.listThreads();
                listThreads.SafeOperation(() =>
                {
                    if (threads != null)
                    {
                        foreach (ThreadDetails thread in threads)
                        {
                            listThreads.UpdateOrAdd($"{thread.ThreadId:X}", new string[] { thread.Active.ToString(), $"0x{thread.EntryPoint:X8}", "" });
                        }
                    }
                });
                List<BreakpointInfo> breakpoints = await _engine.GetCurrentBreakpointsInfo();
                listBreakpoints.SafeOperation(() =>
                {
                    if (breakpoints != null)
                    {
                        foreach (BreakpointInfo breakpoint in breakpoints)
                        {
                            bool hasCallback = _engine.callbacks.ContainsKey(breakpoint.Source);
                            listBreakpoints.UpdateOrAdd($"0x{breakpoint.Offset:X}", new string[] { breakpoint.Instruction, breakpoint.Expression, hasCallback.ToString(), "" });
                        }
                    }
                });
                if (running) { await _engine.SetExecutionStatus(Status); }
            }
            else
            {
                listRegisters.SafeOperation(() =>
                {
                    listRegisters.Items.Clear();
                });
                treeModules.SafeOperation(() =>
                {
                    treeModules.Nodes.Clear();
                });
                listEngineFlags.SafeOperation(() =>
                {
                    listEngineFlags.Items.Clear();
                });
                listBreakpoints.SafeOperation(() =>
                {
                    listBreakpoints.Items.Clear();
                });
                listThreads.SafeOperation(() =>
                {
                    listThreads.Items.Clear();
                });
                listStack.SafeOperation(() =>
                {
                    listStack.Items.Clear();
                });
            }
        }

        private void comboProcesses_DropDown(object sender, EventArgs e)
        {
            comboProcesses.Items.Clear();
            foreach (Process proc in Process.GetProcesses())
            {
                comboProcesses.Items.Add($"{proc.Id} - {proc.ProcessName}");
            }
            comboProcesses.Text = comboProcesses.Items[0].ToString();
            

            /*
            For Kernel Pipes Later: 

            foreach (String pipe in Directory.GetFiles(@"\\.\pipe\"))
            {
                comboProcesses.Items.Add($"{pipe}");
            }
            comboProcesses.Text = comboProcesses.Items[0].ToString();
            */

        }

        private void btnLoadFile_Click(object sender, EventArgs e)
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
                    _debuggeePath = ofd.FileName;
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Loaded Binary: {_debuggeePath}{Environment.NewLine}";
                    });
                }
            }
        }

        private async void btnDetach_Click(object sender, EventArgs e)
        {
            txtOutput.SafeOperation(() =>
            {
                txtOutput.Text = $"Attempting to start debugger...{Environment.NewLine}";
            });
            if (!_attached)
            {
                //Attach here and start debugging
                _engine = new Debugger();
                _engine.SetOutputText(true);
                _engine.useCallbacks = true;
                _engine.OnOutput += handleOutput;
                _engine.OnException += handleException;
                _engine.OnModuleLoad += handleModuleLoad;
                _engine.OnModuleUnload += handleModuleUnload;
                _engine.OnProcessCreate += handleProcessCreate;
                _engine.OnProcessTerminate += handleProcessTerminate;
                _engine.OnBreakpoint += handleBreakpoint;
                _engine.OnThreadCreate += handleThreadCreate;
                _engine.OnThreadTerminate += handleThreadTerminate;
                _engine.OnSessionChange += handleSessionChange;
                _engine.OnStateChange += handleStateChange;
                _engine.OnEngineChange += handleEngineChange;
                _engine.OnSymbolChange += handleSymbolChange;
                _engine.OnSystemError += handleSystemError;
                if (_debuggeePath != null && File.Exists(_debuggeePath))
                {
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Starting debugged process...";
                    });
                    ProcessStartInfo psInfo = new ProcessStartInfo();
                    psInfo.FileName = _debuggeePath;
                    psInfo.UseShellExecute = true;
                    _debuggee = Process.Start(psInfo);
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Done! PID: {_debuggee.Id}{Environment.NewLine}";
                    });
                    _attached = await _engine.AttachTo(_debuggee);
                    if (!_attached)
                    {
                        MessageBox.Show($"The debugger failed to attach to the process with ID: {_debuggee.Id}", "Failed to attach!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _engine.Dispose();
                        _engine = null;
                        frmEnabled_Cycle();
                        return;
                    }
                    else
                    {
                        txtOutput.SafeOperation(() =>
                        {
                            txtOutput.Text += $"Attached!{Environment.NewLine}";
                        });
                        _engine.SetInterrupt();
                        await _engine.WaitForEvent();
                        await _engine.Execute("g");
                    }
                }
                else if (_debuggeePath == null) //Dodges non-existant files that have been selected
                {
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Attaching to process...";
                    });
                    Regex regex = new Regex(@"^(\d{4}) - (.+)$");
                    if (regex.Match(comboProcesses.Text).Success)
                    {
                        _debuggee = Process.GetProcessById(int.Parse(comboProcesses.Text.Split(' ')[0]));
                        _attached = await _engine.AttachTo(_debuggee.Id);
                        if (!_attached)
                        {
                            MessageBox.Show($"The debugger failed to attach to the process with ID: {_debuggee.Id}", "Failed to attach!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _engine.Dispose();
                            _engine = null;
                            frmEnabled_Cycle();
                            return;
                        }
                        txtOutput.SafeOperation(() =>
                        {
                            txtOutput.Text += $"Done!{Environment.NewLine}";
                        });
                    }
                    else
                    {
                        MessageBox.Show($"The selected process has a malformed id structure.", "Failed to attach!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _engine.Dispose();
                        _engine = null;
                        frmEnabled_Cycle();
                        return;
                    }
                }
                //await _engine.SetExceptionBreakStatus(true);
            } 
            else
            {
                if (_engine != null)
                {
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Detaching...";
                    });
                    _engine.Detach();
                    _engine.Dispose();
                    _engine = null;
                    txtOutput.SafeOperation(() =>
                    {
                        txtOutput.Text += $"Done!{Environment.NewLine}";
                    });
                    frmEnabled_Cycle();
                    return;
                }
            }
            frmEnabled_Cycle();
            frmRefresh_Stats();
        }

        #region Handlers

        private void handleOutput(object sender, OutputEventArgs e)
        {
            txtOutput.SafeOperation(() =>
            {
                txtOutput.AppendText(e.Message.Replace("\n", Environment.NewLine));
            });
        }

        private void handleException(object sender, ExceptionEventArgs e)
        {
            KeyValuePair<uint, string> errorInfo = PageGuard.GetError(e.Exception.ExceptionCode);

            //MessageBox.Show(e.Exception.ToString(), "Exception Hit!");
            MessageBox.Show($"First Chance: {e.FirstChance}{Environment.NewLine}" +
                $"Exception - Address: 0x{e.Exception.ExceptionAddress:X}{Environment.NewLine}" + //addr
                $"Exception - Code: 0x{e.Exception.ExceptionCode:X}{Environment.NewLine}" +       //code
                $"Exception - Flags: {e.Exception.ExceptionFlags}{Environment.NewLine}" +
                $"Exception - Record: {e.Exception.ExceptionRecord}{Environment.NewLine}" +
                $"Exception - Number Parameters: {e.Exception.NumberParameters}{Environment.NewLine}" +
                $"Error: {errorInfo.Key} -> {errorInfo.Value}", "Exception Hit!");
            
            if (e.Exception.ExceptionCode == 0x80000001)
            {
                MessageBox.Show("Page Guard Violation Detected!");
            }
            else if (e.Exception.ExceptionCode == 0x80000003)
            {
                MessageBox.Show("Breakpoint Exception!");
            }
            else if (e.Exception.ExceptionCode == 0xC0000005)
            {
                MessageBox.Show("Access Violation!");
            }
        }

        private async void handleModuleLoad(object sender, LoadModuleEventArgs e)
        {
            /*
            MessageBox.Show($"Base Offset: {e.BaseOffset}{Environment.NewLine}" +
                $"Checksum: {e.Checksum}{Environment.NewLine}" +
                $"Image File Handle: {e.ImageFileHandle}{Environment.NewLine}" +
                $"Image Name: {e.ImageName}{Environment.NewLine}" +
                $"Module Name: {e.ModuleName}{Environment.NewLine}" +
                $"Module Size: {e.ModuleSize}{Environment.NewLine}" +
                $"TimeDate Stamp: {e.TimeDateStamp}{Environment.NewLine}", "Importted!"); */
            Dictionary<ulong, string> functions = await _engine.GetModuleFuncs(e.BaseOffset);
            TreeNode node = new TreeNode(e.ModuleName);
            foreach (KeyValuePair<ulong, string> function in functions)
            {
                node.Nodes.Add(function.Value);
            }
            treeModules.SafeOperation(() =>
            {
                treeModules.Nodes.Add(node);
            });
            if (e.ModuleName == "WS2_32")
            {
                IDebugBreakpoint bindBreak = await _engine.SetBreakAtFunction("WS2_32", "bind");
                IDebugBreakpoint listenBreak = await _engine.SetBreakAtFunction("WS2_32", "listen");
                IDebugBreakpoint acceptBreak = await _engine.SetBreakAtFunction("WS2_32", "accept");
                IDebugBreakpoint connectBreak = await _engine.SetBreakAtFunction("WS2_32", "connect");
                IDebugBreakpoint sendBreak = await _engine.SetBreakAtFunction("WS2_32", "send");
                IDebugBreakpoint recvBreak = await _engine.SetBreakAtFunction("WS2_32", "recv");
                _engine.addCallback(bindBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong socketAddr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong nameLen = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);

                    KeyValuePair<AddressFamily, IPEndPoint> address = await _engine.ReadSocketAddress(socketAddr);
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Address Family: {AddressFamily.GetName(typeof(AddressFamily), address.Key)}{Environment.NewLine}" +
                        $"Port: {address.Value.Port}{Environment.NewLine}" +
                        $"Address: {address.Value.Address.ToString()}{Environment.NewLine}" +
                        $"Name Length: {$"0x{nameLen:X}"}", "Bind Called!", MessageBoxButtons.OK);
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
                _engine.addCallback(listenBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong backlog = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);

                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Backlog: {$"0x{backlog:X}"}", "Listen Called!", MessageBoxButtons.OK);
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
                _engine.addCallback(acceptBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong socketAddr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong nameLen = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);

                    KeyValuePair<AddressFamily, IPEndPoint> address = await _engine.ReadSocketAddress(socketAddr);
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Address Family: {AddressFamily.GetName(typeof(AddressFamily), address.Key)}{Environment.NewLine}" +
                        $"Port: {address.Value.Port}{Environment.NewLine}" +
                        $"Address: {address.Value.Address.ToString()}{Environment.NewLine}" +
                        $"Name Length: {$"0x{nameLen:X}"}", "Accept Called!", MessageBoxButtons.OK);
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
                _engine.addCallback(connectBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong socketAddr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong nameLen = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);

                    KeyValuePair<AddressFamily, IPEndPoint> address = await _engine.ReadSocketAddress(socketAddr);
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Address Family: {AddressFamily.GetName(typeof(AddressFamily), address.Key)}{Environment.NewLine}" +
                        $"Port: {address.Value.Port}{Environment.NewLine}" +
                        $"Address: {address.Value.Address.ToString()}{Environment.NewLine}" +
                        $"Name Length: {$"0x{nameLen:X}"}", "Connect Called!", MessageBoxButtons.OK);
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
                _engine.addCallback(sendBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong dataPtr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong dataSize = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);
                    ulong flags = registers[DbgEngRegister.ESP] + (this.pointerSize * 4);

                    short trueSize = await _engine.ReadSignedWord(dataSize);
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Data Pointer: {$"0x{dataPtr:X}"} -> \"{Encoding.ASCII.GetString(await _engine.ReadBytes(await _engine.ReadPointer(dataPtr), (uint)trueSize))}\"{Environment.NewLine}" +
                        $"Data Size: {trueSize}{Environment.NewLine}" +
                        $"Flags: {$"0x{flags:X}"}", "Send Called!", MessageBoxButtons.OK);
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
                _engine.addCallback(recvBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong bufferPtr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong bufferSize = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);
                    ulong flags = registers[DbgEngRegister.ESP] + (this.pointerSize * 4);

                    short trueSize = await _engine.ReadSignedWord(bufferSize);
                    ulong dataBuffer = await _engine.ReadPointer(bufferPtr);
                    //
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Buffer Pointer: {$"0x{dataBuffer:X}"}{Environment.NewLine}" +
                        $"Buffer Size: {trueSize}{Environment.NewLine}" +
                        $"Flags: {$"0x{flags:X}"}", "WS2_32 Recv Called!", MessageBoxButtons.OK);
                    IDebugBreakpoint dataBufferBreak = await _engine.SetBreakAtMemory(dataBuffer);
                    _engine.addCallback(dataBufferBreak, async (dbp) =>
                    {
                        ulong instruct = await _engine.GetCurrentInstructionAddress(); //might this be 1 op passed the instruction? (instruction has to execute to trigger break, so EIP is 1 down from the triggering opcode)
                        string opcode = await _engine.GetCurrentOpcode();
                        MessageBox.Show($"0x{instruct:X} - {opcode}", "Data Buffer Accessed!", MessageBoxButtons.OK);
                        await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                        await _engine.WaitForEvent();
                    });

                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
            }
            else if (e.ModuleName == "wsock32")
            {
                //MessageBox.Show($"We arent adding callbacks to this just yet", "wsock32 Importted!", MessageBoxButtons.OK);
                IDebugBreakpoint altRecvBreak = await _engine.SetBreakAtFunction("wsock32", "recv");
                _engine.addCallback(altRecvBreak, async (bp) =>
                {
                    Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                    ulong socket = registers[DbgEngRegister.ESP] + this.pointerSize;
                    ulong bufferPtr = registers[DbgEngRegister.ESP] + (this.pointerSize * 2);
                    ulong bufferSize = registers[DbgEngRegister.ESP] + (this.pointerSize * 3);
                    ulong flags = registers[DbgEngRegister.ESP] + (this.pointerSize * 4);

                    short trueSize = await _engine.ReadSignedWord(bufferSize);
                    ulong dataBuffer = await _engine.ReadPointer(bufferPtr);
                    MessageBox.Show($"Socket: {$"0x{socket:X}"}{Environment.NewLine}" +
                        $"Buffer Pointer: {$"0x{dataBuffer:X}"}{Environment.NewLine}" +
                        $"Buffer Size: {trueSize}{Environment.NewLine}" +
                        $"Flags: {$"0x{flags:X}"}", "wsock32 Recv Called!", MessageBoxButtons.OK);
                    IDebugBreakpoint dataBufferBreak = await _engine.SetBreakAtMemory(dataBuffer);
                    _engine.addCallback(dataBufferBreak, async (dbp) =>
                    {
                        ulong instruct = await _engine.GetCurrentInstructionAddress();
                        string opcode = await _engine.GetCurrentOpcode();

                        MessageBox.Show($"0x{instruct:X} - {opcode}", "Data Buffer Accessed!", MessageBoxButtons.OK);
                        await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                        await _engine.WaitForEvent();
                    });
                    await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                    await _engine.WaitForEvent();
                });
            }
        }

        private void handleModuleUnload(object sender, UnloadModuleEventArgs e)
        {
            treeModules.Nodes.RemoveByKey(e.ImageBaseName);
        }

        private void handleProcessCreate(object sender, CreateProcessEventArgs e)
        {
            MessageBox.Show($"Base Offset: {e.BaseOffset}{Environment.NewLine}" +
                $"CheckSum: {e.CheckSum}{Environment.NewLine}" +
                $"Handle: {e.Handle}{Environment.NewLine}" +
                $"Image File Handle: {e.ImageFileHandle}{Environment.NewLine}" +
                $"Image Name: {e.ImageName}{Environment.NewLine}" +
                $"Initial Thread Handle: {e.InitialThreadHandle}{Environment.NewLine}" +
                $"Module Name: {e.ModuleName}{Environment.NewLine}" +
                $"Module Size: {e.ModuleSize}{Environment.NewLine}" +
                $"Start Offset: {e.StartOffset}{Environment.NewLine}" +
                $"Thread Data Offset: {e.ThreadDataOffset}{Environment.NewLine}" +
                $"TimeDate Stamp: {e.TimeDateStamp}", "Process Created!");
        }

        private void handleProcessTerminate(object sender, ExitProcessEventArgs e)
        {

            //MessageBox.Show($"Exit Code: {e.ExitCode}", "Process Terminated!");
        }

        private void handleBreakpoint(object sender, BreakpointEventArgs e)
        {
            //MessageBox.Show($"Breakpoint!");//We cant translate this obj yet
            frmRefresh_Stats();
        }

        private void handleThreadCreate(object sender, CreateThreadEventArgs e)
        {
            //MessageBox.Show($"Handle: {e.Handle}{Environment.NewLine}" +
            //    $"Data Offset: {e.DataOffset}{Environment.NewLine}" +
            //    $"Start Offset: {e.StartOffset}", "Thread Created!");
            frmRefresh_Stats();
        }

        private void handleThreadTerminate(object sender, ExitThreadEventArgs e)
        {
            //MessageBox.Show($"Exit Code: {e.ExitCode}", "Thread Termniated!");
            frmRefresh_Stats();
        }

        private void handleSessionChange(object sender, SessionStatusEventArgs e)
        {
            labelStatus.SafeOperation(() =>
            {
                labelStatus.Text = e.Status.ToString();
            });
        }

        private void handleStateChange(object sender, DebuggeeStateEventArgs e)
        {
            labelState.SafeOperation(() =>
            {
                labelState.Text = e.Flags.ToString();
            });
        }

        private void handleEngineChange(object sender, EngineStateEventArgs e)
        {
            listEngineFlags.SafeOperation(() =>
            {
                listEngineFlags.UpdateOrAdd(e.Flags.ToString(), e.Argument.ToString());
            });
        }

        private void handleSymbolChange(object sender, SymbolStateEventArgs e)
        {
            
        }

        private void handleSystemError(object sender, SystemErrorEventArgs e)
        {
            
        }

        #endregion

        private async void btnContinue_Click(object sender, EventArgs e)
        {
            if (_attached && _engine != null)
            {
                await _engine.SetExecutionStatus(DEBUG_STATUS.GO);
                await _engine.WaitForEvent();
                btnBreak.Enabled = true;
                btnContinue.Enabled = false;
            }
        }

        private async void btnBreak_Click(object sender, EventArgs e)
        {
            if (_attached && _engine != null)
            {
                btnBreak.Enabled = false;
                bool broken = await _engine.Break(true);
                btnContinue.Enabled = broken;
                btnBreak.Enabled = !broken;
            }
        }
    }
}
