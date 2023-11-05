﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class FuncEnvironment
    {
        public FuncEnvironment Parent;
        public IValue[] Locals;
        public List<IValue> Arguments;
        public readonly Stack<int> ExceptionStack;

        // State variables
        private List<Instruction> _instructions;
        private int _current;
        private bool _running;

        private readonly Interpreter _interpreter;
        private readonly Stack<IValue> _stack;
        private readonly ModuleEnvironment _moduleEnv;


        public FuncEnvironment(FuncEnvironment parent, Interpreter interpreter)
        {
            Parent = parent;
            Arguments = new List<IValue>();
            ExceptionStack = new Stack<int>();
            _instructions = null;
            _interpreter = interpreter;
            _stack = interpreter._stack;
            _moduleEnv = interpreter._moduleEnv;
        }

        public IValue Evaluate(List<Instruction> instructions)
        {
            _current = 0;
            _running = true;
            _instructions = instructions;

            IValue result = Interpreter.NullInstance;

            while (_running)
            {
                int oparg = _instructions[_current].Argument;
                Opcode opcode = _instructions[_current].Id;

                while (opcode == Opcode.EXTENDED_ARG)
                {
                    _current++;
                    opcode = _instructions[_current].Id;
                    oparg = (oparg << 8) | _instructions[_current].Argument;
                }

                _current++;

                switch (opcode)
                {
                    case Opcode.NOP:
                        break;

                    case Opcode.POP:
                        {
                            for (int i = 0; i < oparg; ++i)
                                _interpreter._stack.Pop();
                        }
                        break;

                    case Opcode.LDNULL:
                        _stack.Push(Interpreter.NullInstance);
                        break;

                    case Opcode.LDARG:
                        _stack.Push(Arguments[oparg]);
                        break;

                    case Opcode.LDCONST:
                        _stack.Push(_moduleEnv.Constants[oparg]);
                        break;

                    case Opcode.LDLOCAL:
                        {
                            // 00 00 UPLEVEL INDEX
                            int idx = oparg & 0xff;
                            int uplevel = oparg >> 8;

                            // Run up the FuncEnv list with uplevel
                            var env = this;
                            for(int i = 0; i < uplevel; ++i)
                            {
                                env = env.Parent;
                            }

                            _stack.Push(env.Locals[idx]);
                        }
                        break;

                    case Opcode.LDVAR:
                        {
                            string name = ((StringValue)_moduleEnv.Constants[oparg]).Value;
                            ModuleVariable variable = _moduleEnv.GetVariable(name);

                            if (variable is null)
                            {
                                // ERROR, Should throw up the exception stack
                                Throw($"Referencing an undefined variable '{name}'");
                            }
                            else
                            {
                                _stack.Push(variable.Value);
                            }

                        }
                        break;

                    case Opcode.LDFUNC:
                        _stack.Push(_moduleEnv.Functions[oparg]);
                        break;

                    case Opcode.LDCLASS:
                        // stack.Push(_moduleEnv.Classes[oparg]);
                        break;

                    case Opcode.LDBOOL:
                        _stack.Push(new BooleanValue(oparg == 1));
                        break;

                    case Opcode.STLOCAL:
                        {
                            // 00 00 UPLEVEL INDEX
                            int idx = oparg & 0xff;
                            int uplevel = oparg >> 8;

                            // Run up the FuncEnv list with uplevel
                            var env = this;
                            for (int i = 0; i < uplevel; ++i)
                            {
                                env = env.Parent;
                            }

                            env.Locals[idx] = _stack.Pop();
                        }
                        break;

                    case Opcode.STVAR:
                        {
                            string name = ((StringValue)_moduleEnv.Constants[oparg]).Value;
                            ModuleVariable variable = _moduleEnv.GetVariable(name);

                            if (variable is null)
                            {
                                Throw($"Assignment to an undefined variable '{name}'");
                            }
                            else
                            {
                                variable.Value = _stack.Pop();
                            }
                        }
                        break;

                    case Opcode.STARG:
                        Arguments[oparg] = _stack.Pop();
                        break;

                    case Opcode.CALL:
                        {
                            IValue value = _stack.Pop();

                            if (value is not IValueCallable)
                            {
                                Throw($"Tried calling a non callable value of type '{value.GetName()}'");
                            }

                            List<IValue> args = new List<IValue>();

                            for (int i = 0; i < oparg; ++i)
                                args.Add(_stack.Pop());

                            // Push result

                            try
                            {
                                _stack.Push(((IValueCallable)value).Call(_interpreter, args));
                            }
                            catch(InterpreterInternalException e)
                            {
                                Throw();
                            }
                        }
                        break;

                    case Opcode.RET:
                        _running = false;
                        result = _stack.Pop();
                        break;

                    // BINOPS
                    case Opcode.ADD:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                            {
                                Throw($"Unsupported operation '+' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            IValue res = ((IValueBinOp)left).Add(right);

                            if (res is null)
                            {
                                Throw($"Unsupported operation '+' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            _stack.Push(res);
                        }
                        break;

                    case Opcode.SUB:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                            {
                                Throw($"Unsupported operation '-' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            IValue res = ((IValueBinOp)left).Subtract(right);

                            if (res is null)
                            {
                                Throw($"Unsupported operation '-' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            _stack.Push(res);
                        }
                        break;

                    case Opcode.MUL:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                            {
                                Throw($"Unsupported operation '*' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            IValue res = ((IValueBinOp)left).Multiply(right);

                            if (res is null)
                            {
                                Throw($"Unsupported operation '*' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            _stack.Push(res);
                        }
                        break;

                    case Opcode.DIV:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                            {
                                Throw($"Unsupported operation '/' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            IValue res = ((IValueBinOp)left).Divide(right);

                            if (res is null)
                            {
                                Throw($"Unsupported operation '/' on types '{left.GetName()}' and '{right.GetName()}'");
                            }

                            _stack.Push(res);
                        }
                        break;

                    case Opcode.THROW:
                        {
                            Throw();
                        }
                        break;

                    case Opcode.CATCH:
                        {
                            ExceptionStack.Push(_current + oparg - 1);
                        }
                        break;

                    case Opcode.TRY_END:
                        ExceptionStack.Pop();
                        break;

                    case Opcode.JMP:
                        _current += oparg - 1;
                        break;

                    case Opcode.JMPA:
                        _current = oparg;
                        break;

                    case Opcode.JMPT:
                        if (_stack.Pop().AsBoolean())
                            _current += oparg - 1;
                        
                        break;

                    case Opcode.JMPF:
                        if (!_stack.Pop().AsBoolean())
                            _current += oparg - 1;

                        break;

                    case Opcode.EQ:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            _stack.Push(new BooleanValue(left.Equals(right)));
                        } 
                        break;

                    case Opcode.NOT:
                        _stack.Push(new BooleanValue(!_stack.Pop().AsBoolean()));
                        break;

                    case Opcode.LT:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                                _stack.Push(Interpreter.NullInstance);
                            else
                                _stack.Push(((IValueBinOp)left).LessThan(right));
                        }
                        break;

                    case Opcode.LTE:
                        {
                            IValue right = _stack.Pop();
                            IValue left = _stack.Pop();

                            if (left is not IValueBinOp)
                                _stack.Push(Interpreter.NullInstance);
                            else
                                _stack.Push(((IValueBinOp)left).LessThanEquals(right));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }

        private void Throw()
        {
            if (ExceptionStack.Count != 0)
            {
                _current = ExceptionStack.Pop();
            }
            else
            {
                throw new InterpreterInternalException();
            }
        }

        private void Throw(string message)
        {
            // TODO: Change to a general exception object in the fututre
            _stack.Push(new StringValue(message));
            Throw();
        }
    }
}
