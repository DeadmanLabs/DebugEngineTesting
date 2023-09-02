using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime.Interop;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WinDbgKiller
{
    public class Debugger : IDebugOutputCallbacks, IDebugEventCallbacksWide, IDisposable
    {
        [DllImport("dbgeng.dll")]
        internal static extern int DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object Interface);

        public IDebugClient5 _client;
        public IDebugControl4 _control;
        public IDebugDataSpaces _debugDataSpace;
        public IDebugRegisters2 _registers;
        public IDebugAdvanced3 _advanced;
        public IDebugSymbols5 _symbols;
        public IDebugSystemObjects3 _sysObjects;

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
        private BlockingCollection<Action> _actionQueue = new BlockingCollection<Action>();
        private Thread _executorThread;

        public Debugger()
        {
            _executorThread = new Thread(ExecutorLoop);
            _executorThread.Start();
            callbacks = new Dictionary<IDebugBreakpoint, Action<IDebugBreakpoint>>();

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
                _symbols.SetSymbolPath("srv*");
                _symbols.Reload("");
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
            });
            return await tcs.Task;
        }

        public async Task<bool> AttachTo(int pid)
        {
            var tcs = new TaskCompletionSource<bool>();

            EnqueueAction(() =>
            {
                int hr = _client.AttachProcess(0, (uint)pid, DEBUG_ATTACH.DEFAULT);
                tcs.SetResult(hr >= 0);
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
                hr = breakpoint.SetOffsetExpression($"{moduleName}!{functionName}"); //this is busted
                if (hr != 0)
                {
                    MessageBox.Show("Failed to set breakpoint offset!");
                    tcs.SetResult(breakpoint); //I know this is a duplicate, but I want it here incase we do some processing later
                }
                tcs.SetResult(breakpoint);
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


        public struct ThreadDetails
        {
            public uint ThreadId { get; set; }
            public ulong EntryPoint { get; set; }
            public string Status { get; set; }
            public uint LastError { get; set; }
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
