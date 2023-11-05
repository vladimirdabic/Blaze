using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace VD.Blaze.Generator
{
    public class LocalEnvironment
    {
        public LocalEnvironment Parent;
        public Dictionary<string, LocalVariable> Locals = new Dictionary<string, LocalVariable>();
        public Dictionary<string, int> Args = new Dictionary<string, int>();

        public LocalEnvironment(LocalEnvironment parent)
        {
            Parent = parent;
        }

        public LocalEnvironment(): this(null) { }


        public void DefineLocal(string name, LocalVariable variable)
        {
            Locals[name] = variable;
        }

        public (LocalVariable, int) GetLocal(string name, int level = 0)
        {
            if(Locals.ContainsKey(name))
            {
                return (Locals[name], level);
            }

            if(Parent is not null) return Parent.GetLocal(name, level + 1);

            return (null, level);
        }
    }
}
