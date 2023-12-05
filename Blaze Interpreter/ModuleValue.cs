using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Interpreter.Types;

namespace Blaze_Interpreter
{
    public class ModuleValue : IValue, IValueProperties
    {
        public ModuleEnv Module;

        public ModuleValue(ModuleEnv module)
        {
            Module = module;
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return "<module>";
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
            return "module";
        }

        public IValue GetProperty(string name)
        {
            switch (name)
            {
                case "unload":
                    return new BuiltinFunctionValue("module.unload", (VM vm, List<IValue> args) =>
                    {
                        if (Module is null)
                            throw new InterpreterInternalException("Module is already unloaded");

                        ((ModuleEnv)Module.Parent).Children.Remove(Module);
                        Module.Parent = null;
                        Module = null;

                        return null;
                    });

                case "get":
                    return new BuiltinFunctionValue("module.get", (VM vm, List<IValue> args) =>
                    {
                        if (Module is null)
                            throw new InterpreterInternalException("Cannot get variable from unloaded module");

                        if (args.Count == 0 || args[0] is not StringValue)
                            throw new InterpreterInternalException("Expected variable name for module.get");

                        IVariable mod_var = Module.GetVariable(((StringValue)args[0]).Value);

                        if (mod_var is null)
                            return null;

                        return mod_var.GetValue();
                    });

                case "set":
                    return new BuiltinFunctionValue("module.set", (VM vm, List<IValue> args) =>
                    {
                        if (Module is null)
                            throw new InterpreterInternalException("Cannot set variable in unloaded module");

                        if (args.Count < 2 || args[0] is not StringValue)
                            throw new InterpreterInternalException("Expected variable name and value for module.set");

                        IVariable mod_var = Module.GetVariable(((StringValue)args[0]).Value);

                        if (mod_var is null)
                            return null;

                        IValue old_value = mod_var.GetValue();
                        mod_var.SetValue(args[1]);

                        return old_value;
                    });

                default:
                    break;
            }

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }
    }
}
