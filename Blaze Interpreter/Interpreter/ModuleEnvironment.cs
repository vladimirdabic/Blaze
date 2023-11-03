using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class ModuleEnvironment
    {
        public Module.Module Module;

        public ModuleEnvironment Parent;
        public List<ModuleEnvironment> Children;

        public Dictionary<string, (VariableType visibility, IValue value)> Variables;

        public ModuleEnvironment(Module.Module module, ModuleEnvironment parent)
        {
            Module = module;
            Parent = parent;
            Children = new List<ModuleEnvironment>();
            Variables = new Dictionary<string, (VariableType visibility, IValue value)>();
        }

        public ModuleEnvironment(Module.Module module): this(module, null) { }
    }
}
