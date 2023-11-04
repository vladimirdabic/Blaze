using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class Interpreter
    {
        // We'll see if this is how this will look like
        internal ModuleEnvironment _moduleEnv;
        internal FuncEnvironment _env;

        private Stack<IValue> _stack;
        private int _current = 0;
        private bool _running = true;

        private static readonly NullValue _nullValue = new NullValue();

        public Interpreter()
        {
            _stack = new Stack<IValue>();
        }

        public ModuleEnvironment LoadModule(Module.Module module)
        {
            ModuleEnvironment env = new ModuleEnvironment(module);

            // TODO: Error checking
            FunctionValue staticFunc = new FunctionValue(env.Module.Functions[0], null);

            RunFunction(env, staticFunc, null);
            
            return env;
        }


        public IValue RunFunction(ModuleEnvironment env, FunctionValue function, List<IValue> args)
        {
            _moduleEnv = env;
            _env = null;
            return function.Call(this, args);
        }

        public IValue Evaluate(List<Instruction> instructions)
        {
            _current = 0;
            _running = true;

            // TODO: Change to instance of Blaze null value
            IValue result = _nullValue;
            _stack.Clear();
            
            while(_running)
            {
                int oparg = instructions[_current].Argument;
                Opcode opcode = instructions[_current].Id;

                while(opcode == Opcode.EXTENDED_ARG)
                {
                    _current++;
                    opcode = instructions[_current].Id;
                    oparg = (oparg << 8) | instructions[_current].Argument;
                }

                _current++;

                switch (opcode)
                {
                    case Opcode.NOP:
                        break;

                    case Opcode.POP:
                        _stack.Pop();
                        break;

                    case Opcode.LDNULL:
                        _stack.Push(_nullValue);
                        break;

                    case Opcode.LDARG:
                        _stack.Push(_env.Arguments[oparg]);
                        break;

                    case Opcode.LDCONST:
                        _stack.Push(_moduleEnv.Constants[oparg]);
                        break;

                    case Opcode.LDLOCAL:
                    {
                        // 00 00 UPLEVEL INDEX
                        int idx = oparg & 0xff;
                        int uplevel = oparg & (0xff << 8);

                        // TODO: Run up the FuncEnv list with uplevel

                        _stack.Push(_env.Locals[idx]);
                    } break;

                    case Opcode.LDVAR:
                    { 
                        string name = ((StringValue)_moduleEnv.Constants[oparg]).Value;
                        ModuleVariable variable = _moduleEnv.GetVariable(name);

                        if(variable is null)
                        {
                            // ERROR, Should throw up the exception stack
                        } 
                        else
                        {
                            _stack.Push(variable.Value);
                        }

                    } break;

                    case Opcode.LDFUNC:
                        _stack.Push(_moduleEnv.Functions[oparg]);
                        break;

                    case Opcode.LDCLASS:
                        // stack.Push(_moduleEnv.Classes[oparg]);
                        break;

                    case Opcode.STLOCAL:
                    {
                        // 00 00 UPLEVEL INDEX
                        int idx = oparg & 0xff;
                        int uplevel = oparg & (0xff << 8);

                        // TODO: Run up the FuncEnv list with uplevel

                        _env.Locals[idx] = _stack.Pop();
                    } break;

                    case Opcode.STVAR:
                    { 
                        string name = ((StringValue)_moduleEnv.Constants[oparg]).Value;
                        ModuleVariable variable = _moduleEnv.GetVariable(name);

                        if(variable is null)
                        {
                            // ERROR, Should throw up the exception stack
                        } 
                        else
                        {
                            variable.Value = _stack.Pop();
                        }
                    } break;

                    case Opcode.CALL:
                    {
                        IValue value = _stack.Pop();

                        if(value is not IValueCallable)
                        {
                            // throw error
                        }

                        List<IValue> args = new List<IValue>();

                        for(int i = 0; i < oparg; ++i)
                            args.Add(_stack.Pop());

                        // Push result
                        _stack.Push(((IValueCallable)value).Call(this, args));
                    } break;

                    case Opcode.RET:
                        _running = false;
                        result = _stack.Pop();
                        break;

                    // BINOPS
                    case Opcode.ADD:
                    {
                        IValue left = _stack.Pop();
                        IValue right = _stack.Pop();

                        if(left is not IValueBinOp)
                        {
                            // throw error (unsupported operation on type)
                        }

                        IValue res = ((IValueBinOp)left).Add(right);

                        if(res is null)
                        {
                            // throw error (unsupported operation on type)
                        }

                        _stack.Push(res);
                    } break;

                    case Opcode.SUB:
                    {
                        IValue left = _stack.Pop();
                        IValue right = _stack.Pop();

                        if(left is not IValueBinOp)
                        {
                            // throw error (unsupported operation on type)
                        }

                        IValue res = ((IValueBinOp)left).Subtract(right);

                        if(res is null)
                        {
                            // throw error (unsupported operation on type)
                        }

                        _stack.Push(res);
                    } break;

                    case Opcode.MUL:
                    {
                        IValue left = _stack.Pop();
                        IValue right = _stack.Pop();

                        if(left is not IValueBinOp)
                        {
                            // throw error (unsupported operation on type)
                        }

                        IValue res = ((IValueBinOp)left).Multiply(right);

                        if(res is null)
                        {
                            // throw error (unsupported operation on type)
                        }

                        _stack.Push(res);
                    } break;

                    case Opcode.DIV:
                    {
                        IValue left = _stack.Pop();
                        IValue right = _stack.Pop();

                        if(left is not IValueBinOp)
                        {
                            // throw error (unsupported operation on type)
                        }

                        IValue res = ((IValueBinOp)left).Divide(right);

                        if(res is null)
                        {
                            // throw error (unsupported operation on type)
                        }

                        _stack.Push(res);
                    } break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }

    }
}
