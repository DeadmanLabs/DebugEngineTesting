using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime.Interop;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Security.Policy;

namespace WinDbgKiller
{
    public enum DbgEngRegister : uint
    {
        EDI = 4,
        ESI = 5,
        EAX = 9,
        EBX = 6,
        ECX = 8,
        EDX = 7,
        EBP = 10,
        EIP = 11,
        EFL = 13,
        ESP = 14,
        AX = 27,
        BX = 24,
        CX = 26,
        DX = 25,
        DI = 22,
        SI = 23,
        BP = 28,
        IP = 29,
        DS = 3,
        ES = 2,
        FS = 1,
        GS = 0,
        SS = 15,
        FL = 30,
        SP = 31,
        IOPL = 73,
        OF = 74,
        DF = 75,
        IF = 76,
        TF = 77,
        SF = 78,
        ZF = 79,
        AF = 80,
        PF = 81,
        CF = 82,
        VIP = 83,
        VIF = 84,
        K0 = 197,
        K1 = 198,
        K2 = 199,
        K3 = 200,
        K4 = 201,
        K5 = 202,
        K6 = 203,
        K7 = 204,
        DR0 = 16,
        DR1 = 17,
        DR2 = 18,
        DR3 = 19,
        DR6 = 20,
        DR7 = 21,
        CS = 12,
        AL = 35,
        BL = 32,
        CL = 34,
        DL = 33,
        AH = 39,
        BH = 36,
        CH = 38,
        DH = 37,
        FPCW = 40,
        FPSW = 41,
        FPTW = 42,
        FOPCODE = 43,
        FPIP = 44,
        FPIPSEL = 45,
        FPDP = 46,
        FPDPSEL = 47,
        MXCSR = 64,
    }
    /*

    [StructLayout(LayoutKind.Explicit)]
    public struct DEBUG_MODULE_AND_ID
    {
        [FieldOffset(0)]
        public ulong ModuleBase;
        [FieldOffset(8)]
        public ulong Id;
    }
    */

