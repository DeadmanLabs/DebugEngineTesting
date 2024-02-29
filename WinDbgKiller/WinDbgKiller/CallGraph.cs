using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WinDbgKiller
{
    public class CallGraph
    {
        private Debugger dbg;
        public List<CallNode> functions { get; private set; }
        public CallNode entry { get; private set; }
        public CallGraph(Debugger dbg)
        {
            functions = new List<CallNode>();
        }
    }

    public class CallNode
    {
        private Debugger dbg;
        public ulong startAddress { get; private set; }
        public ulong endAddress { get; private set; }
        public string name { get; private set; }
        public List<CallConnection> calls { get; private set; }
        public CallNode(Debugger dbg)
        {
            calls = new List<CallConnection>();
        }
    }

    public class CallConnection
    {
        private Debugger dbg;
        public CallConnection(Debugger dbg)
        {

        }
    }
}
