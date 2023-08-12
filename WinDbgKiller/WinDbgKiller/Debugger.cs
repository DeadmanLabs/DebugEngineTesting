using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime.Interop;
using System.Text;

namespace WinDbgKiller
{
    public class Debugger : IDebugOutputCallbacks, IDebugEventCallbacksWide, IDisposable
    {
        [DllImport("dbgeng.dll")]
        internal static extern int DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object Interface);

        IDebugClient5 _client;
        IDebugControl4 _control;
        IDebugDataSpaces _debugDataSpace;
        IDebugRegisters2 _registers;
        IDebugAdvanced3 _advanced;
        IDebugSymbols5 _symbols;
        IDebugSystemObjects3 _sysObjects;

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

        public Action<string> _outputHandler;

        public Debugger(Action<string> outputHandler = null)
        {
            Guid guid = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
            object obj = null;

            int hr = DebugCreate(ref guid, out obj);
            _outputHandler = outputHandler;

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
        }

        public int ReadMemory(ulong address, uint size, out byte[] buf)
        {
            buf = new byte[size];

            _debugDataSpace.ReadVirtual(address, buf, (uint)buf.Length, out uint readBytes);
            return (int)readBytes;
        }

        /// <summary>
        /// Custom Kernel Attach
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns>bool</returns>
        public bool KernelAttachTo(string pipe)
        {
            int hr = _client.AttachKernel(DEBUG_ATTACH.KERNEL_CONNECTION, pipe);
            return hr >= 0;
        }

        /// <summary>
        /// Custom Kernel Attach
        /// </summary>
        /// <param name="attachParams"></param>
        /// <param name="connectionOptions"></param>
        /// <returns></returns>
        public bool KernelAttachTo(DEBUG_ATTACH attachParams, string connectionOptions)
        {
            int hr = _client.AttachKernel(attachParams, connectionOptions);
            return hr >= 0;
        }

        public bool AttachTo(int pid)
        {
            int hr = _client.AttachProcess(0, (uint)pid, DEBUG_ATTACH.DEFAULT);
            return hr >= 0;
        }

        public int GetExecutionStatus(out DEBUG_STATUS status)
        {
            int ret = int.MinValue;
            status = DEBUG_STATUS.TIMEOUT;
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
            return ret;
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

            if (_outputHandler != null)
            {
                _outputHandler($"{Text}{Environment.NewLine}");
            }
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
            if (ExceptionOccurred != null)
            {
                ExceptionInfo exInfo = new ExceptionInfo(Exception, FirstChance);
                ExceptionOccurred(exInfo);
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
            //IDebugBreakpoint2 bp;

            //_control.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out bp);
            //bp.SetOffset(BaseOffset + StartOffset);
            //bp.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
            //bp.SetCommandWide(".echo Stopping on process attach");

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
                    Console.WriteLine(ex.ToString());
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

        public int ChangeEngineState([In] DEBUG_CES Flags, [In] ulong Argument)
        {
            return 0;
        }

        public int ChangeSymbolState([In] DEBUG_CSS Flags, [In] ulong Argument)
        {
            return 0;
        }

        #region Custom

        public struct ThreadDetails
        {
            public uint ThreadId { get; set; }
            public ulong EntryPoint { get; set; }
            public string Status { get; set; }
            public uint LastError { get; set; }
        }

        public struct ModuleDetails
        {

        }

        public object[] GetThreads()
        {
            uint originalThreadId;
            int hr = _sysObjects.GetCurrentThreadId(out originalThreadId);
            if (hr != 0)
            {
                return new object[0];
            }
            List<object> threads = new List<object>();
            uint numThreads;
            hr = _sysObjects.GetNumberThreads(out numThreads);
            if (hr != 0)
            {
                return threads.ToArray();
            }
            uint[] threadIds = new uint[numThreads];
            uint[] engineThreadIds = new uint[numThreads];
            hr = _sysObjects.GetThreadIdsByIndex(0, numThreads, engineThreadIds, threadIds);
            if (hr != 0)
            {
                return threads.ToArray();
            }
            for (uint i = 0; i < numThreads; i++)
            {
                uint threadId = threadIds[i];
                uint engineThreadId = engineThreadIds[i];

                hr = _sysObjects.SetCurrentThreadId(engineThreadId);
                if (hr != 0)
                {
                    //Failure
                    continue;
                }
                ulong entryPoint;
                string status = "Unknown";
                DEBUG_THREAD_BASIC_INFORMATION basicInfo;
                
                //Below is where we construct the return
                var threadInfo = new
                {
                    ThreadId = threadId,
                    EngineThreadId = engineThreadId,
                };
                threads.Add(threadInfo);
            }
            return threads.ToArray();
        }

        public Dictionary<string, byte[]> GetRegisters()
        {
            Dictionary<string, byte[]> registers = new Dictionary<string, byte[]>();
            uint numRegisters;
            int hr = _registers.GetNumberRegisters(out numRegisters);
            if (hr != 0)
            {
                //Failed
                return null;
            }
            StringBuilder nameBuf = new StringBuilder(256);
            for (uint i = 0; i < numRegisters; i++)
            {
                uint nameSize;
                DEBUG_REGISTER_DESCRIPTION description;
                hr = _registers.GetDescription(i, nameBuf, nameBuf.Capacity, out nameSize, out description);
                if (hr != 0)
                {
                    //Failed
                    continue;
                }
                string registerName = nameBuf.ToString();
                DEBUG_VALUE registerValue;
                hr = _registers.GetValue(i, out registerValue);
                if (hr != 0)
                {
                    //Failed
                    continue;
                }
                byte[] valueBytes = ValueToByteReader(registerValue, description);
                registers[registerName] = valueBytes;
            }
            return registers;
        }

        public object[] GetCallstack()
        {
            List<object> callstack = new List<object>();

            return callstack.ToArray();
        }

        public object[] GetBreakpoints()
        {
            List<object> breakpoints = new List<object>();

            return breakpoints.ToArray();
        }

        private byte[] ValueToByteReader(DEBUG_VALUE value, DEBUG_REGISTER_DESCRIPTION description)
        {
            byte[] bytes = null;

            switch (value.Type)
            {
                case DEBUG_VALUE_TYPE.INT32:
                    bytes = BitConverter.GetBytes(value.I32);
                    break;
                case DEBUG_VALUE_TYPE.INT64:
                    bytes = BitConverter.GetBytes(value.I64);
                    break;
                case DEBUG_VALUE_TYPE.INT8:
                    bytes = new byte[] { value.I8 };
                    break;
                case DEBUG_VALUE_TYPE.INT16:
                    bytes = BitConverter.GetBytes(value.I16);
                    break;
                case DEBUG_VALUE_TYPE.INVALID:
                    
                    break;
                case DEBUG_VALUE_TYPE.TYPES:
                    
                    break;
                case DEBUG_VALUE_TYPE.VECTOR64:
                    
                    break;
                case DEBUG_VALUE_TYPE.VECTOR128:

                    break;
                case DEBUG_VALUE_TYPE.FLOAT32:
                    bytes = BitConverter.GetBytes(value.F32);
                    break;
                case DEBUG_VALUE_TYPE.FLOAT64:
                    bytes = BitConverter.GetBytes(value.F64);
                    break;
                case DEBUG_VALUE_TYPE.FLOAT128:
                    
                    break;
                case DEBUG_VALUE_TYPE.FLOAT80:

                    break;
                case DEBUG_VALUE_TYPE.FLOAT82:

                    break;
            }
            return bytes;
        }

        #endregion
    }
}
