using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.ExceptionServices;

namespace WinDbgKiller
{
    public struct ModuleInfo
    {
        public ulong ImageFileHandle;
        public ulong BaseOffset;
        public uint ModuleSize;
        public string ModuleName;
        public string ImageName;
        public uint CheckSum;
        public uint TimeDateStamp;

        public ModuleInfo(ulong imageFileHandle, ulong baseOffset, uint moduleSize, string moduleName, string imageName, uint checkSum, uint timeDateStamp)
        {
            ImageFileHandle = imageFileHandle;
            BaseOffset = baseOffset;
            ModuleSize = moduleSize;
            ModuleName = moduleName;
            ImageName = imageName;
            CheckSum = checkSum;
            TimeDateStamp = timeDateStamp;
        }
    }

    public struct ExceptionInfo
    {
        public EXCEPTION_RECORD64 Record;
        public bool FirstChance;

        public ExceptionInfo(EXCEPTION_RECORD64 record, uint firstChance)
        {
            Record = record;
            FirstChance = firstChance != 0;
        }
    }
    public partial class DbgEngine: IDebugOutputCallbacks, IDisposable
    {
        [DllImport("dbgeng.dll")]
        private static extern int DebugCreate(in Guid InterfaceId, out IntPtr pDebugClient);

        private static readonly object s_sync = new object();
        private bool _disposed;

        private readonly IDebugClient _client;
        private readonly IDebugControl _control;
        private readonly StringBuilder _result = new StringBuilder(1024);
        private readonly DEBUG_OUTPUT _mask;

        /// <summary>
        /// Memory dump file name
        /// </summary>
        public string DumpFileName { get; }

        /// <summary>
        /// ClrMD DataTarget instance
        /// </summary>
        public DataTarget DataTargetInstance { get; }

        /// <summary>
        /// Creates and instance of debugger and also initialize DataTarget
        /// </summary>
        /// <param name="dumpfilename">Memory dump file name</param>
        public DbgEngine(string dumpfilename)
        {
            _mask = DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.SYMBOLS | DEBUG_OUTPUT.ERROR | DEBUG_OUTPUT.WARNING | DEBUG_OUTPUT.DEBUGGEE;

            Guid guid = new Guid("{27fe5639-8407-4f47-8364-ee118fb08ac8}");
            int hr = DebugCreate(guid, out IntPtr pDebugClient);
            if (hr < 0)
            {
                throw new Exception($"Failed to OpenDumpFile, hr={hr:x}.");
            }

            _client = (IDebugClient)Marshal.GetTypedObjectForIUnknown(pDebugClient, typeof(IDebugClient));
            _control = (IDebugControl)_client;

            hr = _client.OpenDumpFile(dumpfilename);
            if (hr < 0)
            {
                throw new Exception($"Failed to OpenDumpFile, hr={hr:x}.");
            }

            hr = _control.WaitForEvent(DEBUG_WAIT.DEFAULT, 10000);
            if (hr < 0)
            {
                throw new Exception($"Failed to attach to dump file, hr={hr:x}.");
            }

            Marshal.Release(pDebugClient);

            DataTargetInstance = DataTarget.CreateFromDebuggerInterface((IDebugClient)Marshal.GetTypedObjectForIUnknown(pDebugClient, typeof(IDebugClient)));

        }

        public string Execute(string cmd)
        {
            string result = string.Empty;
            lock (s_sync)
            {
                _client.GetOutputCallbacks(out IDebugOutputCallbacks callbacks);
                try
                {
                    int hr = _client.SetOutputCallbacks(this);
                    if (hr < 0)
                    {
                        return null;
                    }
                    hr = _control.Execute(DEBUG_OUTCTL.THIS_CLIENT, cmd, DEBUG_EXECUTE.DEFAULT);
                    if (hr < 0)
                    {
                        _result.Append($"Command encountered an error. HRESULT={hr:x}.");
                    }
                    result = _result.ToString();
                }
                finally
                {
                    if (callbacks != null)
                    {
                        _client.SetOutputCallbacks(callbacks);
                    }
                    _result.Clear();
                }
            }
            return result;
        }

