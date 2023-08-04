using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Runtime.InteropServices;
using System.Xml;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace WinDbgKiller
{
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

        private bool _disposed;
        private IDebugClient debugger;
        private IntPtr debuggerPtr;
        private IDebugControl controller;
        private IntPtr controllerPtr;
        private IDebugSymbols symbols;
        private IntPtr symbolsPtr;
        private IDebugSystemObjects debugSystemObjects;
        private IntPtr debugSystemObjectsPtr;
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
            debugger = (IDebugClient)Marshal.GetTypedObjectForIUnknown(interfacePtr, typeof(IDebugClient));
            debuggerPtr = interfacePtr;
            HResult = Marshal.QueryInterface(debuggerPtr, ref IID_IDebugControl, out IntPtr ppv);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult:x}");
            }
            controller = (IDebugControl)Marshal.GetTypedObjectForIUnknown(ppv, typeof(IDebugControl));
            controllerPtr = ppv;
            HResult = Marshal.QueryInterface(debuggerPtr, ref IID_IDebugSymbols, out IntPtr pppv);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult:x}");
            }
            symbols = (IDebugSymbols)Marshal.GetTypedObjectForIUnknown(pppv, typeof(IDebugSymbols));
            symbolsPtr = pppv;
            //HResult = Marshal.QueryInterface(debuggerPtr, ref IID_IDebugSystemObjects, out IntPtr ppppv);
            //if (!HResult.Succeeded)
            //{
            //    throw new Exception($"HResult: {HResult:x}");
            //}
            //debugSystemObjects = (IDebugSystemObjects)Marshal.GetTypedObjectForIUnknown(ppppv, typeof(IDebugSystemObjects));
            //debugSystemObjectsPtr = ppppv;
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
            HRESULT HResult = debugger.AttachProcess(0, pid, DEBUG_ATTACH.DEFAULT);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult}");
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

        public WinDbgEngine(string exePath, bool runOnExecute) : this()
        {
            if (!File.Exists(exePath) || Path.GetExtension(exePath).ToLower() != ".exe")
            {
                throw new FileNotFoundException();
            }
            if (Is32BitProcess(exePath) == Environment.Is64BitProcess)
            {
                DialogResult response = MessageBox.Show($"The requested executable has an architecture that differs from this client ({(Is32BitProcess(exePath) ? "x32" : "x64")}). Would you like to run the {(Environment.Is64BitProcess ? "x32" : "x64")} bit client?", "Executable Architecture Mismatch", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (response == DialogResult.Yes)
                {
                    Process proc = new Process();
                    string path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, $"{(Is32BitProcess(exePath) ? "x86" : "x64")}/Debug/WinDbgKiller.exe");
                    MessageBox.Show(path);
                    proc.StartInfo.FileName = path;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                    Environment.Exit(0);
                }
            }
            HRESULT HResult = debugger.CreateProcessAndAttach(0, exePath, DEBUG_CREATE_PROCESS.NO_DEBUG_HEAP, 0, DEBUG_ATTACH.DEFAULT);
            if (!HResult.Succeeded)
            {
                throw new Exception($"HResult: {HResult}");
            }
            else
            {
                controller.SetInterrupt(DEBUG_INTERRUPT.ACTIVE);
                controller.WaitForEvent((uint)DEBUG_WAIT.DEFAULT, uint.MaxValue);
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
                this.ReleaseObj(ref controllerPtr);
                this.ReleaseObj(ref symbolsPtr);
                this.ReleaseObj(ref debugSystemObjectsPtr);
                _disposed = true;
            }
        }
    }
}
