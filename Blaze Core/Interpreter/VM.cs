using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class VM
    {
        public BaseEnv Environment;
        public ModuleEnv Module;

        public bool Done;
        public int Line;
        public Stack<IValue> Stack;
        public static readonly NullValue NullInstance = new NullValue();

        internal Stack<ExecutionContext> _contexts;
        internal int _current;
        internal List<Instruction> _instructions;
        internal Stack<int> _exceptionStack;
        internal bool _inConstructor;

        public VM()
        {
            Stack = new Stack<IValue>();
            _contexts = new Stack<ExecutionContext>();
            _exceptionStack = new Stack<int>();
            Done = true;
        }

        /// <summary>
        /// Loads a Module and returns a ModuleEnv
        /// </summary>
        /// <param name="module">Module to load</param>
        /// <param name="parent">Optional parent environment</param>
        /// <returns></returns>
        public ModuleEnv LoadModule(Module.Module module, ModuleEnv parent = null)
        {
            ModuleEnv env = new ModuleEnv(module, parent);
            FunctionValue staticFunc = new FunctionValue(env.Module.Functions[0], env, env);

            staticFunc.Call(this, null);

            if (Done)
                Execute();
            
            return env;
        }

        /// <summary>
        /// Loads a Module and returns a ModuleEnv using a VM instance
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static ModuleEnv StaticLoadModule(Module.Module module, ModuleEnv parent = null)
        {
            VM vm = new VM();

            ModuleEnv env = new ModuleEnv(module, parent);
            FunctionValue staticFunc = new FunctionValue(env.Module.Functions[0], env, env);

            vm.RunFunction(staticFunc, null);

            return env;
        }

        /// <summary>
        /// Runs a Blaze function
        /// </summary>
        /// <param name="function">The function</param>
        /// <param name="args">List of arguments</param>
        /// <returns>Function return value</returns>
        public IValue RunFunction(FunctionValue function, List<IValue> args)
        {
            function.Call(this, args);
            Execute();

            IValue res = Stack.Pop();
            return res;
        }

        /// <summary>
        /// Executes all instructions.
        /// If you're trying to run a function use <see cref="RunFunction(ModuleEnv, FunctionValue, List{IValue})"/>.
        /// </summary>
        public void Execute()
        {
            Done = false;

            Stack.Clear();
            _contexts.Clear();
            _exceptionStack.Clear();
            _current = 0;
            Line = 0;
            _inConstructor = false;

            while (!Done)
            {
                Step();
            }
        }

        /// <summary>
        /// Executes one instruction
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Step()
        {
            Line = (int)_instructions[_current].Line;

            uint oparg = _instructions[_current].Argument;
            int opargi = (int)oparg;
            Opcode opcode = _instructions[_current].Opcode;

            _current++;

            switch (opcode)
            {
                case Opcode.NOP:
                    break;

                case Opcode.POP:
                    {
                        for (int i = 0; i < oparg; ++i)
                            Stack.Pop();
                    }
                    break;

                case Opcode.LDNULL:
                    Stack.Push(NullInstance);
                    break;

                case Opcode.LDARG:
                    // Guaranteed to be a FuncEnv
                    Stack.Push(((FuncEnv)Environment).Arguments[opargi]);
                    break;

                case Opcode.LDCONST:
                    Stack.Push(Module.Constants[opargi]);
                    break;

                case Opcode.LDLOCAL:
                    {
                        // 00 00 UPLEVEL INDEX
                        uint idx = oparg & 0xff;
                        uint uplevel = oparg >> 8;

                        IValue value = Environment.GetParent((int)uplevel).GetVariable((int)idx).GetValue();

                        Stack.Push(value);
                    }
                    break;

                case Opcode.LDVAR:
                    {
                        string name = ((StringValue)Module.Constants[opargi]).Value;
                        IVariable variable = Environment.GetVariable(name);

                        if (variable is null)
                        {
                            Throw($"Referencing an undefined variable '{name}'");
                            break;
                        }
                        else
                        {
                            Stack.Push(variable.GetValue());
                        }

                    }
                    break;

                case Opcode.LDFUNC:
                    {
                        // This must be done so each function can have its own closure, otherwise they all share the same closure
                        var func = Module.GetFunction(opargi);
                        func.Closure = Environment;
                        Stack.Push(func);
                    }
                    break;

                case Opcode.LDCLASS:
                    {
                        // This must be done so each class can have its own closure, otherwise they all share the same closure
                        var cls = Module.GetClass(opargi);
                        cls.Closure = Environment;
                        Stack.Push(cls);
                    }
                    break;

                case Opcode.LDBOOL:
                    Stack.Push(new BooleanValue(oparg == 1));
                    break;

                case Opcode.STLOCAL:
                    {
                        // 00 00 UPLEVEL INDEX
                        uint idx = oparg & 0xff;
                        uint uplevel = oparg >> 8;

                        IVariable variable = Environment.GetParent((int)uplevel).GetVariable((int)idx);

                        variable.SetValue(Stack.Pop());
                    }
                    break;

                case Opcode.STVAR:
                    {
                        string name = ((StringValue)Module.Constants[opargi]).Value;
                        IVariable variable = Environment.GetVariable(name);

                        if (variable is null)
                        {
                            Throw($"Assignment to an undefined variable '{name}'");
                            break;
                        }
                        else
                        {
                            variable.SetValue(Stack.Pop());
                        }
                    }
                    break;

                case Opcode.STARG:
                    ((FuncEnv)Environment).Arguments[opargi] = Stack.Pop();
                    break;

                case Opcode.CALL:
                    {
                        IValue value = Stack.Pop();

                        if (value is not IValueCallable)
                        {
                            Throw($"Tried calling a non callable value of type '{value.GetName()}'");
                            break;
                        }

                        List<IValue> args = new List<IValue>();

                        for (int i = 0; i < oparg; ++i)
                            args.Add(Stack.Pop());

                        try
                        {
                            ((IValueCallable)value).Call(this, args);
                        }
                        catch (InterpreterInternalException e)
                        {
                            Throw(e.Message);
                        }
                    }
                    break;

                case Opcode.NEW:
                    {
                        IValue value = Stack.Pop();

                        if (value is not IValueNew)
                        {
                            Throw($"Tried calling a new instance value of type '{value.GetName()}'");
                            break;
                        }

                        List<IValue> args = new List<IValue>();

                        var instance = new ClassInstanceValue(null);

                        for (int i = 0; i < oparg; ++i)
                            args.Add(Stack.Pop());

                        try
                        {
                            Stack.Push(instance);
                            ((IValueNew)value).New(this, instance, args);
                        }
                        catch (InterpreterInternalException e)
                        {
                            Throw(e.Message);
                        }
                    }
                    break;

                case Opcode.RET:
                    if (_inConstructor)
                    {
                        Stack.Pop(); // Pop return value from constructor, not needed
                        _inConstructor = false;
                    }

                    if (_contexts.Count == 0)
                        Done = true;
                    else
                        PopContext();
                    break;

                // BINOPS
                case Opcode.ADD:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                        {
                            Throw($"Unsupported operation '+' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        IValue res = ((IValueBinOp)left).Add(right);

                        if (res is null)
                        {
                            Throw($"Unsupported operation '+' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        Stack.Push(res);
                    }
                    break;

                case Opcode.SUB:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                        {
                            Throw($"Unsupported operation '-' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        IValue res = ((IValueBinOp)left).Subtract(right);

                        if (res is null)
                        {
                            Throw($"Unsupported operation '-' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        Stack.Push(res);
                    }
                    break;

                case Opcode.MUL:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                        {
                            Throw($"Unsupported operation '*' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        IValue res = ((IValueBinOp)left).Multiply(right);

                        if (res is null)
                        {
                            Throw($"Unsupported operation '*' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        Stack.Push(res);
                    }
                    break;

                case Opcode.DIV:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                        {
                            Throw($"Unsupported operation '/' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        IValue res = ((IValueBinOp)left).Divide(right);

                        if (res is null)
                        {
                            Throw($"Unsupported operation '/' on types '{left.GetName()}' and '{right.GetName()}'");
                            break;
                        }

                        Stack.Push(res);
                    }
                    break;

                case Opcode.THROW:
                    {
                        Throw();
                    }
                    break;

                case Opcode.CATCH:
                    {
                        _exceptionStack.Push(_current + opargi - 1);
                    }
                    break;

                case Opcode.TRY_END:
                    _exceptionStack.Pop();
                    break;

                case Opcode.JMP:
                    _current += opargi - 1;
                    break;

                case Opcode.JMPB:
                    _current -= opargi + 1;
                    break;

                case Opcode.JMPA:
                    _current = opargi;
                    break;

                case Opcode.JMPT:
                    if (Stack.Pop().AsBoolean())
                        _current += opargi - 1;

                    break;

                case Opcode.JMPF:
                    if (!Stack.Pop().AsBoolean())
                        _current += opargi - 1;

                    break;

                case Opcode.EQ:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        Stack.Push(new BooleanValue(left.Equals(right)));
                    }
                    break;

                case Opcode.AND:
                    {
                        bool right = Stack.Pop().AsBoolean();
                        bool left = Stack.Pop().AsBoolean();

                        Stack.Push(new BooleanValue(left && right));
                    }
                    break;

                case Opcode.OR:
                    {
                        bool right = Stack.Pop().AsBoolean();
                        bool left = Stack.Pop().AsBoolean();

                        Stack.Push(new BooleanValue(left || right));
                    }
                    break;

                case Opcode.NOT:
                    Stack.Push(new BooleanValue(!Stack.Pop().AsBoolean()));
                    break;

                case Opcode.LT:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                            Stack.Push(NullInstance);
                        else
                            Stack.Push(((IValueBinOp)left).LessThan(right));
                    }
                    break;

                case Opcode.LTE:
                    {
                        IValue right = Stack.Pop();
                        IValue left = Stack.Pop();

                        if (left is not IValueBinOp)
                            Stack.Push(NullInstance);
                        else
                            Stack.Push(((IValueBinOp)left).LessThanEquals(right));
                    }
                    break;

                case Opcode.DUP:
                    Stack.Push(Stack.Peek());
                    break;

                case Opcode.VARARGS:
                    // TODO
                    break;

                case Opcode.LDLIST:
                    {
                        ListValue listValue = new ListValue();

                        for (int i = 0; i < oparg; ++i)
                            listValue.Values.Add(Stack.Pop());

                        Stack.Push(listValue);
                    }
                    break;

                case Opcode.LDOBJ:
                    {
                        DictionaryValue dictionaryValue = new DictionaryValue();

                        for (int i = 0; i < oparg; ++i)
                        {
                            IValue key = Stack.Pop();
                            IValue value = Stack.Pop();

                            dictionaryValue.Entries[key] = value;
                        }

                        Stack.Push(dictionaryValue);
                    }
                    break;

                case Opcode.LDINDEX:
                    {
                        IValue obj = Stack.Pop();
                        IValue idx = Stack.Pop();

                        if (obj is not IValueIndexable)
                        {
                            Throw($"Tried indexing a non indexable type '{obj.GetName()}'");
                            break;
                        }

                        IValueIndexable indexable = (IValueIndexable)obj;

                        try
                        {
                            Stack.Push(indexable.GetAtIndex(idx));
                        }
                        catch (IndexOutOfBounds)
                        {
                            Throw($"Tried indexing out of bounds");
                            break;
                        }
                        catch (IndexNotFound)
                        {
                            Throw($"Invalid index '{idx.AsString()}'");
                            break;
                        }
                    }
                    break;

                case Opcode.STINDEX:
                    {
                        IValue obj = Stack.Pop();
                        IValue idx = Stack.Pop();
                        IValue new_value = Stack.Pop();

                        if (obj is not IValueIndexable)
                        {
                            Throw($"Tried indexing a non indexable type '{obj.GetName()}'");
                            break;
                        }

                        IValueIndexable indexable = (IValueIndexable)obj;

                        try
                        {
                            indexable.SetAtIndex(idx, new_value);
                        }
                        catch (IndexOutOfBounds)
                        {
                            Throw($"Tried indexing out of bounds");
                            break;
                        }
                        catch (IndexNotFound)
                        {
                            Throw($"Invalid index '{idx.AsString()}'");
                            break;
                        }
                    }
                    break;

                case Opcode.LDPROP:
                    {
                        IValue obj = Stack.Pop();

                        if (obj is not IValueProperties)
                        {
                            Throw($"Object of type '{obj.GetName()}' doesn't have properties");
                            break;
                        }

                        IValueProperties indexable = (IValueProperties)obj;
                        string propName = ((StringValue)Module.Constants[opargi]).Value;

                        try
                        {
                            Stack.Push(indexable.GetProperty(propName));
                        }
                        catch (PropertyNotFound)
                        {
                            Throw($"Unknown property '{propName}'");
                            break;
                        }
                    }
                    break;

                case Opcode.STPROP:
                    {
                        IValue obj = Stack.Pop();
                        IValue new_value = Stack.Pop();

                        if (obj is not IValueProperties)
                        {
                            Throw($"Object of type '{obj.GetName()}' doesn't have properties");
                            break;
                        }

                        IValueProperties indexable = (IValueProperties)obj;
                        string propName = ((StringValue)Module.Constants[opargi]).Value;

                        try
                        {
                            indexable.SetProperty(propName, new_value);
                        }
                        catch (PropertyNotFound)
                        {
                            Throw($"Unknown property '{propName}'");
                            break;
                        }
                    }
                    break;

                case Opcode.LDEVENT:
                    Stack.Push(new EventValue(this));
                    break;

                case Opcode.ITER:
                    {
                        IValue value = Stack.Pop();

                        if (value is not IValueIterable)
                        {
                            Throw($"Object of type '{value.GetName()}' is not iterable");
                            break;
                        }

                        IteratorValue iterator = ((IValueIterable)value).GetIterator();
                        Stack.Push(iterator);
                    }
                    break;

                case Opcode.ATTACH:
                    {
                        IValue ev = Stack.Pop();
                        IValue callback = Stack.Pop();

                        if(ev is not IEvent)
                        {
                            Throw($"Cannot attach a callback to an object of type '{ev.GetName()}'");
                            break;
                        }

                        if (callback is not IValueCallable)
                        {
                            Throw($"Cannot attach a non callable object ({callback.GetName()}) to an event");
                            break;
                        }

                        ((IEvent)ev).Attach((IValueCallable)callback);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Throw(string msg)
        {
            // TODO: Probably change to an exception object
            Stack.Push(new StringValue(msg));
            Throw();
        }

        private void Throw()
        {
            string module_name = Module.Module.Name;

            do
            {
                if (_exceptionStack.Count != 0)
                {
                    _current = _exceptionStack.Pop();
                    return;
                }
                else if(_contexts.Count != 0)
                {
                    // Go back up one call
                    PopContext();
                }
                else
                {
                    break;
                }
            } while (true);

            throw new VMException(Stack.Pop(), this, (module_name, Line));
        }

        internal void PushContext(List<Instruction> instructions = null)
        {
            _contexts.Push(new ExecutionContext(_current, _instructions, Environment, Module, _exceptionStack, _inConstructor));
            _current = 0;
            _instructions = instructions;
            _exceptionStack = new Stack<int>();
            _inConstructor = false;
        }

        internal void PopContext()
        {
            var ctx = _contexts.Pop();
            _current = ctx.Current;
            _instructions = ctx.Instructions;
            Environment = ctx.Environment;
            _exceptionStack = ctx.ExceptionStack;
            Module = ctx.Module;
            _inConstructor = ctx.InConstructor;
        }
    }

    public class VMException : Exception
    {
        public IValue Value;
        public VM VM;
        public (string filename, int line) Location;

        public VMException(IValue value, VM vm, (string filename, int line) location)
        {
            Value = value;
            VM = vm;
            Location = location;
        }
    }

    public class InterpreterInternalException : Exception
    {
        public InterpreterInternalException(string msg) : base(msg)
        {
        }
    }
}
