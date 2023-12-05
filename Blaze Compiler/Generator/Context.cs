using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace VD.Blaze.Generator
{
    internal class Context
    {
        public readonly Function Function;
        public readonly Stack<LoopContext> LoopContexts;

        public Context(Function func = null)
        {
            Function = func;
            LoopContexts = new Stack<LoopContext>();
        }
    }
}
