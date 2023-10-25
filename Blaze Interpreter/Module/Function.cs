using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Function
    {
        public Constant Name;
        public int NumOfArgs;
        public int NumOfLocals { get; private set; }
        private List<(Instruction inst, byte arg)> _instructions;

        public Function(Constant name, int numOfArgs)
        {
            Name = name;
            NumOfArgs = numOfArgs;
            _instructions = new List<(Instruction inst, byte arg)>();
        }

        public void Emit(Instruction instruction, byte argument)
        {
            _instructions.Add((instruction, argument));
        }

        public int DeclareLocal()
        {
            return NumOfLocals++;
        }

        public void ToBinary(BinaryWriter writer)
        {

        }

        public void FroMBinary(BinaryReader reader)
        {

        }
    }

    public enum Instruction
    {

    }
}