        public int Output(DEBUG_OUTPUT Mask, string Text)
        {
            if ((_mask & Mask) == 0)
            {
                return 0;
            }
            lock (_result)
            {
                _result.Append(Text);
            }
            return 0;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DataTargetInstance.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => "Dump filename : " + DumpFileName;
    }

    public partial class DebugEngine : IDisposable, IDebugOutputCallbacks, IDebugEventCallbacksWide
    {
        [DllImport("dbgeng.dll")]
        internal static extern int DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object Interface);

        IDebugClient5 _client;
        IDebugControl4 _control;
        IDebugDataSpaces _debugDataSpace;

        bool _outputText = true;
        public void SetOutputText(bool output)
        {
            _outputText = output;
        }

        public bool BreakpointHit { get; set; }
        public bool StateChanged { get; set; }

        public delegate void ModuleLoadedDelegate(ModuleInfo modInfo);
        public ModuleLoadedDelegate ModuleLoaded;

        public delegate void ExceptionOccuredDelegate(ExceptionInfo exInfo);
        public ExceptionOccuredDelegate ExceptionOccured;

        public DebugEngine()
        {
            Guid guid = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
            object obj = null;
            int hr = DebugCreate(ref guid, out obj);
            if (hr < 0)
            {
                throw new Exception("Failed to create debugger interface!");
                return;
            }
            _client = obj as IDebugClient5;
            _control = obj as IDebugControl4;
            _debugDataSpace = obj as IDebugDataSpaces;
            _client.SetOutputCallbacks(this);
            _client.SetEventCallbacksWide(this);
        }

