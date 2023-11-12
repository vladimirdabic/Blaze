using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Generator.Environment
{
    public class FuncEnv : BaseEnv
    {
        public List<Dictionary<string, IVariable>> Locals = new List<Dictionary<string, IVariable>>();
        public Dictionary<string, int> Arguments = new Dictionary<string, int>();

        public FuncEnv(BaseEnv parent) : base(parent)
        {
            Locals.Add(new Dictionary<string, IVariable>());
        }

        public override void DefineVariable(string name, IVariable variable = null)
        {
            Locals[Locals.Count - 1][name] = variable;
        }

        public override (IVariable, int) GetVariable(string name, int level = 0)
        {
            var local = GetFromStack(name);

            if (local is not null)
            {
                return (local, level);
            }

            if (Arguments.ContainsKey(name))
                return (new Variable(name, null) { Index = Arguments[name] }, level);

            if (Parent is not null) return Parent.GetVariable(name, level + 1);

            return (null, level);
        }

        public override void PushFrame()
        {
            Locals.Add(new Dictionary<string, IVariable>());
        }

        public override void PopFrame()
        {
            Locals.RemoveAt(Locals.Count - 1);
        }

        private IVariable GetFromStack(string name)
        {
            for (int i = Locals.Count - 1; i >= 0; i--)
            {
                var locals = Locals[i];
                if (locals.ContainsKey(name))
                    return locals[name];
            }

            return null;
        }


        public class Variable : IVariable
        {
            public string Name { get; set; }
            public LocalVariable LocalVar;
            public int Index;

            public Variable(string name, LocalVariable localVar)
            {
                Name = name;
                LocalVar = localVar;
            }

            public string GetName()
            {
                return Name;
            }
        }
    }
}
