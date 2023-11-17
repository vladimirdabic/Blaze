using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter.Types
{
    public class ClassValue : IValue, IValueNew
    {
        public string Name = "<anonymous>";
        public List<string> InstanceMembers;
        public FunctionValue Constructor;
        public BaseEnv Closure;
        public ModuleEnv ParentModule;

        public ClassValue(Class cls, BaseEnv closure, ModuleEnv parentModule)
        {
            if(cls.Name is not null)
                Name = ((Constant.String)cls.Name).Value;
            
            InstanceMembers = new List<string>();

            foreach(var member in cls.Members) 
                InstanceMembers.Add(((Constant.String)member).Value);

            Constructor = parentModule.GetFunction(cls.Constructor.Index);
            Closure = closure;
            ParentModule = parentModule;
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return $"<class {Name}>";
        }

        public void New(Interpreter interpreter, IValue instance, List<IValue> args)
        {
            // do some base class initialization probably

            var env = new ClassEnv(Closure, instance);
            ((ClassInstanceValue)instance).Properties = env;

            foreach (var member in InstanceMembers)
                env.Members[member] = new ClassEnv.Variable(Interpreter.NullInstance);

            Constructor.Closure = env;
            Constructor.Call(interpreter, args);
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
