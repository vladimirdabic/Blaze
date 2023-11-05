using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class ModuleEnvironment
    {
        public Module.Module Module;

        public ModuleEnvironment Parent;
        public List<ModuleEnvironment> Children;

        public List<IValue> Constants;
        public Dictionary<string, ModuleVariable> Variables;
        public List<FunctionValue> Functions;

        public ModuleEnvironment(Module.Module module, ModuleEnvironment parent)
        {
            Module = module;
            Parent = parent;
            Children = new List<ModuleEnvironment>();

            Variables = new Dictionary<string, ModuleVariable>();
            Functions = new List<FunctionValue>();
            Constants = new List<IValue>();

            Load();
        }

        public ModuleEnvironment(Module.Module module): this(module, null) { }


        private void Load()
        {
            foreach(Constant constant in Module.Constants)
            {
                switch(constant.Type)
                {
                    case ConstantType.NUMBER:
                        Constants.Add(new NumberValue(((Constant.Number)constant).Value));
                        break;

                    case ConstantType.STRING:
                        Constants.Add(new StringValue(((Constant.String)constant).Value));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            foreach(Variable variable in Module.Variables)
            {
                // TODO: Error checking
                string name = ((Constant.String)variable.Name).Value;
                Variables[name] = new ModuleVariable(variable.Type, null);
            }

            foreach(Function function in Module.Functions)
            {
                Functions.Add(new FunctionValue(function, null));
            }
        }


        /// <summary>
        /// Gets a variable from this module (returns public and private variables)
        /// If not found, look through children and parents (returns a public variable)
        /// </summary>
        /// <param name="name">Variable name</param>
        public ModuleVariable GetVariable(string name)
        {
            if (Variables.ContainsKey(name))
            {
                ModuleVariable variable = Variables[name];

                // Look into children and parent
                if(variable.Visibility == VariableType.EXTERNAL)
                {
                    foreach(var child in Children)
                    {
                        ModuleVariable value = child.GetPublicVariable(name);
                        if (value is not null) return value;
                    }

                    if(Parent is not null)
                    {
                        // Maybe have an internal visibility to allow children of parent to access internal variables
                        ModuleVariable value = Parent.GetPublicVariable(name, this);
                        if (value is not null) return value;
                    }

                    // Not found
                    return null;
                }

                // Public and private so return
                return variable;
            }

            // Not defined
            return null;
        }

        /// <summary>
        /// Gets a public variable from this module
        /// </summary>
        /// <param name="name">Variable name</param>
        public ModuleVariable GetPublicVariable(string name, ModuleEnvironment callerChild = null)
        {
            if (Variables.ContainsKey(name))
            {
                ModuleVariable variable = Variables[name];

                if (variable.Visibility != VariableType.PUBLIC)
                {
                    // Check children for same variable
                    foreach (var child in Children)
                    {
                        if (child != callerChild)
                        {
                            ModuleVariable value = child.GetPublicVariable(name);
                            if (value is not null) return value;
                        }
                    }

                    // Not public so it can't be accessed
                    return null;
                }

                return variable;
            }
            else
            {
                // Check children if not found
                foreach (var child in Children)
                {
                    if (child != callerChild)
                    {
                        ModuleVariable value = child.GetPublicVariable(name);
                        if (value is not null) return value;
                    }
                }
            }

            // Not defined
            return null;
        }

        /// <summary>
        /// Returns a named function
        /// </summary>
        /// <param name="name">Name of the function</param>
        public FunctionValue GetFunction(string name)
        {
            foreach(var func  in Functions)
            {
                if(func.Name == name) return func;
            }

            return null;
        }
    }

    public class ModuleVariable
    {
        public VariableType Visibility;
        public IValue Value;

        public ModuleVariable(VariableType visibility, IValue value)
        {
            Visibility = visibility;
            Value = value;
        }
    }
}
