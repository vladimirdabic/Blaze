using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter.Environment
{
    public class ClassEnv : BaseEnv
    {
        public Dictionary<string, IVariable> Members;

        public ClassEnv(BaseEnv parent) : base(parent)
        {
            Members = new Dictionary<string, IVariable>();
        }

        public override IVariable DefineVariable(string name, IValue value = null)
        {
            throw new NotImplementedException();
        }

        public override IVariable GetVariable(string name)
        {
            if (Members.ContainsKey(name))
                return Members[name];

            if(Parent is not null)
                return Parent.GetVariable(name);

            return null;
        }

        public override IVariable GetVariable(int index)
        {
            throw new NotImplementedException();
        }

        public class Variable : IVariable
        {
            public IValue Value;

            public Variable(IValue value)
            {
                Value = value;
            }

            public IValue GetValue()
            {
                return Value;
            }

            public void SetValue(IValue value)
            {
                Value = value;
            }
        }
    }
}