        public void DbgTest(string exePath)
        {
            Guid COM_IDebugClient = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
            object obj = null;
            HRESULT hResult = DebugCreate(ref COM_IDebugClient, out obj);
            IDebugClient debugger = obj as IDebugClient;
            hResult = debugger.CreateClient(out debugger);
            if (hResult.Succeeded)
            {
                IDebugControl controller = (IDebugControl)debugger;
                IDebugSymbols symbols = (IDebugSymbols)debugger;
                IDebugSystemObjects sysObjects = (IDebugSystemObjects)debugger;
                hResult = debugger.CreateProcess(0, exePath, DEBUG_CREATE_PROCESS.DEFAULT);
                if (hResult == HRESULT.S_OK)
                {
                    hResult = controller.WaitForEvent((uint)DEBUG_WAIT.DEFAULT, uint.MaxValue);
                    if (hResult == HRESULT.S_OK)
                    {
                        symbols.SetSymbolPath("srv*");
                        uint processCount;
                        controller.GetNumberProcessors(out processCount);
                        MessageBox.Show($"Processors: {processCount}");
                        StringBuilder stackTrace = new StringBuilder(2048);
                        DEBUG_STACK_FRAME[] frames = new DEBUG_STACK_FRAME[64];
                        uint framesFilled;
                        hResult = controller.GetStackTrace(0, 0, 0, frames, 64, out framesFilled);
                        if (hResult == HRESULT.S_OK)
                        {
                            for (uint i = 0; i < framesFilled; i++)
                            {
                                ulong offset;
                                StringBuilder name = new StringBuilder(512);
                                uint nameSize;
                                symbols.GetNameByOffset(frames[i].InstructionOffset, name, 512, out nameSize, out offset);
                                stackTrace.AppendLine($"{name} + 0x{offset:x}");
                            }
                            MessageBox.Show(stackTrace.ToString());
                        }
                        else
                        {
                            MessageBox.Show("Failed to grab stack trace!");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Failed to await event! {hResult}");
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to launch process! {hResult}");
                }
                debugger.DetachProcesses();
                debugger.EndSession(DEBUG_END.ACTIVE_DETACH);
            }
            else
            {
                MessageBox.Show("Failed to create debugger!");
            }
        }

        public void Rebuild()
        {
            IDebugClient debug;
            _client.CreateClient(out debug);
            IDebugControl control = (IDebugControl)debug;
            _client = debug as IDebugClient5;
            _control = debug as IDebugControl4;
            _debugDataSpace = debug as IDebugDataSpaces;
        }

        public int ReadMemory(ulong address, uint size, out byte[] buf)
        {
            buf = new byte[size];
            _debugDataSpace.ReadVirtual(address, buf, (uint)buf.Length, out uint readBytes);
            return (int)readBytes;
        }

        public bool AttachTo(int pid)
        {
            int hr = _client.AttachProcess(0, (uint)pid, DEBUG_ATTACH.DEFAULT);
            if (hr < 0)
            {
                MessageBox.Show($"Failed to attach error {hr}");
                MessageBox.Show($"Failed to attach error 0x{hr:X8}");
            }
            return hr >= 0;
        }

        public bool AttachViaCommand(int pid)
        {
            int hr = this.ExecuteWide($".attach {pid}");
            if (hr < 0)
            {
                MessageBox.Show("Failed to execute command");
            }
            return hr >= 0;
        }

        public int GetExecutionStatus(out DEBUG_STATUS status)
        {
            return _control.GetExecutionStatus(out status);
        }

        public void OutputCurrentState(DEBUG_OUTCTL outputControl, DEBUG_CURRENT flags)
        {
            _control.OutputCurrentState(outputControl, flags);
        }

        public void OutputPromptWide(DEBUG_OUTCTL outputControl, string format)
        {
            _control.OutputPromptWide(outputControl, format);
        }

        public void FlushCallbacks()
        {
            _client.FlushCallbacks();
        }

        public int Execute(string command)
        {
            return _control.Execute(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT);
        }

        public int Execute(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags)
        {
            return _control.Execute(outputControl, command, flags);
        }

        public int ExecuteWide(string command)
        {
            return _control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT);
        }

        public int ExecuteWide(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags)
        {
            return _control.ExecuteWide(outputControl, command, flags);
        }

        public int WaitForEvent(DEBUG_WAIT flag = DEBUG_WAIT.DEFAULT, int timeout = Timeout.Infinite)
        {
            unchecked
            {
                return _control.WaitForEvent(flag, (uint)timeout);
            }
        }

        public void SetInterrupt(DEBUG_INTERRUPT flag = DEBUG_INTERRUPT.ACTIVE)
        {
            _control.SetInterrupt(flag);
        }

        public void Detach()
        {
            _client.DetachProcesses();
        }

        public void Dispose()
        {
            if (_debugDataSpace != null)
            {
                Marshal.ReleaseComObject(_debugDataSpace);
                _debugDataSpace = null;
            }
            if (_control != null)
            {
                Marshal.ReleaseComObject(_control);
                _control = null;
            }
            if (_client != null)
            {
                Marshal.ReleaseComObject(_client);
                _client = null;
            }
        }

        public int Output([In] DEBUG_OUTPUT Mask, [In, MarshalAs(UnmanagedType.LPStr)] string Text)
        {
            if (_outputText == false)
            {
                return 0;
            }
            MessageBox.Show(Text);
            return 0;
        }

        public int GetInterestMask([Out] out DEBUG_EVENT Mask)
        {
            Mask = DEBUG_EVENT.BREAKPOINT | DEBUG_EVENT.CHANGE_DEBUGGEE_STATE
                | DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.CHANGE_SYMBOL_STATE
                | DEBUG_EVENT.CREATE_PROCESS | DEBUG_EVENT.CREATE_THREAD | DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS
                | DEBUG_EVENT.EXIT_THREAD | DEBUG_EVENT.LOAD_MODULE | DEBUG_EVENT.SESSION_STATUS | DEBUG_EVENT.SYSTEM_ERROR
                | DEBUG_EVENT.UNLOAD_MODULE;
            return 0;
        }

        public int Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp)
        {
            BreakpointHit = true;
            StateChanged = true;
            return (int)DEBUG_STATUS.BREAK;
        }