    public class PageGuard 
    {
        public const uint PAGE_READONLY = 0x02;
        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_GUARD = 0x100;
        public const uint PAGE_NOACCESS = 0x01;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, StringBuilder lpBuffer, uint nSize, IntPtr Arguments);

        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        public static KeyValuePair<uint, string> GetError()
        {
            uint errorCode = GetLastError();
            StringBuilder messageBuffer = new StringBuilder(1024);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, IntPtr.Zero, errorCode, 0, messageBuffer, (uint)messageBuffer.Capacity, IntPtr.Zero);
            string errorMessage = messageBuffer.ToString();
            KeyValuePair<uint, string> error = new KeyValuePair<uint, string>(errorCode, errorMessage);
            return error;
        }

        public static KeyValuePair<uint, string> GetError(uint code)
        {
            StringBuilder messageBuffer = new StringBuilder(1024);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, IntPtr.Zero, code, 0, messageBuffer, (uint)messageBuffer.Capacity, IntPtr.Zero);
            string errorMessage = messageBuffer.ToString();
            KeyValuePair<uint, string> error = new KeyValuePair<uint, string>(code, errorMessage);
            return error;
        }
    }

    public class Patches
    {
        IDebugSymbols5 _symbols;
        public Patches(Microsoft.Diagnostics.Runtime.Interop.IDebugSymbols5 syms)
        {
            _symbols = (IDebugSymbols5)syms;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEBUG_MODULE_AND_ID
        {
            public ulong ModuleBase;
            public ulong Id;
        }

        [ComImport, Guid("c65fa83e-1e69-475e-8e0e-b5d79e9cc17e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDebugSymbols5
        {
            [PreserveSig]
            int GetSymbolEntryString(ref DEBUG_MODULE_AND_ID symbolEntry, uint Which, StringBuilder buf, int BufferSize, out uint NameSize);
        }

        public string GetSymbolEntryStringPatch(Microsoft.Diagnostics.Runtime.Interop.DEBUG_MODULE_AND_ID symEnt)
        {
            DEBUG_MODULE_AND_ID symbolEntry = new DEBUG_MODULE_AND_ID();
            symbolEntry.ModuleBase = symEnt.ModuleBase;
            symbolEntry.Id = symEnt.Id;
            StringBuilder buffer = new StringBuilder(1024);
            uint nameSize;
            int hr = _symbols.GetSymbolEntryString(
                ref symbolEntry,
                0,
                buffer,
                buffer.Capacity,
                out nameSize);
            return buffer.ToString();
        }
    }

    public class Debugger : IDebugOutputCallbacks, IDebugEventCallbacksWide, IDisposable
    {
        [DllImport("dbgeng.dll")]
        internal static extern int DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object Interface);

        static List<DbgEngRegister> registersAsList = new List<DbgEngRegister>
        {
            DbgEngRegister.EDI,
            DbgEngRegister.ESI,
            DbgEngRegister.EAX,
            DbgEngRegister.EBX,
            DbgEngRegister.ECX,
            DbgEngRegister.EDX,
            DbgEngRegister.EBP,
            DbgEngRegister.EIP,
            DbgEngRegister.EFL,
            DbgEngRegister.ESP,
            DbgEngRegister.AX,
            DbgEngRegister.BX,
            DbgEngRegister.CX,
            DbgEngRegister.DX,
            DbgEngRegister.DI,
            DbgEngRegister.SI,
            DbgEngRegister.BP,
            DbgEngRegister.IP,
            DbgEngRegister.DS,
            DbgEngRegister.ES,
            DbgEngRegister.FS,
            DbgEngRegister.GS,
            DbgEngRegister.SS,
            DbgEngRegister.FL,
            DbgEngRegister.SP,
            DbgEngRegister.IOPL,
            DbgEngRegister.OF,
            DbgEngRegister.DF,
            DbgEngRegister.IF,
            DbgEngRegister.TF,
            DbgEngRegister.SF,
            DbgEngRegister.ZF,
            DbgEngRegister.AF,
            DbgEngRegister.PF,
            DbgEngRegister.CF,
            DbgEngRegister.VIP,
            DbgEngRegister.VIF,
            DbgEngRegister.K0,
            DbgEngRegister.K1,
            DbgEngRegister.K2,
            DbgEngRegister.K3,
            DbgEngRegister.K4,
            DbgEngRegister.K5,
            DbgEngRegister.K6,
            DbgEngRegister.K7,
            DbgEngRegister.DR0,
            DbgEngRegister.DR1,
            DbgEngRegister.DR2,
            DbgEngRegister.DR3,
            DbgEngRegister.DR6,
            DbgEngRegister.DR7,
            DbgEngRegister.CS,
            DbgEngRegister.AL,
            DbgEngRegister.BL,
            DbgEngRegister.CL,
            DbgEngRegister.DL,
            DbgEngRegister.AH,
            DbgEngRegister.BH,
            DbgEngRegister.CH,
            DbgEngRegister.DH,
            DbgEngRegister.FPCW,
            DbgEngRegister.FPSW,
            DbgEngRegister.FPTW,
            DbgEngRegister.FOPCODE,
            DbgEngRegister.FPIP,
            DbgEngRegister.FPIPSEL,
            DbgEngRegister.FPDP,
            DbgEngRegister.FPDPSEL,
            DbgEngRegister.MXCSR
        };

        public IDebugClient5 _client;
        public IDebugControl4 _control;
        public IDebugDataSpaces _debugDataSpace;
        public IDebugRegisters2 _registers;
        public IDebugAdvanced3 _advanced;
        public IDebugSymbols5 _symbols;
        public IDebugSystemObjects3 _sysObjects;
        public SymbolDebugger _symbolDbg;
        public Patches _patches;

        bool _outputText;
        public void SetOutputText(bool output)
        {
            _outputText = output;
        }

        public bool BreakpointHit { get; set; }
        public bool StateChanged { get; set; }

        public delegate void ModuleLoadedDelegate(ModuleInfo modInfo);
        public ModuleLoadedDelegate ModuleLoaded;

        public delegate void ExceptionOccurredDelegate(ExceptionInfo exInfo);
        public ExceptionOccurredDelegate ExceptionOccurred;

        public event EventHandler<OutputEventArgs> OnOutput;
        public event EventHandler<BreakpointEventArgs> OnBreakpoint;
        public event EventHandler<ExceptionEventArgs> OnException;
        public event EventHandler<CreateThreadEventArgs> OnThreadCreate;
        public event EventHandler<ExitThreadEventArgs> OnThreadTerminate;
        public event EventHandler<LoadModuleEventArgs> OnModuleLoad;
        public event EventHandler<UnloadModuleEventArgs> OnModuleUnload;
        public event EventHandler<CreateProcessEventArgs> OnProcessCreate;
        public event EventHandler<ExitProcessEventArgs> OnProcessTerminate;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;
        public event EventHandler<SessionStatusEventArgs> OnSessionChange;
        public event EventHandler<DebuggeeStateEventArgs> OnStateChange;
        public event EventHandler<EngineStateEventArgs> OnEngineChange;
        public event EventHandler<SymbolStateEventArgs> OnSymbolChange;

        public bool useCallbacks { get; set; }
        public Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint>> callbacks { get; private set; }
        public Dictionary<ulong, uint> guardedPages { get; private set; }
        private BlockingCollection<Action> _actionQueue = new BlockingCollection<Action>();
        private Thread _executorThread;
        private bool disposing = false;
        private bool breakOnException;
        private int processId { get; set; }

        public Debugger(bool breakOnExceptions = false)
        {
            _executorThread = new Thread(ExecutorLoop);
            _executorThread.Start();
            callbacks = new Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint>>();
            guardedPages = new Dictionary<ulong, uint>();
            this.breakOnException = breakOnExceptions;
            
            EnqueueAction(() =>
            {
                Guid guid = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
                object obj = null;

                int hr = DebugCreate(ref guid, out obj);

                if (hr < 0)
                {
                    Console.WriteLine("SourceFix: Unable to acquire client interface");
                    return;
                }

                _client = obj as IDebugClient5;
                _control = _client as IDebugControl4;
                _debugDataSpace = _client as IDebugDataSpaces;
                _registers = _client as IDebugRegisters2;
                _advanced = _client as IDebugAdvanced3;
                _symbols = _client as IDebugSymbols5;
                _sysObjects = _client as IDebugSystemObjects3;
                _client.SetOutputCallbacks(this);
                _client.SetEventCallbacksWide(this);
                _control.AddEngineOptions(DEBUG_ENGOPT.INITIAL_BREAK);
                _symbols.SetSymbolPath("srv*http://msdl.microsoft.com/download/symbols");
                _patches = new Patches(_symbols);
            });
        }

        private void ExecutorLoop()
        {
            foreach (var action in _actionQueue.GetConsumingEnumerable())
            {
                action.Invoke();
            }
        }

        private void EnqueueAction(Action action)
        {
            _actionQueue.Add(action);
        }

        public async Task<int> ReadMemory(ulong address, uint size, byte[] buf)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, buf, size, out uint readBytes);
                tcs.SetResult((int)readBytes);
            });
            return await tcs.Task;
        }

        public async Task<IntPtr> ReadNativePointer(ulong address, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<IntPtr>();
            if (Environment.Is64BitProcess)
            {
                byte[] ptr = new byte[8];
                EnqueueAction(() =>
                {
                    _debugDataSpace.ReadVirtual(address, ptr, 8, out _);
                    if (littleEndian) Array.Reverse(ptr);
                    tcs.SetResult(new IntPtr(BitConverter.ToInt64(ptr, 0)));
                });
            }
            else
            {
                byte[] ptr = new byte[4];
                EnqueueAction(() =>
                {
                    _debugDataSpace.ReadVirtual(address, ptr, 4, out _);
                    if (littleEndian) Array.Reverse(ptr);
                    tcs.SetResult(new IntPtr(BitConverter.ToInt32(ptr, 0)));
                });
            }
            return await tcs.Task;
        }

        public async Task<short> ReadSignedWord(ulong address, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<short>();
            byte[] word = new byte[2];
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, word, 2, out _);
                if (littleEndian) Array.Reverse(word);
                tcs.SetResult(BitConverter.ToInt16(word, 0));
            });
            return await tcs.Task;
        }

        public async Task<byte> ReadByte(ulong address)
        {
            var tcs = new TaskCompletionSource<byte>();
            byte[] myByte = new byte[1];
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, myByte, 1, out _);
                tcs.SetResult(myByte[0]);
            });
            return await tcs.Task;
        }

        public async Task<byte[]> ReadBytes(ulong address, uint size = 8, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            byte[] byteArray = new byte[size];
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, byteArray, size, out _);
                tcs.SetResult(byteArray);
            });
            return await tcs.Task;
        }

        public async Task<ulong> ReadPointer(ulong address, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<ulong>();
            if (Environment.Is64BitProcess)
            {
                byte[] ptr = new byte[8];
                EnqueueAction(() =>
                {
                    _debugDataSpace.ReadVirtual(address, ptr, 8, out _);
                    if (littleEndian) Array.Reverse(ptr);
                    tcs.SetResult(BitConverter.ToUInt64(ptr, 0));
                });
            }
            else
            {
                byte[] ptr = new byte[4];
                EnqueueAction(() =>
                {
                    _debugDataSpace.ReadVirtual(address, ptr, 4, out _);
                    if (littleEndian) Array.Reverse(ptr);
                    tcs.SetResult((ulong)BitConverter.ToUInt32(ptr, 0));
                });
            }
            return await tcs.Task;
        }

        public async Task<int> ReadSignedDWord(ulong address, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<int>();
            byte[] dword = new byte[4];
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, dword, 4, out _);
                if (littleEndian) Array.Reverse(dword);
                tcs.SetResult(BitConverter.ToInt32(dword, 0));
            });
            return await tcs.Task;
        }

        public async Task<uint> ReadUnsignedDWord(ulong address, bool littleEndian = false)
        {
            var tcs = new TaskCompletionSource<uint>();
            byte[] dword = new byte[4];
            EnqueueAction(() =>
            {
                _debugDataSpace.ReadVirtual(address, dword, 4, out _);
                if (littleEndian) Array.Reverse(dword);
                tcs.SetResult(BitConverter.ToUInt32(dword, 0));
            });
            return await tcs.Task;
        }

        public async Task<KeyValuePair<AddressFamily, IPEndPoint>> ReadSocketAddress(ulong address, bool littleEndian = false)
        {
            //Its little endian, the ip is backwards
            AddressFamily family = (AddressFamily)(await ReadSignedWord(await ReadPointer(address, littleEndian), littleEndian));
            byte[] portRaw = await ReadBytes(await ReadPointer(address, littleEndian) + 2, 2, littleEndian);
            //MessageBox.Show($"Raw Port: {BitConverter.ToString(portRaw)}", "Port Info", MessageBoxButtons.OK);
            int port = portRaw[0] * 256 + portRaw[1];
            string ipAddress = $"{await ReadByte(await ReadPointer(address, littleEndian) + 4)}.{await ReadByte(await ReadPointer(address, littleEndian) + 5)}.{await ReadByte(await ReadPointer(address, littleEndian) + 6)}.{await ReadByte(await ReadPointer(address, littleEndian) + 7)}";
            KeyValuePair<AddressFamily, IPEndPoint> socketAddress = new KeyValuePair<AddressFamily, IPEndPoint>(family, new IPEndPoint(IPAddress.Parse(ipAddress), port));
            return socketAddress;
        }

        public async Task<bool> Break(bool blocking)
        {
            var tcs = new TaskCompletionSource<bool>();
            EnqueueAction(() =>
            {
                int hr = _control.SetInterrupt(DEBUG_INTERRUPT.ACTIVE);
                if (hr != 0)
                {
                    tcs.SetResult(false);
                    return;
                }
                hr = _control.WaitForEvent(DEBUG_WAIT.DEFAULT, blocking ? uint.MaxValue : 0);
                if (hr != 0)
                {
                    tcs.SetResult(false);
                    return;
                }
                tcs.SetResult(true);
            });
            return await tcs.Task;
        }

        public async Task<string> getModuleNameFromOffset(ulong baseOffset)
        {
            var tcs = new TaskCompletionSource<string>();
            EnqueueAction(() =>
            {
                StringBuilder moduleName = new StringBuilder(512);
                uint Index, nameSize;
                ulong Base;
                int hr = _symbols.GetModuleByOffset(baseOffset, 0, out Index, out Base);
                if (hr != 0)
                {
                    tcs.SetResult("");
                    return;
                }
                hr = _symbols.GetModuleNameString(DEBUG_MODNAME.MODULE, Index, 0, moduleName, (uint)moduleName.Capacity, out nameSize);
                if (hr != 0)
                {
                    tcs.SetResult("");
                    return;
                }
                tcs.SetResult(moduleName.ToString());
            });
            return await tcs.Task;
        }

        public async Task<Dictionary<ulong, string>> GetModuleFuncs(ulong baseOffset)
        {
            var tcs = new TaskCompletionSource<Dictionary<ulong, string>>();
            EnqueueAction(() =>
            {
                Dictionary<ulong, string> symbols = new Dictionary<ulong, string>();
                StringBuilder moduleName = new StringBuilder(1024);
                uint Index, nameSize;
                ulong Base;
                int hr = _symbols.GetModuleByOffset(baseOffset, 0, out Index, out Base);
                if (hr != 0)
                {
                    tcs.SetResult(symbols);
                    return;
                }
                hr = _symbols.GetModuleNameString(DEBUG_MODNAME.MODULE, Index, 0, moduleName, (uint)moduleName.Capacity, out nameSize);
                if (hr != 0)
                {
                    tcs.SetResult(symbols);
                    return;
                }
                ulong handle, symbolOffset;
                uint matchSize;
                StringBuilder symbolName = new StringBuilder(1024);
                hr = _symbols.StartSymbolMatch($"{moduleName.ToString()}!*", out handle);
                if (hr == 0)
                {
                    while(_symbols.GetNextSymbolMatch(handle, symbolName, symbolName.Capacity, out matchSize, out symbolOffset) == 0)
                    {
                        if (!symbols.Keys.Contains(symbolOffset) && symbolName.ToString() != "")
                        {
                            //MessageBox.Show($"{symbolOffset} -> {symbolName.ToString()}");
                            symbols.Add(symbolOffset, symbolName.ToString().Split('!')[1]);
                            symbolName.Clear();
                        }
                    }
                    _symbols.EndSymbolMatch(handle);
                }
                tcs.SetResult(symbols);
            });
            return await tcs.Task;
        }

        /// <summary>
        /// Custom Kernel Attach
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns>bool</returns>
        public async Task<bool> KernelAttachTo(string pipe)
        {
            var tcs = new TaskCompletionSource<bool>();

            EnqueueAction(() =>
            {
                int hr = _client.AttachKernel(DEBUG_ATTACH.KERNEL_CONNECTION, pipe);
                tcs.SetResult(hr >= 0);
                if (hr >= 0)
                {
                    //... Figure out kernel symbols later
                }
            });
            return await tcs.Task;
        }

        /// <summary>
        /// Custom Kernel Attach
        /// </summary>
        /// <param name="attachParams"></param>
        /// <param name="connectionOptions"></param>
        /// <returns></returns>
        public async Task<bool> KernelAttachTo(DEBUG_ATTACH attachParams, string connectionOptions)
        {
            var tcs = new TaskCompletionSource<bool>();

            EnqueueAction(() =>
            {
                int hr = _client.AttachKernel(attachParams, connectionOptions);
                tcs.SetResult(hr >= 0);
                if (hr >= 0)
                {
                    //... Figure out kernel symbols later
                }
            });
            return await tcs.Task;
        }

        public async Task<bool> AttachTo(int pid)
        {
            var tcs = new TaskCompletionSource<bool>();

            EnqueueAction(() =>
            {
                int hr = _client.AttachProcess(0, (uint)pid, DEBUG_ATTACH.DEFAULT);
                this.processId = pid;
                tcs.SetResult(hr >= 0);
                if (hr >= 0)
                {
                    //... Figure out kernel symbols later
                }
            });
            return await tcs.Task;
        }

        public async Task<bool> AttachTo(Process proc)
        {
            var tcs = new TaskCompletionSource<bool>();
            EnqueueAction(() =>
            {
                int hr = _client.AttachProcess(0, (uint)proc.Id, DEBUG_ATTACH.DEFAULT);
                this.processId = proc.Id;
                tcs.SetResult(hr >= 0);
                if (hr >= 0)
                {
                    _symbolDbg = new SymbolDebugger(proc);
                    if (this.breakOnException)
                    {
                        
                    }
                }
            });
            return await tcs.Task;
        }

        public async Task<(int Result, DEBUG_STATUS Status)> GetExecutionStatus()
        {
            var tcs = new TaskCompletionSource<(int, DEBUG_STATUS)>();

            EnqueueAction(() =>
            {
                int ret = int.MinValue;
                DEBUG_STATUS status = DEBUG_STATUS.TIMEOUT;
                try
                {
                    ret = _control.GetExecutionStatus(out status);
                    Console.WriteLine($"STATUS -> {status.ToString()}");
                }
                catch (InvalidCastException)
                {
                    try
                    {
                        ret = (_client as IDebugControl3).GetExecutionStatus(out status);
                        Console.WriteLine($"STATUS -> {status.ToString()}");
                    }
                    catch (InvalidCastException)
                    {
                        try
                        {
                            ret = (_client as IDebugControl2).GetExecutionStatus(out status);
                            Console.WriteLine($"STATUS -> {status.ToString()}");
                        }
                        catch (InvalidCastException)
                        {
                            try
                            {
                                ret = (_client as IDebugControl).GetExecutionStatus(out status);
                                Console.WriteLine($"STATUS -> {status.ToString()}");
                            }
                            catch (InvalidCastException)
                            {
                                MessageBox.Show("I tried everything man...");
                            }
                        }
                    }
                }
                tcs.SetResult((ret, status));
            });
            return await tcs.Task;
        }

        public void OutputCurrentState(DEBUG_OUTCTL outputControl, DEBUG_CURRENT flags)
        {
            EnqueueAction(() =>
            {
                _control.OutputCurrentState(outputControl, flags);
            });
        }

        public void OutputPromptWide(DEBUG_OUTCTL outputControl, string format)
        {
            EnqueueAction(() =>
            {
                _control.OutputPromptWide(outputControl, format);
            });
        }

        public void FlushCallbacks()
        {
            EnqueueAction(() =>
            {
                _client.FlushCallbacks();
            });
        }

        public async Task<int> Execute(string command)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                tcs.SetResult(_control.Execute(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT));
            });
            return await tcs.Task;
        }

        public async Task<int> Execute(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                tcs.SetResult(_control.Execute(outputControl, command, flags));
            });
            return await tcs.Task;
        }

        public async Task<int> ExecuteWide(string command)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                tcs.SetResult(_control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT));
            });
            return await tcs.Task;
        }

        public async Task<int> ExecuteWide(DEBUG_OUTCTL outputControl, string command, DEBUG_EXECUTE flags)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                tcs.SetResult(_control.ExecuteWide(outputControl, command, flags));
            });
            return await tcs.Task;
        }

        public async Task<int> WaitForEvent(DEBUG_WAIT flag = DEBUG_WAIT.DEFAULT, int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                unchecked
                {
                    tcs.SetResult(_control.WaitForEvent(flag, (uint)timeout));
                }
            });
            return await tcs.Task;
        }

        public void SetInterrupt(DEBUG_INTERRUPT flag = DEBUG_INTERRUPT.ACTIVE)
        {
            EnqueueAction(() =>
            {
                _control.SetInterrupt(flag);
            });
        }

        public void Detach()
        {
            EnqueueAction(() =>
            {
                _client.DetachProcesses();
            });
        }

        public void Dispose()
        {
            _actionQueue.CompleteAdding();
            _executorThread.Join();
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

            switch (Mask)
            {
                case DEBUG_OUTPUT.DEBUGGEE:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                case DEBUG_OUTPUT.PROMPT:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                case DEBUG_OUTPUT.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case DEBUG_OUTPUT.EXTENSION_WARNING:
                case DEBUG_OUTPUT.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case DEBUG_OUTPUT.SYMBOLS:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            OnOutput?.Invoke(this, new OutputEventArgs(Mask, Text));
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
            OnBreakpoint?.Invoke(this, new BreakpointEventArgs(Bp));
            IDebugBreakpoint representative = (IDebugBreakpoint)Bp;
            if (useCallbacks && callbacks.ContainsKey(representative))
            {
                callbacks[representative](representative);
            }
            BreakpointHit = true;
            StateChanged = true;
            return (int)DEBUG_STATUS.BREAK;
        }

        public int Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp)
        {
            OnBreakpoint?.Invoke(this, new BreakpointEventArgs(Bp));
            if (useCallbacks && callbacks.ContainsKey(Bp))
            {
                callbacks[Bp](Bp);
            }
            BreakpointHit = true;
            StateChanged = true;
            return (int)DEBUG_STATUS.BREAK;
        }

        public int Exception([In] ref EXCEPTION_RECORD64 Exception, [In] uint FirstChance)
        {
            OnException?.Invoke(this, new ExceptionEventArgs(Exception, FirstChance));
            if (ExceptionOccurred != null)
            {
                ExceptionInfo exInfo = new ExceptionInfo(Exception, FirstChance);
                ExceptionOccurred(exInfo);
            }

            return (int)DEBUG_STATUS.BREAK;
        }

        public int CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset)
        {
            OnThreadCreate?.Invoke(this, new CreateThreadEventArgs(Handle, DataOffset, StartOffset));
            return 0;
        }

        public int ExitThread([In] uint ExitCode)
        {
            OnThreadTerminate?.Invoke(this, new ExitThreadEventArgs(ExitCode));
            return 0;
        }

        public int CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPStr)] string ImageName,
            [In] uint CheckSum, [In] uint TimeDateStamp, [In] ulong InitialThreadHandle, [In] ulong ThreadDataOffset, [In] ulong StartOffset)
        {
            //IDebugBreakpoint2 bp;

            //_control.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out bp);
            //bp.SetOffset(BaseOffset + StartOffset);
            //bp.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
            //bp.SetCommandWide(".echo Stopping on process attach");
            OnProcessCreate?.Invoke(this, new CreateProcessEventArgs(ImageFileHandle, Handle, BaseOffset, ModuleSize, ModuleName, ImageName, CheckSum, TimeDateStamp, InitialThreadHandle, ThreadDataOffset, StartOffset));
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public int ExitProcess([In] uint ExitCode)
        {
            OnProcessTerminate?.Invoke(this, new ExitProcessEventArgs(ExitCode));
            return 0;
        }

        public int LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp)
        {
            OnModuleLoad?.Invoke(this, new LoadModuleEventArgs(ImageFileHandle, BaseOffset, ModuleSize, ModuleName, ImageName, CheckSum, TimeDateStamp));
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
                    Console.WriteLine(ex.ToString());
                }
            }

            return 0;
        }

        public int UnloadModule([In, MarshalAs(UnmanagedType.LPStr)] string ImageBaseName, [In] ulong BaseOffset)
        {
            OnModuleUnload?.Invoke(this, new UnloadModuleEventArgs(ImageBaseName, BaseOffset));
            return 0;
        }

        public int SystemError([In] uint Error, [In] uint Level)
        {
            OnSystemError?.Invoke(this, new SystemErrorEventArgs(Error, Level));
            return 0;
        }

        public int SessionStatus([In] DEBUG_SESSION Status)
        {
            OnSessionChange?.Invoke(this, new SessionStatusEventArgs(Status));
            return 0;
        }

        public int ChangeDebuggeeState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            OnStateChange?.Invoke(this, new DebuggeeStateEventArgs(Flags, Argument));
            return 0;
        }

        public int ChangeEngineState([In] DEBUG_CES Flags, [In] ulong Argument)
        {
            OnEngineChange?.Invoke(this, new EngineStateEventArgs(Flags, Argument));
            return 0;
        }

        public int ChangeSymbolState([In] DEBUG_CSS Flags, [In] ulong Argument)
        {
            OnSymbolChange?.Invoke(this, new SymbolStateEventArgs(Flags, Argument));
            return 0;
        }

        #region Custom

        public async Task<int> SetExecutionStatus(DEBUG_STATUS status)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                int hr = _control.SetExecutionStatus(status);
                tcs.SetResult(hr);
            });
            return await tcs.Task;
        }

        public void addCallback(IDebugBreakpoint bp, Action<IDebugBreakpoint> callback)
        {
            callbacks[bp] = callback;
        }

        public void removeCallback(IDebugBreakpoint bp)
        {
            callbacks.Remove(bp);
        }

        public Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint>> readCallbacks()
        {
            return callbacks;
        }

        public async Task<bool> IsModuleLoaded(string moduleName)
        {
            var tcs = new TaskCompletionSource<bool>();
            EnqueueAction(() =>
            {
                uint index;
                ulong baseAddress;
                int hr = _symbols.GetModuleByModuleName(moduleName, 0, out index, out baseAddress);
                if (hr == 0)
                {
                    tcs.SetResult(true);
                }
                else
                {
                    tcs.SetResult(false);
                }
            });
            return await tcs.Task;
        }

        public async Task<IDebugBreakpoint> SetBreakAtFunction(String moduleName, String functionName)
        {
            var tcs = new TaskCompletionSource<IDebugBreakpoint>();
            EnqueueAction(() =>
            {
                IDebugBreakpoint breakpoint;
                int hr = _control.AddBreakpoint(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out breakpoint);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to install breakpoint!");
                    tcs.SetResult(breakpoint);
                }
                hr = breakpoint.SetOffsetExpression($"{moduleName}!{functionName}");
                if (hr != 0)
                {
                    MessageBox.Show("Failed to set breakpoint offset!");
                    tcs.SetResult(breakpoint); //I know this is a duplicate, but I want it here incase we do some processing later
                }
                hr = breakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to enable breakpoint!");
                    tcs.SetResult(breakpoint);
                }
                tcs.SetResult(breakpoint);
            });
            return await tcs.Task;
        }
        public async Task<IDebugBreakpoint> SetBreakAtMemory(ulong address)
        {
            var tcs = new TaskCompletionSource<IDebugBreakpoint>();
            EnqueueAction(() =>
            {
                IDebugBreakpoint breakpoint;
                int hr = _control.AddBreakpoint(DEBUG_BREAKPOINT_TYPE.DATA, uint.MaxValue, out breakpoint);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to install breakpoint!");
                    tcs.SetResult(breakpoint);
                }
                hr = breakpoint.SetOffset(address);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to set breakpoint offset!");
                    tcs.SetResult(breakpoint);
                }
                hr = breakpoint.SetDataParameters(1, DEBUG_BREAKPOINT_ACCESS_TYPE.READ);
                hr = breakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to enable breakpoint!");
                    tcs.SetResult(breakpoint);
                }
                tcs.SetResult(breakpoint);
            });
            return await tcs.Task;
        }

        public IntPtr PtrToNative(ulong address)
        {
            if (Environment.Is64BitProcess)
            {
                IntPtr addressPtr = unchecked((IntPtr)(long)address);
                return addressPtr;
            }
            else
            {
                if (address <= uint.MaxValue)
                {
                    IntPtr addressPtr = (IntPtr)(int)address;
                    return addressPtr;
                }
                else
                {
                    uint maskedValue = (uint)(address & 0xFFFFFFFF);
                    return new IntPtr(unchecked((int)maskedValue));
                    throw new ArgumentOutOfRangeException(nameof(address), "Address is too large for a 32-it environment.");
                }
            }
            return IntPtr.Zero;
        }

        public IntPtr PtrToNativeUnsafe(ulong address)
        {
            return unchecked((IntPtr)(long)address);
        }

        public async Task<bool> SetExceptionBreakStatus(bool breakOnException)
        {
            var tcs = new TaskCompletionSource<bool>();
            EnqueueAction(() =>
            {
                DEBUG_EXCEPTION_FILTER_PARAMETERS[] parameters;
                if (breakOnException)
                {
                    parameters = new DEBUG_EXCEPTION_FILTER_PARAMETERS[1];
                    parameters[0].ExecutionOption = DEBUG_FILTER_EXEC_OPTION.BREAK;
                    parameters[0].ExceptionCode = 0xC0000005;
                }
                else
                {
                    parameters = new DEBUG_EXCEPTION_FILTER_PARAMETERS[0];
                }
                _control.SetExceptionFilterParameters(1, parameters);
            });
            return await tcs.Task;
        }

        public uint SetMemoryGuard(ulong address, uint size, bool useUnsafe = false, int processId = -1)
        {
            IntPtr processHandle = PageGuard.OpenProcess(PageGuard.PROCESS_VM_OPERATION | PageGuard.PROCESS_VM_READ | PageGuard.PROCESS_VM_WRITE, false, (uint)(processId > -1 ? this.processId : processId));
            if (processHandle == IntPtr.Zero)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to get process handle for PID: {(uint)(processId > -1 ? this.processId : processId)}{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to get process handle!", MessageBoxButtons.OK);
                return 0;
            }
            MessageBox.Show($"New Process Handle: {$"0x{processHandle.ToString("X")}"}", "Process Handle Created!", MessageBoxButtons.OK);
            uint oldProtect;
            bool result = PageGuard.VirtualProtectEx(processHandle, useUnsafe ? PtrToNativeUnsafe(address) : PtrToNative(address), size, PageGuard.PAGE_NOACCESS, out oldProtect);
            if (!result)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to install page guard at {address:X} for {size} bytes{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to install page guard!", MessageBoxButtons.OK);
                return 0;
            }
            MessageBox.Show($"Old Protect: {oldProtect}", "Page Guard Installed!", MessageBoxButtons.OK);
            bool closeHandle = PageGuard.CloseHandle(processHandle);
            if (!closeHandle)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to close process handle: 0x{processHandle.ToString("X")}{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to close handle!", MessageBoxButtons.OK);
            }
            MessageBox.Show($"Closed Process Handle: {processHandle.ToString("X")}", "Process Handle Closed!", MessageBoxButtons.OK);
            guardedPages.Add((ulong)address, oldProtect);
            return oldProtect;
        }

        public uint RemoveMemoryGuard(ulong address, uint size, bool useUnsafe = false, int processId = -1)
        {
            IntPtr processHandle = PageGuard.OpenProcess(PageGuard.PROCESS_VM_OPERATION | PageGuard.PROCESS_VM_READ | PageGuard.PROCESS_VM_WRITE, false, (uint)(processId > -1 ? this.processId : processId));
            if (processHandle == IntPtr.Zero)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to get process handle for PID: {(uint)(processId > -1 ? this.processId : processId)}{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to get process handle!", MessageBoxButtons.OK);
                return 0;
            }
            uint oldProtect;
            bool result = PageGuard.VirtualProtectEx(processHandle, useUnsafe ? PtrToNativeUnsafe(address) : PtrToNative(address), size, PageGuard.PAGE_NOACCESS, out oldProtect);
            if (!result)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to get process handle for PID: {(uint)(processId > -1 ? this.processId : processId)}{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to get process handle!", MessageBoxButtons.OK);
                return 0;
            }
            bool closeHandle = PageGuard.CloseHandle(processHandle);
            if (!closeHandle)
            {
                KeyValuePair<uint, string> error = PageGuard.GetError();
                MessageBox.Show($"Failed to close process handle: 0x{processHandle.ToString("X")}{Environment.NewLine}" +
                    $"Error: {error.Key} -> {error.Value}", "Failed to close handle!", MessageBoxButtons.OK);
            }
            guardedPages.Remove((ulong)address);
            return oldProtect;
        }

        public async Task<int> GetRegisterIndex(string register)
        {
            var tcs = new TaskCompletionSource<int>();
            EnqueueAction(() =>
            {
                uint indexReg;
                int hr = _registers.GetIndexByName(register, out indexReg);
                if (hr != 0)
                {
                    MessageBox.Show($"Failed with HRESULT {hr}", $"Failed to grab {register}");
                    tcs.SetResult(-1);
                    return;
                }
                tcs.SetResult((int)indexReg);
            });
            return await tcs.Task;
        }

        public async Task<uint> GetBrokenThread(IDebugBreakpoint bp)
        {
            var tcs = new TaskCompletionSource<uint>();
            EnqueueAction(() =>
            {
                uint threadId;
                int hr = bp.GetMatchThreadId(out threadId);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to get the broken thread id!");
                    tcs.SetResult(0);
                }
                tcs.SetResult(threadId);
            });
            return await tcs.Task;
        }

        public async Task<string> GetCurrentOpcode()
        {
            var tcs = new TaskCompletionSource<string>();
            EnqueueAction(() =>
            {
                ulong address;
                int hr = _registers.GetInstructionOffset(out address);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to read instruction location!");
                    tcs.SetResult("Error");
                }
                uint instructionSize;
                ulong endingOffset;
                StringBuilder instruction = new StringBuilder(512);
                hr = _control.Disassemble(address, 0, instruction, instruction.Capacity, out instructionSize, out endingOffset);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to disassemble the instruction!");
                    tcs.SetResult("Error");
                }
                tcs.SetResult(instruction.ToString());
            });
            return await tcs.Task;
        }

        public async Task<string> GetOpcodeAtAddress(ulong address)
        {
            var tcs = new TaskCompletionSource<string>();
            EnqueueAction(() =>
            {
                uint instructionSize;
                ulong endingOffset;
                StringBuilder instruction = new StringBuilder(512);
                int hr = _control.Disassemble(address, 0, instruction, instruction.Capacity, out instructionSize, out endingOffset);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to disassemble the instruction!");
                    tcs.SetResult("Error");
                }
                tcs.SetResult(instruction.ToString());
            });
            return await tcs.Task;
        }

        public async Task<ulong> GetCurrentInstructionAddress()
        {
            var tcs = new TaskCompletionSource<ulong>();
            EnqueueAction(() =>
            {
                ulong address;
                int hr = _registers.GetInstructionOffset(out address);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to read instruction location!");
                    tcs.SetResult(0);
                }
                tcs.SetResult(address);
            });
            return await tcs.Task;
        }

        public async Task<string> GetRegisterValueAsString(DbgEngRegister register = DbgEngRegister.EAX)
        {
            DEBUG_VALUE regValue = await GetRegisterValue(register);
            return regValue.I64.ToString("X");
        }

        public async Task<byte[]> GetMemoryFromRegisterPtr(int length, DbgEngRegister register = DbgEngRegister.EAX)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            DEBUG_VALUE pointer = await GetRegisterValue(register);
            EnqueueAction(() =>
            {
                uint bytesRead;
                byte[] buffer = new byte[length];
                int hr = _debugDataSpace.ReadVirtual(pointer.I64, buffer, (uint)length, out bytesRead);
                if (hr != 0)
                {
                    MessageBox.Show($"Failed to grab memory at address!");
                    tcs.SetResult(new byte[0]);
                }
            });
            return await tcs.Task;
        }
        public async Task<DEBUG_VALUE> GetRegisterValue(DbgEngRegister register = DbgEngRegister.EAX)
        {
            var tcs = new TaskCompletionSource<DEBUG_VALUE>();
            EnqueueAction(() =>
            {
                DEBUG_VALUE registerValue;
                _registers.GetValue((uint)register, out registerValue);
                tcs.SetResult(registerValue);
            });
            return await tcs.Task;
        }

        public async Task<List<string>> ListFuncsInModule(string moduleName)
        {
            List<string> functions = new List<string>();
            if (!(await IsModuleLoaded(moduleName)))
            {
                return functions;
            }
            uint moduleIndex;
            ulong baseAddress;

            int hr = _symbols.GetModuleByModuleName(moduleName, 0, out moduleIndex, out baseAddress);
            if (hr != 0)
            {
                MessageBox.Show("Failed to get module somehow!");
                return functions;
            }

            StringBuilder typeNameBuffer = new StringBuilder(256);
            uint typeId;
            ulong module;

            for (uint i = 0; i < uint.MaxValue; i++)
            {
                string symbol = $"{moduleName}!{i}";
                hr = _symbols.GetSymbolTypeId(symbol, out typeId, out module);
                if (hr != 0)
                {
                    break;
                }
                if (module == baseAddress)
                {
                    uint nameSize;
                    hr = _symbols.GetTypeName(baseAddress, typeId, typeNameBuffer, typeNameBuffer.Capacity, out nameSize);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to get type!");
                        continue;
                    }
                    string typeName = typeNameBuffer.ToString();
                    functions.Add(typeName);
                }
            }
            return functions;
        }

        public async Task<List<string>> ListFuncsInDebuggee()
        {
            var tcs = new TaskCompletionSource<List<string>>();
            EnqueueAction(() =>
            {
                uint moduleCount;
                int hr = _symbols.GetNumberModules(out moduleCount, out uint loadedModuleCount);
                if (hr != 0)
                {
                    tcs.SetResult(new List<string>());
                    return;
                }
                MessageBox.Show($"Modules Loaded: {moduleCount.ToString()}");
                for (uint i = 0; i < moduleCount; i++)
                {
                    StringBuilder imageBuilder = new StringBuilder(512);
                    uint imageNameSize;
                    StringBuilder moduleBuilder = new StringBuilder(512);
                    uint moduleNameSize;
                    StringBuilder loadedBuilder = new StringBuilder(512);
                    uint loadedNameSize;
                    hr = _symbols.GetModuleNames(i, 0, imageBuilder, imageBuilder.Capacity, out imageNameSize, moduleBuilder, moduleBuilder.Capacity, out moduleNameSize, loadedBuilder, loadedBuilder.Capacity, out loadedNameSize);
                    if (hr != 0)
                    {
                        MessageBox.Show($"Failed to grab module name @ {i}!");
                        continue;
                    }
                    MessageBox.Show($"imageName: {imageBuilder.ToString()}{Environment.NewLine}moduleName: {moduleBuilder.ToString()}{Environment.NewLine}loadedModuleName: {loadedBuilder.ToString()}");
                    IDebugSymbolGroup _symbolGroup;
                    hr = _symbols.GetScopeSymbolGroup(DEBUG_SCOPE_GROUP.ALL, null, out _symbolGroup);
                    if (hr != 0)
                    {
                        MessageBox.Show($"Failed to grab symbol group!");
                        continue;
                    }
                    
                    uint symbolCount;
                    hr = _symbolGroup.GetNumberSymbols(out symbolCount);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to grab number of symbols!");
                        continue;
                    }
                    MessageBox.Show($"Grabbed Symbols: {symbolCount}");
                    for (uint j = 0; j < symbolCount; j++)
                    {
                        StringBuilder symbolName = new StringBuilder(512);
                        uint symbolNameSize;
                        _symbolGroup.GetSymbolName(j, symbolName, symbolName.Capacity, out symbolNameSize);
                        MessageBox.Show(symbolName.ToString(), $"{imageBuilder.ToString()} -> {moduleBuilder.ToString()} -> {loadedBuilder.ToString()}");
                    }
                }
                tcs.SetResult(new List<string>());
            });
            return await tcs.Task;
        }
        public async Task<List<string>> GetAllModuleNames()
        {
            var tcs = new TaskCompletionSource<List<string>>();
            EnqueueAction(() =>
            {
                List<string> modules = new List<string>();

                uint loadedModules, unloadedModules;
                _symbols.GetNumberModules(out loadedModules, out unloadedModules);

                for (uint moduleIndex = 0; moduleIndex < loadedModules; moduleIndex++)
                {
                    ulong baseOfModule = 0;//what is this?
                    uint moduleNameSize, fileNameSize, imageSize;
                    StringBuilder imageNameBuffer = new StringBuilder(512);
                    StringBuilder moduleNameBuffer = new StringBuilder(512);
                    StringBuilder loadedModuleNameBuffer = new StringBuilder(512);
                    _symbols.GetModuleNames(moduleIndex, baseOfModule, imageNameBuffer, imageNameBuffer.Capacity, out imageSize, moduleNameBuffer, moduleNameBuffer.Capacity, out moduleNameSize, loadedModuleNameBuffer, loadedModuleNameBuffer.Capacity, out fileNameSize);

                    // Iterate over the range of the module
                    for (ulong address = baseOfModule; address < baseOfModule + imageSize; address++)
                    {
                        uint nameSize;
                        ulong displacement;
                        StringBuilder nameBuffer = new StringBuilder(512);
                        _symbols.GetNameByOffset(address, nameBuffer, nameBuffer.Capacity, out nameSize, out displacement);
                        if (!string.IsNullOrEmpty(nameBuffer.ToString()) && !modules.Contains(nameBuffer.ToString()))
                        {
                            modules.Add(nameBuffer.ToString());
                        }
                    }
                }
                modules = modules.Where((obj) => !obj.Contains("\\")).ToList();

                tcs.SetResult(modules);
            });
            return await tcs.Task;
        }

        public async Task<Dictionary<string, string[]>> GetAllFunctionNames()
        {
            Dictionary<string, string[]> functions = new Dictionary<string, string[]>();
            List<string> modules = await GetAllModuleNames();
            foreach (string module in modules)
            {
                uint moduleIndex;
                _symbols.GetModuleByModuleName(module, 0, out moduleIndex, out ulong moduleBase);
                if (moduleIndex == uint.MaxValue)
                {
                    continue;
                }
                DEBUG_MODULE_PARAMETERS[] moduleParams = new DEBUG_MODULE_PARAMETERS[1];
                _symbols.GetModuleParameters(1, new ulong[] { moduleBase }, 0, moduleParams);
                ulong moduleSize = moduleParams[0].Size;
                List<string> moduleFuncs = new List<string>();

                for (ulong offset = moduleBase; offset < moduleBase + moduleSize; offset++)
                {
                    ulong matchingOffset;
                    int hr = _symbols.GetOffsetByName($"{module}!*", out matchingOffset);
                    if (hr == 0 && matchingOffset == offset)
                    {
                        uint nameSize;
                        ulong displacement;
                        StringBuilder symbolName = new StringBuilder(1024);
                        _symbols.GetNameByOffset(matchingOffset, symbolName, symbolName.Capacity, out nameSize, out displacement);
                        moduleFuncs.Add(symbolName.ToString());
                    }
                }
                functions[module] = moduleFuncs.ToArray();
            }
            return functions;
        }

        public async Task<string> GetFunctionFromFrame(DEBUG_STACK_FRAME frame)
        {
            var tcs = new TaskCompletionSource<string>();
            EnqueueAction(() =>
            {
                /*
                DEBUG_MODULE_AND_ID[] symbolIds = new DEBUG_MODULE_AND_ID[1];
                ulong[] displacements = new ulong[1];
                uint entries;
                int hr = _symbols.GetSymbolEntriesByOffset(frame.InstructionOffset, 0, symbolIds, displacements, 1, out entries);
                if (hr != 0 || entries <= 0)
                {
                    MessageBox.Show("Error grabbing symbol entries of instruction offset.");
                    tcs.SetResult("");
                    return;
                }
                DEBUG_SYMBOL_ENTRY symbolEntry;
                hr = _symbols.GetSymbolEntryInformation(symbolIds[0], out symbolEntry);
                if (hr != 0)
                {
                    MessageBox.Show("Error grabbing symbol entry information!");
                    tcs.SetResult("");
                    return;
                }
                */
                StringBuilder name = new StringBuilder(512);
                uint nameSize;
                ulong displacement;
                int hr = _symbols.GetNameByOffset(frame.InstructionOffset, name, name.Capacity, out nameSize, out displacement);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to grab name from stack frame!");
                    tcs.SetResult("");
                    return;
                }
                tcs.SetResult(name.ToString());
            });
            return await tcs.Task;
        }

        public async Task<Dictionary<DbgEngRegister, ulong>> listRegisters()
        {
            var tcs = new TaskCompletionSource<Dictionary<DbgEngRegister, ulong>>();
            EnqueueAction(async () =>
            {
                Dictionary<DbgEngRegister, ulong> results = new Dictionary<DbgEngRegister, ulong>();
                foreach (DbgEngRegister register in registersAsList)
                {
                    DEBUG_VALUE value = await GetRegisterValue(register);
                    results.Add(register, value.I64);
                }
                tcs.SetResult(results);
            });
            return await tcs.Task;
        }

        public async Task<List<DEBUG_STACK_FRAME>> listStack()
        {
            var tcs = new TaskCompletionSource<List<DEBUG_STACK_FRAME>>();
            EnqueueAction(() =>
            {
                List<DEBUG_STACK_FRAME> stack = new List<DEBUG_STACK_FRAME>();
                int frameSize = 10;
                DEBUG_STACK_FRAME[] frames = new DEBUG_STACK_FRAME[frameSize];
                uint framesFilled;
                int hr = _control.GetStackTrace(0, 0, 0, frames, frameSize, out framesFilled);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to grab the current stack frame!");
                    tcs.SetResult(null);
                    return;
                }
                for (int i = 0; i < framesFilled; i++)
                {
                    stack.Add(frames[i]);
                }
                tcs.SetResult(stack);
            });
            return await tcs.Task;
        }

        public async Task<List<ThreadDetails>> listThreads()
        {
            var tcs = new TaskCompletionSource<List<ThreadDetails>>();
            /*
            EnqueueAction(() =>
            {
                List<ThreadDetails> threads = new List<ThreadDetails>();
                uint originalThreadId;
                int hr = _sysObjects.GetCurrentThreadId(out originalThreadId);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to get current thread id!");
                    tcs.SetResult(threads);
                    return;
                }
                uint numThreads;
                hr = _sysObjects.GetNumberThreads(out numThreads);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to get the number of threads!");
                    tcs.SetResult(threads);
                    return;
                }
                MessageBox.Show($"Thread Count: {numThreads}");
                uint[] threadIds = new uint[numThreads];
                uint[] engineThreadIds = new uint[numThreads];
                hr = _sysObjects.GetThreadIdsByIndex(0, numThreads, engineThreadIds, threadIds);
                if (hr != 0)
                {
                    MessageBox.Show("Failed to get thread ids!");
                    tcs.SetResult(threads);
                    return;
                }
                for (uint i = 0; i < numThreads; i++)
                {
                    uint threadId = threadIds[i];
                    uint engineThreadId = engineThreadIds[i];

                    hr = _sysObjects.SetCurrentThreadId(threadId);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to set current thread id!");
                        //Failure
                        continue;
                    }
                    ulong entryPoint;
                    string status = "Unknown";
                    DEBUG_THREAD_BASIC_INFORMATION basicInfo;
                    IntPtr threadContext = IntPtr.Zero;
                    uint contextSize = 0;
                    hr = _advanced.GetThreadContext(threadContext, contextSize);
                    if (hr != 0)
                    {
                        MessageBox.Show("Failed to get thread context");
                        continue;
                    }
                    ulong offset, handle, tebOffset;
                    uint Id, SysId;
                    _sysObjects.GetCurrentThreadDataOffset(out offset);
                    _sysObjects.GetCurrentThreadHandle(out handle);
                    _sysObjects.GetCurrentThreadId(out Id);
                    _sysObjects.GetCurrentThreadSystemId(out SysId);
                    _sysObjects.GetCurrentThreadTeb(out tebOffset);
                    //Below is where we construct the return
                    var threadInfo = new ThreadDetails();
                    threadInfo.ThreadId = Id;
                    threadInfo.SysId = SysId;
                    threadInfo.DataOffset = offset;
                    threadInfo.Active = (Id == originalThreadId);

                    threadInfo.LastError = 0;
                    threadInfo.Status = "";
                    threadInfo.EntryPoint = 0;
                    threads.Add(threadInfo);
                }
                tcs.SetResult(threads);
                hr = _sysObjects.SetCurrentThreadId(originalThreadId);
                if (hr != 0)
                {
                    //MessageBox.Show("Failed to return to original thread, this can cause unexpected behaviour!", "Failed to return to original thread!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            });
            */

            EnqueueAction(() =>
            {
                List<ThreadDetails> threads = new List<ThreadDetails>();
                uint totalThreads, currentThread;
                int hr = _sysObjects.GetNumberThreads(out totalThreads);
                uint[] threadIds = new uint[totalThreads];
                uint[] sysThreadIds = new uint[totalThreads];
                hr = _sysObjects.GetThreadIdsByIndex(0, totalThreads, threadIds, sysThreadIds);
                hr = _sysObjects.GetCurrentThreadId(out currentThread);
                for (uint i = 0; i < totalThreads; i++)
                {
                    ThreadDetails details = new ThreadDetails();
                    //hr = _control.SetInterrupt(0);
                    //_control.WaitForEvent(DEBUG_WAIT.DEFAULT, uint.MaxValue);
                    hr = _sysObjects.SetCurrentThreadId(threadIds[i]);
                    if (hr == 0)
                    {
                        uint Id, sysId;
                        ulong dataOffset;
                        _sysObjects.GetCurrentThreadId(out Id);
                        _sysObjects.GetCurrentThreadSystemId(out sysId);
                        _sysObjects.GetCurrentThreadDataOffset(out dataOffset);
                        details.DataOffset = dataOffset;
                        details.SysId = sysId;
                        details.ThreadId = Id;
                        details.Active = (Id == currentThread);
                        ulong handle;
                        _sysObjects.GetCurrentThreadHandle(out handle);
                        IntPtr context = Marshal.AllocHGlobal(4096);
                        hr = _advanced.GetThreadContext(context, 4096);
                        details.EntryPoint = 0;
                        threads.Add(details);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to switch thread!");
                    }
                }
                hr = _sysObjects.SetCurrentThreadId(currentThread);
                tcs.SetResult(threads);
            });
            return await tcs.Task;
        }

        public async Task<List<BreakpointInfo>> GetCurrentBreakpointsInfo()
        {
            var tcs = new TaskCompletionSource<List<BreakpointInfo>>();
            EnqueueAction(async () =>
            {
                List<BreakpointInfo> breakpoints = new List<BreakpointInfo>();
                uint count = 0;
                int hr = _control.GetNumberBreakpoints(out count);
                for (uint i = 0; i < count; i++)
                {
                    IDebugBreakpoint breakpoint;
                    if (_control.GetBreakpointByIndex(i, out breakpoint) == 0)
                    {
                        //Get Brekapoint
                        BreakpointInfo info = new BreakpointInfo();
                        breakpoint.GetId(out uint id);
                        info.Id = id;
                        breakpoint.GetOffset(out ulong offset);
                        info.Offset = offset;
                        int isEnabled = 0;

                        info.Enabled = isEnabled != 0;
                        StringBuilder expression = new StringBuilder(1024);
                        uint expressionSize;
                        hr = breakpoint.GetOffsetExpression(expression, expression.Capacity, out expressionSize);
                        info.Expression = $"{expression.ToString()}";
                        string opcode = await GetOpcodeAtAddress(offset);
                        info.Instruction = opcode;
                        info.Source = breakpoint;
                        breakpoints.Add(info);
                    }
                }
                tcs.SetResult(breakpoints);
            });
            return await tcs.Task;
        }

        public async Task<List<IDebugBreakpoint>> GetCurrentBreakpoints()
        {
            var tcs = new TaskCompletionSource<List<IDebugBreakpoint>>();
            EnqueueAction(() =>
            {
                List<IDebugBreakpoint> breakpoints = new List<IDebugBreakpoint>();
                uint count = 0;
                int hr = _control.GetNumberBreakpoints(out count);
                for (uint i = 0; i < count; i++)
                {
                    IDebugBreakpoint breakpoint;
                    if (_control.GetBreakpointByIndex(i, out breakpoint) == 0)
                    {
                        breakpoints.Add(breakpoint);
                    }
                }
                tcs.SetResult(breakpoints);
            });
            return await tcs.Task;
        }

        #endregion
    }

    public struct ThreadDetails
    {
        public uint ThreadId { get; set; }
        public uint SysId { get; set; }
        public ulong EntryPoint { get; set; }
        public ulong DataOffset { get; set; }
        public string Status { get; set; }
        public uint LastError { get; set; }
        public bool Active { get; set; }
    }

    public class SymbolDebugger : IDisposable
    {
        [DllImport("DbgHelp.dll", SetLastError = true)]
        private static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess);
        [DllImport("DbgHelp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SymEnumImports(IntPtr hProcess, IntPtr hMod, string Name, SymEnumImportsCallback EnumImportsCallback, IntPtr UserContext);
        [DllImport("DbgHelp.dll", SetLastError = true)]
        private static extern bool SymCleanup(IntPtr hProcess);
        private delegate bool SymEnumImportsCallback(string SymbolName, IntPtr SymbolAddress, IntPtr UserContext);
        private delegate bool SymEnumSymbolsProc(ref SymbolInfo pSymInfo, uint SymbolSize, IntPtr UserContext);
        [DllImport("DbgHelp.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SymEnumSymbols(IntPtr hProcess, ulong baseAddress, string Mask, SymEnumSymbolsProc Callback, IntPtr UserContext);

        public Process child { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct SymbolInfo
        {
            public uint SizeOfStruct;
            public uint TypeIndex;
            public ulong Reserved1;
            public ulong Reserved2;
            public uint Index;
            public uint Size;
            public ulong ModBase;
            public uint Flags;
            public ulong Value;
            public ulong Address;
            public uint Register;
            public uint Scope;
            public uint Tag;
            public uint NameLen;
            public uint MaxNameLen;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string Name;
        }

        public SymbolDebugger(Process proc)
        {
            child = proc;
            if (!SymInitialize(proc.Handle, null, false))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Exception($"SymInitialize failed with error {error}");
            }
        }
        private static bool EnumImportsCallback(string SymbolName, IntPtr SymbolAddress, IntPtr UserContext)
        {
            Console.WriteLine($"Imported Symbol: {SymbolName}, Address: 0x{SymbolAddress.ToInt64():X}");
            return true;
        }

        public Dictionary<ulong, SymbolInfo> GetSymbolsDictionary(ulong baseOffset)
        {
            Dictionary<ulong, SymbolInfo> symbols = new Dictionary<ulong, SymbolInfo>();
            SymEnumSymbolsProc callback = (ref SymbolInfo symInfo, uint symSize, IntPtr userContext) =>
            {
                symbols.Add(symInfo.Address, symInfo);
                return true;
            };
            if (!SymEnumSymbols(child.Handle, baseOffset, null, callback, IntPtr.Zero))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Exception($"SymEnumSymbols failed with error {error}");
            }
            return symbols;
        }

        private static bool SymbolCallback(ref SymbolInfo pSymInfo, uint SymbolSize, IntPtr UserContext)
        {
            MessageBox.Show($"{pSymInfo.Name}");
            return true;
        }

        public void Dispose()
        {
            if (!SymCleanup(child.Handle))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to cleanup symbols with error {error}");
            }
        }
    }

    public struct BreakpointInfo
    {
        public uint Id { get; set; }
        public ulong Offset { get; set; }
        public bool Enabled { get; set; }
        public string Expression { get; set; }
        public string Instruction { get; set; }

        public IDebugBreakpoint Source { get; set; }
    }

    #region Args
    public class OutputEventArgs : EventArgs
    {
        public DEBUG_OUTPUT Mask { get; set; }
        public string Message { get; set; }
        public OutputEventArgs(DEBUG_OUTPUT Mask, string Text)
        {
            this.Mask = Mask;
            this.Message = Text;
        }
    }
    public class BreakpointEventArgs : EventArgs
    {
        public IDebugBreakpoint Breakpoint { get; set; }
        public BreakpointEventArgs(IDebugBreakpoint Bp)
        {
            this.Breakpoint = Bp;
        }
    }
    public class ExceptionEventArgs : EventArgs
    {
        public EXCEPTION_RECORD64 Exception { get; set; }
        public uint FirstChance { get; set; }
        public ExceptionEventArgs(EXCEPTION_RECORD64 ex, uint firstChance)
        {
            this.Exception = ex;
            this.FirstChance = firstChance;
        }
    }
    public class CreateThreadEventArgs : EventArgs
    {
        public ulong Handle { get; set; }
        public ulong DataOffset { get; set; }
        public ulong StartOffset { get; set; }
        public CreateThreadEventArgs(ulong handle, ulong dataOffset, ulong startOffset)
        {
            this.Handle = handle;
            this.DataOffset = dataOffset;
            this.StartOffset = startOffset;
        }
    }
    public class ExitThreadEventArgs : EventArgs
    {
        public uint ExitCode { get; set; }
        public ExitThreadEventArgs(uint result)
        {
            this.ExitCode = result;
        }
    }
    public class LoadModuleEventArgs : EventArgs
    {
        public ulong ImageFileHandle { get; set; }
        public ulong BaseOffset { get; set; }
        public uint ModuleSize { get; set; }
        public string ModuleName { get; set; }
        public string ImageName { get; set; }
        public uint Checksum { get; set; }
        public DateTime TimeDateStamp { get; set; }
        public LoadModuleEventArgs(ulong imageHandle, ulong baseOffset, uint Size, string Name, string ImageName, uint Checksum, uint TimeDateStamp)
        {
            this.ImageFileHandle = imageHandle;
            this.BaseOffset = baseOffset;
            this.ModuleSize = Size;
            this.ModuleName = Name;
            this.ImageName = ImageName;
            this.Checksum = Checksum;
            DateTime dawn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.TimeDateStamp = dawn.AddSeconds(TimeDateStamp);
        }
    }
    public class UnloadModuleEventArgs : EventArgs
    {
        public string ImageBaseName { get; set; }
        public ulong BaseOffset { get; set; }
        public UnloadModuleEventArgs(string imageBaseName, ulong baseOffset)
        {
            this.ImageBaseName = imageBaseName;
            this.BaseOffset = baseOffset;
        }
    }
    public class CreateProcessEventArgs : EventArgs
    {
        public ulong ImageFileHandle { get; set; }
        public ulong Handle { get; set; }
        public ulong BaseOffset { get; set; }
        public uint ModuleSize { get; set; }
        public string ModuleName { get; set; }
        public string ImageName { get; set; }
        public uint CheckSum { get; set; }
        public DateTime TimeDateStamp { get; set; }
        public ulong InitialThreadHandle { get; set; }
        public ulong ThreadDataOffset { get; set; }
        public ulong StartOffset { get; set; }
        public CreateProcessEventArgs(ulong imageFileHandle, ulong handle, ulong baseOffset, uint moduleSize, string moduleName, string imageName, uint checksum, uint dateTimeStamp, ulong initialHandle, ulong threadDataOffset, ulong startOffset)
        {
            this.ImageFileHandle = imageFileHandle;
            this.Handle = handle;
            this.BaseOffset = baseOffset;
            this.ModuleSize = moduleSize;
            this.ModuleName = moduleName;
            this.ImageName = imageName;
            this.CheckSum = checksum;
            DateTime dawn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.TimeDateStamp = dawn.AddSeconds(dateTimeStamp);
            this.InitialThreadHandle = initialHandle;
            this.ThreadDataOffset = threadDataOffset;
            this.StartOffset = startOffset;
        }

    }
    public class ExitProcessEventArgs : EventArgs
    {
        public uint ExitCode { get; set; }
        public ExitProcessEventArgs(uint exit)
        {
            this.ExitCode = exit;
        }
    }
    public class SystemErrorEventArgs : EventArgs
    {
        public uint Error { get; set; }
        public uint Level { get; set; }
        public SystemErrorEventArgs(uint error, uint level)
        {
            this.Error = error;
            this.Level = level;
        }
    }
    public class SessionStatusEventArgs : EventArgs
    {
        public DEBUG_SESSION Status { get; set; }
        public SessionStatusEventArgs(DEBUG_SESSION Status)
        {
            this.Status = Status;
        }
    }
    public class DebuggeeStateEventArgs : EventArgs
    {
        public DEBUG_CDS Flags { get; set; }
        public ulong Argument { get; set; }
        public DebuggeeStateEventArgs(DEBUG_CDS flags, ulong argument)
        {
            this.Flags = flags;
            this.Argument = argument;
        }
    }
    public class EngineStateEventArgs : EventArgs
    {
        public DEBUG_CES Flags { get; set; }
        public ulong Argument { get; set; }
        public EngineStateEventArgs(DEBUG_CES flags, ulong argument)
        {
            this.Flags = flags;
            this.Argument = argument;
        }
    }
    public class SymbolStateEventArgs : EventArgs
    {
        public DEBUG_CSS Flags { get; set; }
        public ulong Argument { get; set; }
        public SymbolStateEventArgs(DEBUG_CSS flags, ulong argument)
        {
            this.Flags = flags;
            this.Argument = argument;
        }
    }

    #endregion

    #region Infos
    public class ExceptionInfo
    {
        public EXCEPTION_RECORD64 ExceptionRecord { get; }
        public uint FirstChance { get; }

        public ExceptionInfo(EXCEPTION_RECORD64 exception, uint firstChance)
        {
            ExceptionRecord = exception;
            FirstChance = firstChance;
        }

        // ... other properties or methods related to the exception
    }

    public class ModuleInfo
    {
        public ulong ImageFileHandle { get; }
        public ulong BaseOffset { get; }
        public uint ModuleSize { get; }
        public string ModuleName { get; }
        public string ImageName { get; }
        public uint CheckSum { get; }
        public uint TimeDateStamp { get; }

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

        // ... other properties or methods related to the module
    }

    #endregion

}