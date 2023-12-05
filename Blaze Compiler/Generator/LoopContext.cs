using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace VD.Blaze.Generator
{
    internal struct LoopContext
    {
        public readonly int Start;
        public readonly List<Instruction> ToBeResolved;

        public LoopContext(int start)
        {
            Start = start;
            ToBeResolved = new List<Instruction>();
        }

        public readonly void Resolve(int position)
        {
            foreach (Instruction instruction in ToBeResolved)
            {
                instruction.Argument = (uint)position - instruction.Argument;
            }

            ToBeResolved.Clear();
        }
    }
}
