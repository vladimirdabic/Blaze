using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter.Environment
{
    public class FuncEnv : BaseEnv
    {
        public IValue[] Locals;
        public List<IValue> Arguments;

        public FuncEnv(BaseEnv parent) : base(parent) { }

        public override IVariable DefineVariable(string name, IValue value = null)
        {
            throw new NotImplementedException();
        }

        public override IVariable GetVariable(string name)
        {
            return Parent?.GetVariable(name);
        }

        public override IVariable GetVariable(int index)
        {
            return new Variable(index, this);
        }

        public class Variable : IVariable
        {
            public int Index;
            public FuncEnv Owner;

            public Variable(int index, FuncEnv owner)
            {
                Index = index;
                Owner = owner;
            }

            public IValue GetValue()
            {
                return Owner.Locals[Index];
            }

            public void SetValue(IValue value)
            {
                Owner.Locals[Index] = value;
            }
        }
    }
}