        public int Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp)
        {
            BreakpointHit = true;
            StateChanged = true;
            return (int)DEBUG_STATUS.BREAK;
        }

        public int Exception([In] ref EXCEPTION_RECORD64 Exception, [In] uint FirstChance)
        {
            if (ExceptionOccured != null)
            {
                ExceptionInfo exInfo = new ExceptionInfo(Exception, FirstChance);
                ExceptionOccured(exInfo);
            }
            return (int)DEBUG_STATUS.BREAK;
        }

        public int CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset)
        {
            return 0;
        }

        public int ExitThread([In] uint ExitCode)
        {
            return 0;
        }

        public int CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPStr)] string ImageName,
            [In] uint CheckSum, [In] uint TimeDateStamp, [In] ulong InitialThreadHandle, [In] ulong ThreadDataOffset, [In] ulong StartOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public int ExitProcess([In] uint ExitCode)
        {
            return 0;
        }

        public int LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp)
        {
            if (ModuleLoaded != null)
            {
                ModuleInfo modInfo = default(ModuleInfo);
                try
                {
                    modInfo = new ModuleInfo(ImageFileHandle, BaseOffset, ModuleSize, ModuleName, ImageName, CheckSum, TimeDateStamp);
                    ModuleLoaded(modInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return 0;
        }

        public int UnloadModule([In, MarshalAs(UnmanagedType.LPStr)] string ImageBaseName, [In] ulong BaseOffset)
        {
            return 0;
        }

        public int SystemError([In] uint Error, [In] uint Level)
        {
            return 0;
        }

        public int SessionStatus([In] DEBUG_SESSION Status)
        {
            return 0;
        }

        public int ChangeDebuggeeState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            return 0;
        }

        public int ChangeEngineState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            return 0;
        }

        public int ChangeSymbolState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            return 0;
        }

        int IDebugEventCallbacksWide.ChangeEngineState(DEBUG_CES Flags, ulong Argument)
        {
            //throw new NotImplementedException();
            return 0;
        }

        int IDebugEventCallbacksWide.ChangeSymbolState(DEBUG_CSS Flags, ulong Argument)
        {
            //throw new NotImplementedException();
            return 0;
        }
    }
    public partial class WinDbgEngine : IDisposable
    {

        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", CallingConvention = CallingConvention.StdCall)]
        private static extern int DebugCreate(in Guid InterfaceId, out IntPtr InterfacePtr);
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        private static Guid IID_IDebugClient = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
        private static Guid IID_IDebugControl = new Guid("5182e668-105e-416e-ad92-24ef800424ba");
        private static Guid IID_IDebugSymbols = new Guid("8c31e98c-983a-48a5-9016-6fe5d667a950");
        private static Guid IID_IDebugSystemObjects = new Guid("6b86fe2c-2c4f-4f0c-9da2-d9a7d55d4871");
        //private static Guid IID_IDebugRegisters = new Guid("");
        //private static Guid IID_IDebugDataSpaces = new Guid("");
        //private static Guid IID_IDebugAdvanced = new Guid("");

        #region Helper
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, StringBuilder lpBuffer, uint nSize, IntPtr Arguments);

        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        public static string GetFormatMessageFromHRESULT(int hResult)
        {
            StringBuilder buffer = new StringBuilder(255);
            uint result = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, IntPtr.Zero, (uint)hResult, 0, buffer, (uint)buffer.Capacity, IntPtr.Zero);
            if (result != 0)
            {
                return buffer.ToString();
            }
            else
            {
                MessageBox.Show($"Failed to read hResult of {hResult}");
                return $"Unknown error (0x{hResult:X8})";
            }
        }
        
        #endregion

        private bool _disposed;
        public IDebugClient6 debugger { get; private set; }
        private IntPtr debuggerPtr;
        public IDebugControl6 controller { get; private set; }
        public IDebugSymbols5 symbols { get; private set; }
        public IDebugSystemObjects3 debugSystemObjects { get; private set; }
        public IDebugAdvanced3 advanced { get; private set; }
        public IDebugDataSpaces4 dataSpaces { get; private set; }
        public IDebugRegisters2 registers { get; private set; }
        //private IDebugRegisters registers;
        //private IntPtr registersPtr;
        //private IDebugDataSpaces dataSpaces;
        //private IntPtr dataSpacesPtr;
        //private IDebugAdvanced advanced;
        //private IntPtr advancedPtr;

        public WinDbgEngine()
        {
            HRESULT HResult = DebugCreate(IID_IDebugClient, out IntPtr interfacePtr);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult:x}");
            }
            debugger = (IDebugClient6)Marshal.GetTypedObjectForIUnknown(interfacePtr, typeof(IDebugClient5));
            debuggerPtr = interfacePtr;
            controller = (IDebugControl6)debugger;
            symbols = (IDebugSymbols5)debugger;
            debugSystemObjects = (IDebugSystemObjects3)debugger;
            advanced = (IDebugAdvanced3)debugger;
            dataSpaces = (IDebugDataSpaces4)debugger;
            registers = (IDebugRegisters2)debugger;
        }

        private static bool Is32BitProcess(uint pid)
        {
            Process process = Process.GetProcessById((int)pid);
            bool isWow64;
            if (!IsWow64Process(process.Handle, out isWow64))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return !isWow64;
        }

        private static bool Is32BitProcess(string exePath)
        {
            byte[] data = new byte[4];
            using (FileStream fs = new FileStream(exePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(data, 0, 4);
            }
            if (Encoding.UTF8.GetString(data, 0, 2) != "MZ")
            {
                throw new Exception("The selected binary is not a valid PE file");
            }
            int peHeaderAddress;
            using (BinaryReader br = new BinaryReader(File.OpenRead(exePath)))
            {
                br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                peHeaderAddress = br.ReadInt32();
            }
            ushort machine;
            using (BinaryReader br = new BinaryReader(File.OpenRead(exePath)))
            {
                br.BaseStream.Seek(peHeaderAddress + 4, SeekOrigin.Begin);
                machine = br.ReadUInt16();
            }
            if (machine == 0x014C)
            {
                return true;
            }
            else if (machine == 0x8664)
            {
                return false;
            }
            throw new Exception("Unable to read the architecture of the selected binary");
        }

        public WinDbgEngine(uint pid) : this()
        {
            if (!Is32BitProcess(pid) != Environment.Is64BitProcess)
            {
                DialogResult response = MessageBox.Show($"The requested executable has an architecture that differs from this client ({(Is32BitProcess(pid) ? "x32" : "x64")}). Would you like to run the {(Environment.Is64BitProcess ? "x32" : "x64")} bit client?", "Executable Architecture Mismatch", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (response == DialogResult.Yes)
                {
                    Process proc = new Process();
                    string path = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName).FullName, $"{(Is32BitProcess(pid) ? "x86" : "x64")}\\Debug\\WinDbgKiller.exe");
                    MessageBox.Show(path);
                    proc.StartInfo.FileName = path;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                    Environment.Exit(0);
                }
            }
            HRESULT HResult = debugger.AttachProcess(0, (uint)pid, DEBUG_ATTACH.DEFAULT);
            MessageBox.Show(GetFormatMessageFromHRESULT(HResult));
            HResult = controller.AddEngineOptions(DEBUG_ENGOPT.INITIAL_BREAK);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult:x}");
            }
            else
            {
                controller.WaitForEvent((uint)DEBUG_WAIT.DEFAULT, uint.MaxValue);
            }
        }

        public WinDbgEngine(string pipe) : this()
        {
            //IDK how to check architecture here
            HRESULT HResult = debugger.AttachKernel(DEBUG_ATTACH.KERNEL_CONNECTION, $"com:pipe,port={pipe}");
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult:x}");
            }
            else
            {
                controller.WaitForEvent((uint)DEBUG_WAIT.DEFAULT, uint.MaxValue);
            }
        }

        public WinDbgEngine(string exePath, bool runOnExecute, IDebugOutputCallbacks _outputHandler = null, IDebugEventCallbacks _eventHandler = null) : this()
        {
            debugger.SetOutputCallbacks(_outputHandler);
            debugger.SetEventCallbacks(_eventHandler);
            if (!File.Exists(exePath) || Path.GetExtension(exePath).ToLower() != ".exe")
            {
                throw new FileNotFoundException();
            }
            if (Is32BitProcess(exePath) == Environment.Is64BitProcess)
            {
                DialogResult response = MessageBox.Show($"The requested executable has an architecture that differs from this client ({(Is32BitProcess(exePath) ? "x32" : "x64")}). Would you like to run the {(Environment.Is64BitProcess ? "x32" : "x64")} bit client?", "Executable Architecture Mismatch", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (response == DialogResult.Yes)
                {
                    debugger.SetOutputCallbacks(_outputHandler);
                    debugger.SetEventCallbacks(_eventHandler);
                    Process proc = new Process();
                    string path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, $"{(Is32BitProcess(exePath) ? "x86" : "x64")}/Debug/WinDbgKiller.exe");
                    MessageBox.Show(path);
                    proc.StartInfo.FileName = path;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                    Environment.Exit(0);
                }
            }
            HRESULT HResult = debugger.CreateProcess(0, exePath, DEBUG_CREATE_PROCESS.DEFAULT);
            MessageBox.Show(GetFormatMessageFromHRESULT(HResult));
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult}");
            }
        }

        public void InstallOutputHandler(IDebugOutputCallbacks method)
        {
            debugger.SetOutputCallbacks(method);
        }

        public void InstallBreakpointHandler(IDebugEventCallbacks breakpointHandler)
        {
            debugger.SetEventCallbacks(breakpointHandler);
        }

        public IDebugBreakpoint Break(IntPtr offset, bool memory = false)
        {
            IDebugBreakpoint breakpoint;
            controller.AddBreakpoint(memory ? DEBUG_BREAKPOINT_TYPE.DATA : DEBUG_BREAKPOINT_TYPE.CODE, 0, out breakpoint);
            breakpoint.SetOffset((ulong)offset);
            breakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
            return breakpoint;
        }

        public IDebugBreakpoint RemoveBreak(IntPtr offset)
        {
            uint breakCount;
            controller.GetNumberBreakpoints(out breakCount);
            for (uint i = 0; i < breakCount; i++)
            {
                IDebugBreakpoint breakpoint;
                controller.GetBreakpointByIndex(i, out breakpoint);
                ulong currentOffset;
                breakpoint.GetOffset(out currentOffset);
                if (currentOffset == (uint)offset)
                {
                    HRESULT response = controller.RemoveBreakpoint(breakpoint);
                    if (!response.Succeeded)
                    {
                        throw new Exception($"HResult: ${response}");
                    }
                    else
                    {
                        return breakpoint;
                    }
                }
            }
            return null;
        }

        public byte[] ReadMemory(IntPtr offset, uint length)
        {
            byte[] buff = new byte[length];
            HRESULT result = dataSpaces.ReadVirtual((ulong)offset, buff, length, out uint bytesRead);
            if (result.Succeeded)
            {
                return buff;
            }
            else
            {
                throw new Exception(result.ToString());
            }
            return null;
        }

        public void Execute(string cmd)
        {
            controller.Execute(DEBUG_OUTCTL.ALL_CLIENTS, cmd, DEBUG_EXECUTE.DEFAULT);
        }

        private void ReleaseObj(ref IntPtr obj)
        {
            if (obj != IntPtr.Zero)
            {
                Marshal.Release(obj);
                obj = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.ReleaseObj(ref debuggerPtr);
                _disposed = true;
            }
        }
    }
}
