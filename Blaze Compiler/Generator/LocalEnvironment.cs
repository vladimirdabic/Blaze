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
        public List<Dictionary<string, LocalVariable>> Locals = new List<Dictionary<string, LocalVariable>>();
        public Dictionary<string, int> Args = new Dictionary<string, int>();

        public LocalEnvironment(LocalEnvironment parent)
        {
            Parent = parent;
            Locals.Add(new Dictionary<string, LocalVariable>());
        }

        public LocalEnvironment(): this(null) { }


        public void DefineLocal(string name, LocalVariable variable)
        {
            Locals[Locals.Count - 1][name] = variable;
        }

        public (LocalVariable, int) GetLocal(string name, int level = 0)
        {
            var local = GetFromStack(name);

            if (local is not null)
            {
                return (local, level);
            }

            if(Parent is not null) return Parent.GetLocal(name, level + 1);

            return (null, level);
        }

        private LocalVariable GetFromStack(string name)
        {
            for(int i = Locals.Count - 1; i >= 0; i--)
            {
                var locals = Locals[i];
                if(locals.ContainsKey(name))
                    return locals[name];
            }

            return null;
        }

        public void PushFrame()
        {
            Locals.Add(new Dictionary<string, LocalVariable>());
        }

        public void PopFrame()
        {
            Locals.RemoveAt(Locals.Count - 1);
        }
    }
}
