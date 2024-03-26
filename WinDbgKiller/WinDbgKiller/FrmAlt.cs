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
        public FrmAlt()
        {
            InitializeComponent();
        }

        private void FrmAlt_Load(object sender, EventArgs e)
        {
            txtOutput.ScrollBars = ScrollBars.Vertical;
            this.Text = this.Text + $" - {(Environment.Is64BitProcess ? "x64" : "x86")}";
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
                Dictionary<DbgEngRegister, ulong> registers = await _engine.listRegisters();
                listRegisters.SafeOperation(() =>
                {
                    foreach (KeyValuePair<DbgEngRegister, ulong> register in registers)
                    {
                        listRegisters.UpdateOrAdd(register.Key.ToString(), $"0x{register.Value.ToString("X")}");
                    }
                });
                List<DEBUG_STACK_FRAME> stack = await _engine.listStack();
                listStack.SafeOperation(async () =>
                {
                    if (stack != null)
                    {
                        listStack.Items.Clear();
                        foreach (DEBUG_STACK_FRAME frame in stack)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = await _engine.GetFunctionFromFrame(frame);
                            lvi.SubItems.Add(frame.Virtual.ToString());
                            listStack.Items.Add(lvi);
                        }
                    }
                });
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
            //MessageBox.Show(e.Exception.ToString(), "Exception Hit!");
            MessageBox.Show($"First Chance: {e.FirstChance}{Environment.NewLine}" +
                $"Exception - Address: {e.Exception.ExceptionAddress}{Environment.NewLine}" +
                $"Exception - Code: {e.Exception.ExceptionCode}{Environment.NewLine}" +
                $"Exception - Flags: {e.Exception.ExceptionFlags}{Environment.NewLine}" +
                $"Exception - Record: {e.Exception.ExceptionRecord}{Environment.NewLine}" +
                $"Exception - Number Parameters: {e.Exception.NumberParameters}", "Exception Hit!");
        }

        private void handleModuleLoad(object sender, LoadModuleEventArgs e)
        {
            MessageBox.Show($"Base Offset: {e.BaseOffset}{Environment.NewLine}" +
                $"Checksum: {e.Checksum}{Environment.NewLine}" +
                $"Image File Handle: {e.ImageFileHandle}{Environment.NewLine}" +
                $"Image Name: {e.ImageName}{Environment.NewLine}" +
                $"Module Name: {e.ModuleName}{Environment.NewLine}" +
                $"Module Size: {e.ModuleSize}{Environment.NewLine}" +
                $"TimeDate Stamp: {e.TimeDateStamp}{Environment.NewLine}", "Importted!");
        }

        private void handleModuleUnload(object sender, UnloadModuleEventArgs e)
        {
            MessageBox.Show($"Base Offset: {e.BaseOffset}{Environment.NewLine}" +
                $"Image Base Name: {e.ImageBaseName}", "Unloaded!");
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
            MessageBox.Show($"Exit Code: {e.ExitCode}", "Process Terminated!");
        }

        private void handleBreakpoint(object sender, BreakpointEventArgs e)
        {
            MessageBox.Show($"Breakpoint!");//We cant translate this obj yet
        }

        private void handleThreadCreate(object sender, CreateThreadEventArgs e)
        {
            MessageBox.Show($"Handle: {e.Handle}{Environment.NewLine}" +
                $"Data Offset: {e.DataOffset}{Environment.NewLine}" +
                $"Start Offset: {e.StartOffset}", "Thread Created!");
        }

        private void handleThreadTerminate(object sender, ExitThreadEventArgs e)
        {
            MessageBox.Show($"Exit Code: {e.ExitCode}", "Thread Termniated!");
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
