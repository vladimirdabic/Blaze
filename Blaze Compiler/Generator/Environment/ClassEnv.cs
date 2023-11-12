using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace VD.Blaze.Generator.Environment
{
    public class ClassEnv : BaseEnv
    {
        public Dictionary<string, IVariable> Members = new Dictionary<string, IVariable>();

        public ClassEnv(BaseEnv parent) : base(parent)
        {
        }

        public override void DefineVariable(string name, IVariable variable = null)
        {
            Members[name] = variable;
        }

        public override (IVariable, int) GetVariable(string name, int level = 0)
        {
            if(Members.ContainsKey(name))
            {
                return (Members[name], level);
            }

            if (Parent is not null) return Parent.GetVariable(name, level + 1);
            return (null, level);
        }

        public override void PopFrame()
        {
            throw new NotImplementedException();
        }

        public override void PushFrame()
        {
            throw new NotImplementedException();
        }


        public class Variable : IVariable
        {
            public string Name;
            public Constant Constant;

            public Variable(string name, Constant name_constant)
            {
                Name = name;
                Constant = name_constant;
            }

            public string GetName()
            {
                return Name;
            }
        }
    }
}
