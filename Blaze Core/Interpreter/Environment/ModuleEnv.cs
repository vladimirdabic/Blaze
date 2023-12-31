﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter.Environment
{
    public class ModuleEnv : BaseEnv
    {
        public Module.Module Module;
        public List<ModuleEnv> Children;

        public List<IValue> Constants;
        public Dictionary<string, Variable> Variables;
        public List<FunctionValue> Functions;

        public ModuleEnv(Module.Module module, ModuleEnv parent = null) : base(parent)
        {
            Module = module;
            Children = new List<ModuleEnv>();

            Constants = new List<IValue>();
            Variables = new Dictionary<string, Variable>();
            Functions = new List<FunctionValue>();

            if(Module is not null)
                Load();
        }

        public ModuleEnv() : this(null, null) { }

        private void Load()
        {
            foreach (Constant constant in Module.Constants)
            {
                switch (constant.Type)
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

            foreach (var variable in Module.Variables)
            {
                // TODO: Error checking
                string name = ((Constant.String)variable.Name).Value;
                Variables[name] = new Variable(variable.Type, VM.NullInstance, this);
            }
        }

        /// <summary>
        /// Creates a new instance of a FunctionValue from a Module Function
        /// </summary>
        /// <param name="index">Index of the function in the module function list</param>
        /// <returns></returns>
        public FunctionValue GetFunction(int index)
        {
            return new FunctionValue(Module.Functions[index], this, this);
        }

        /// <summary>
        /// Creates a new instance of a ClassValue from a Module Function
        /// </summary>
        /// <param name="index">Index of the class in the module class list</param>
        /// <returns></returns>
        public ClassValue GetClass(int index)
        {
            // return new ClassValue(Module.Functions[index], this, this);
            return new ClassValue(Module.Classes[index], this, this);
        }

        /// <summary>
        /// Returns a named function
        /// </summary>
        /// <param name="name">Name of the function</param>
        public FunctionValue GetFunction(string name)
        {
            foreach (var func in Module.Functions)
            {
                if (func.Name is not null && ((StringValue)Constants[func.Name.Index]).Value == name)
                    return new FunctionValue(func, this, this);
            }

            return null;
        }

        public IVariable DefineVariable(string name, VariableType visibility, IValue value = null)
        {
            Variable variable = new Variable(visibility, value ?? VM.NullInstance, this);
            Variables[name] = variable;
            return variable;
        }

        public override IVariable DefineVariable(string name, IValue value = null)
        {
            Variable variable = new Variable(VariableType.PRIVATE, value ?? VM.NullInstance, this);
            Variables[name] = variable;
            return variable;
        }

        public override IVariable GetVariable(string name)
        {
            if (Variables.ContainsKey(name))
            {
                Variable variable = Variables[name];

                // Look into children and parent
                if (variable.Visibility == VariableType.EXTERNAL)
                {
                    foreach (var child in Children)
                    {
                        Variable value = child.GetPublicVariable(name);
                        if (value is not null) return value;
                    }

                    if (Parent is not null)
                    {
                        // Maybe have an internal visibility to allow children of parent to access internal variables
                        // Parent of ModuleEnv is always gonna be a ModuleEnv
                        Variable value = ((ModuleEnv)Parent).GetPublicVariable(name, this);
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
        public Variable GetPublicVariable(string name, ModuleEnv callerChild = null)
        {
            if (Variables.ContainsKey(name))
            {
                Variable variable = Variables[name];

                if (variable.Visibility == VariableType.PUBLIC)
                    return variable;
            }

            // Check children if not found
            foreach (var child in Children)
            {
                if (child != callerChild)
                {
                    Variable value = child.GetPublicVariable(name);
                    if (value is not null) return value;
                }
            }

            if (Parent is not null)
            {
                Variable value = ((ModuleEnv)Parent).GetPublicVariable(name, this);
                if (value is not null) return value;
            }

            // Not defined
            return null;
        }

        public override IVariable GetVariable(int index)
        {
            throw new NotImplementedException();
        }

        public void SetParent(ModuleEnv parent)
        {
            base.SetParent(parent);
            parent.Children.Add(this);
        }

        public class Variable : IVariable
        {
            public VariableType Visibility;
            public IValue Value;
            public ModuleEnv Owner;

            public Variable(VariableType visibility, IValue value, ModuleEnv owner)
            {
                Visibility = visibility;
                Value = value;
                Owner = owner;
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
