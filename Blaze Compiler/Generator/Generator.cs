using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;
using VD.Blaze.Parser;
using VD.Blaze.Lexer;
using System.Runtime.Remoting.Activation;
using VD.Blaze.Generator.Environment;
using static VD.Blaze.Generator.Environment.BaseEnv;
using System.Xml.Linq;

namespace VD.Blaze.Generator
{
    public class Generator : Expression.IVisitor, Statement.IVisitor
    {
        private Module.Module _module;
        private readonly Dictionary<string, Variable> _variables;

        private Stack<Function> _functionStack;
        private Function _function;
        private string _source;
        private int _line;

        // private LocalEnvironment _localEnv;
        private BaseEnv _env;

        public Generator()
        {
            _functionStack = new Stack<Function>();
            _variables = new Dictionary<string, Variable>();
        }

        public Module.Module Generate(Statement statement, string source, bool debug = false)
        {
            _module = new Module.Module();
            _module.Name = source;
            _module.Debug = debug;
            _functionStack.Clear();
            _variables.Clear();
            _function = null;
            _env = null;
            _source = source;
            _line = 1;

            Evaluate(statement);

            return _module;
        }

        private void Evaluate(Statement stmt)
        {
            if(stmt.Line != 0)
            {
                _line = stmt.Line;
                _module.CurrentLine = _line;
            }
            
            stmt.Accept(this);
        }

        private void Evaluate(Expression expr)
        {
            expr.Accept(this);
        }

        public void VisitNumber(Expression.Number number)
        {
            Constant constant = _module.AddConstant(new Constant.Number(number.Value));
            _function.Emit(Opcode.LDCONST, constant);
        }

        public void VisitDefinitions(Statement.Definitions definitions)
        {
            foreach (var definition in definitions.Statements)
            {
                Evaluate(definition);
            }
        }

        public void VisitTopFuncDef(Statement.TopFuncDef topFuncDef)
        {
            if(_variables.ContainsKey(topFuncDef.Name))
            {
                throw new GeneratorException(_source, _line, $"Cannot define function '{topFuncDef.Name}', variable '{topFuncDef.Name}' already exists");
            }

            VariableType visibility = VariableType.PRIVATE;

            switch(topFuncDef.Visibility)
            {
                case TokenType.PRIVATE: visibility = VariableType.PRIVATE; break;
                case TokenType.PUBLIC: visibility = VariableType.PUBLIC; break;
                case TokenType.EXTERN: visibility = VariableType.EXTERNAL; break;
            }

            if(_function is not null)
                _functionStack.Push(_function);

            // Setup function and local env
            _function = _module.CreateFunction(topFuncDef.Name, topFuncDef.Args.Count, visibility);
            // _functions[topFuncDef.Name] = _function;
            _env = new FuncEnv(_env);
            
            // CreateFunction adds a variable binding for the function, it's gonna be the last one in the list after the call
            _variables[topFuncDef.Name] = _module.Variables[_module.Variables.Count - 1];

            // Setup arg indicies
            for(int i = 0;  i < topFuncDef.Args.Count; i++)
            {
                ((FuncEnv)_env).Arguments[topFuncDef.Args[i]] = i;
            }

            foreach (Statement stmt in topFuncDef.Body)
            {
                Evaluate(stmt);

                // No need to generate more instructions after this return
                if (stmt is Statement.Return) break;
            }

            if(_functionStack.Count != 0)
                _function = _functionStack.Pop();

            _env = _env.Parent;
        }

        public void VisitTopVarDef(Statement.TopVariableDef topVarDef)
        {
            VariableType visibility = VariableType.PRIVATE;

            switch (topVarDef.Visibility)
            {
                case TokenType.PRIVATE: visibility = VariableType.PRIVATE; break;
                case TokenType.PUBLIC: visibility = VariableType.PUBLIC; break;
                case TokenType.EXTERN: visibility = VariableType.EXTERNAL; break;
            }

            Variable variable = _module.DefineVariable(topVarDef.Name, visibility);
            _variables[topVarDef.Name] = variable;

            // Generate value
            var staticFunc = _module.GetStaticFunction();

            if (visibility != VariableType.EXTERNAL)
            {
                if (topVarDef.Value is not null)
                    Evaluate(topVarDef.Value);
                else
                    staticFunc.Emit(Opcode.LDNULL);
                
                staticFunc.Emit(Opcode.STVAR, variable);
            }
        }

        public void VisitString(Expression.String str)
        {
            Constant constant = _module.AddConstant(new Constant.String(str.Value));
            _function.Emit(Opcode.LDCONST, constant);
        }

        public void VisitBinaryOp(Expression.BinaryOperation binOp)
        {
            Evaluate(binOp.Left);
            Evaluate(binOp.Right);

            switch(binOp.Operator)
            {
                case TokenType.PLUS:
                    _function.Emit(Opcode.ADD);
                    break;

                case TokenType.MINUS:
                    _function.Emit(Opcode.SUB);
                    break;

                case TokenType.STAR:
                    _function.Emit(Opcode.MUL);
                    break;

                case TokenType.SLASH:
                    _function.Emit(Opcode.DIV);
                    break;

                case TokenType.DOUBLE_EQUALS:
                    _function.Emit(Opcode.EQ);
                    break;

                case TokenType.NOT_EQUALS:
                    _function.Emit(Opcode.EQ);
                    _function.Emit(Opcode.NOT);
                    break;

                case TokenType.LESS:
                    _function.Emit(Opcode.LT);
                    break;

                case TokenType.LESS_EQUALS:
                    _function.Emit(Opcode.LTE);
                    break;

                case TokenType.GREATER:
                    _function.Emit(Opcode.LTE);
                    _function.Emit(Opcode.NOT);
                    break;

                case TokenType.GREATER_EQUALS:
                    _function.Emit(Opcode.LT);
                    _function.Emit(Opcode.NOT);
                    break;

                case TokenType.AND:
                    _function.Emit(Opcode.AND);
                    break;

                case TokenType.OR:
                    _function.Emit(Opcode.OR);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void VisitExprStmt(Statement.ExprStmt exprStmt)
        {
            Evaluate(exprStmt.Expr);
            _function.Emit(Opcode.POP, 1);
        }

        public void VisitVariable(Expression.Variable variable)
        {
            string name = (string)variable.Data.Value;

            (IVariable envVar, int level) = _env.GetVariable(name);
            
            if(envVar is not null)
            {
                if (envVar is FuncEnv.Variable local)
                {
                    if (level != 0)
                        _function.Emit(Opcode.EXTENDED_ARG, (byte)level);

                    // must be an argument
                    if(local.LocalVar is null)
                        _function.Emit(Opcode.LDARG, (byte)local.Index);
                    else
                        _function.Emit(Opcode.LDLOCAL, local.LocalVar);
                    return;
                }
            }

            if (_variables.ContainsKey(name))
            {
                Variable moduleVar = _variables[name];
                _function.Emit(Opcode.LDVAR, moduleVar);
                return;
            }

            throw new GeneratorException(_source, _line, $"Undefined variable '{name}'");
        }

        public void VisitCall(Expression.Call call)
        {
            for(int i = call.Arguments.Count - 1; i >= 0; i--)
            {
                Evaluate(call.Arguments[i]);
            }

            Evaluate(call.Callee);

            _function.Emit(Opcode.CALL, (byte)call.Arguments.Count);
        }

        public void VisitReturn(Statement.Return returnStmt)
        {
            if(returnStmt.Value is null)
                _function.Emit(Opcode.LDNULL);
            else
                Evaluate(returnStmt.Value);

            _function.Emit(Opcode.RET);
        }

        public void VisitLocalVarDef(Statement.LocalVariableDef localVarDef)
        {
            LocalVariable local = _function.DeclareLocal();
            var variable = new FuncEnv.Variable(localVarDef.Name, local);
            _env.DefineVariable(localVarDef.Name, variable);

            if(localVarDef.Value is not null)
            {
                Evaluate(localVarDef.Value);
                _function.Emit(Opcode.STLOCAL, local);
            }
        }

        public void VisitAssignVar(Expression.AssignVariable assignVar)
        {
            Evaluate(assignVar.Value);
            _function.Emit(Opcode.DUP, 1);

            (IVariable envVar, int level) = _env.GetVariable(assignVar.Name);

            if(envVar is not null)
            {
                if (envVar is FuncEnv.Variable local)
                {
                    if (level != 0)
                        _function.Emit(Opcode.EXTENDED_ARG, (byte)level);

                    if (local.LocalVar is null)
                        _function.Emit(Opcode.STARG, (byte)local.Index);
                    else
                        _function.Emit(Opcode.STLOCAL, local.LocalVar);

                    return;
                }
            }

            if (_variables.ContainsKey(assignVar.Name))
            {
                Variable moduleVar = _variables[assignVar.Name];
                _function.Emit(Opcode.STVAR, moduleVar);
                return;
            }

            throw new GeneratorException(_source, _line, $"Assignment to an undefined variable '{assignVar.Name}'");
        }

        public void VisitBool(Expression.Boolean boolean)
        {
            _function.Emit(Opcode.LDBOOL, (byte)(boolean.Value ? 1 : 0));
        }

        public void VisitNull(Expression.Null nullExpr)
        {
            _function.Emit(Opcode.LDNULL);
        }

        public void VisitBlock(Statement.Block block)
        {
            _env.PushFrame();

            foreach(var stmt in block.Statements)
            {
                Evaluate(stmt);

                // No need to generate more instructions after this return
                if (stmt is Statement.Return) break;
            }

            _env.PopFrame();
        }

        public void VisitTryCatch(Statement.TryCatch tryCatch)
        {
            int catchInstIdx = _function.Instructions.Count;
            _function.Emit(Opcode.CATCH, 0);

            Evaluate(tryCatch.TryStmt);

            _function.Emit(Opcode.TRY_END);
            int jmpInstIdx = _function.Instructions.Count;
            _function.Emit(Opcode.JMP, 0);

            // Modify arg of catch instruction 
            _function.Instructions[catchInstIdx].Argument = (uint)(_function.Instructions.Count - catchInstIdx);

            _env.PushFrame();

            if(tryCatch.CatchName is not null)
            {
                LocalVariable catchVar = _function.DeclareLocal();
                _env.DefineVariable(tryCatch.CatchName, new FuncEnv.Variable(tryCatch.CatchName, catchVar));
                _function.Emit(Opcode.STLOCAL, catchVar);
            }
            else
            {
                _function.Emit(Opcode.POP, 1);
            }

            Evaluate(tryCatch.CatchStmt);
            _env.PopFrame();

            // Modify arg of jmp instruction 
            _function.Instructions[jmpInstIdx].Argument = (uint)(_function.Instructions.Count - jmpInstIdx);
        }

        public void VisitIf(Statement.IfStatement ifStmt)
        {
            Evaluate(ifStmt.Condition);

            int jmpIdx = _function.Instructions.Count;
            _function.Emit(Opcode.JMPF, 0);

            Evaluate(ifStmt.Body);

            int elseSkipIdx = _function.Instructions.Count;
            if(ifStmt.Else is not null)
                _function.Emit(Opcode.JMP, 0);

            _function.Instructions[jmpIdx].Argument = (uint)(_function.Instructions.Count - jmpIdx);

            if(ifStmt.Else is not null)
            {
                Evaluate(ifStmt.Else);
                _function.Instructions[elseSkipIdx].Argument = (uint)(_function.Instructions.Count - elseSkipIdx);
            }
        }

        public void VisitFunctionValue(Expression.FuncValue funcValue)
        {
            if (_function is not null)
                _functionStack.Push(_function);

            // Setup function and local env
            Function function = _module.CreateAnonymousFunction(funcValue.Args.Count);
            _function = function;
            _env = new FuncEnv(_env);
            int funcval_line = _line;

            // Setup arg indicies
            for (int i = 0; i < funcValue.Args.Count; i++)
            {
                ((FuncEnv)_env).Arguments[funcValue.Args[i]] = i;
            }

            foreach (Statement stmt in funcValue.Body)
            {
                Evaluate(stmt);

                // No need to generate more instructions after this return
                if (stmt is Statement.Return) break;
            }

            if (_functionStack.Count != 0)
                _function = _functionStack.Pop();

            _env = _env.Parent;

            _line = funcval_line;
            _module.CurrentLine = funcval_line;
            _function.Emit(Opcode.LDFUNC, function);
        }

        public void VisitThrow(Statement.Throw throwStmt)
        {
            if (throwStmt.Value is null)
                _function.Emit(Opcode.LDNULL);
            else
                Evaluate(throwStmt.Value);

            _function.Emit(Opcode.THROW);
        }

        public void VisitStaticStmt(Statement.StaticStmt staticStmt)
        {
            if (_function is not null)
                _functionStack.Push(_function);

            // Static function
            _function = _module.Functions[0];
            _env = new FuncEnv(_env);

            Evaluate(staticStmt.Stmt);

            if (_functionStack.Count != 0)
                _function = _functionStack.Pop();

            _env = _env.Parent;
        }

        public void VisitEventValue(Expression.EventValue eventValue)
        {
            _function.Emit(Opcode.LDEVENT);
        }

        public void VisitListValue(Expression.ListValue listValue)
        {
            // Push values backwards
            for(int i = listValue.Values.Count - 1;  i >= 0; i--)
            {
                Evaluate(listValue.Values[i]);
            }

            _function.Emit(Opcode.LDLIST, (byte)listValue.Values.Count);
        }

        public void VisitTopEventDef(Statement.TopEventDef topEventDef)
        {
            throw new NotImplementedException();
        }

        public void VisitGetIndex(Expression.GetIndex getIndex)
        {
            Evaluate(getIndex.Index);
            Evaluate(getIndex.Left);

            _function.Emit(Opcode.LDINDEX);
        }

        public void VisitSetIndex(Expression.SetIndex setIndex)
        {
            Evaluate(setIndex.Value);
            _function.Emit(Opcode.DUP);
            Evaluate(setIndex.Index);
            Evaluate(setIndex.Left);

            _function.Emit(Opcode.STINDEX);
        }

        public void VisitGetProperty(Expression.GetProperty getProperty)
        {
            Constant propName = _module.AddConstant(new Constant.String(getProperty.Property));
            Evaluate(getProperty.Left);

            _function.Emit(Opcode.LDPROP, propName);
        }

        public void VisitSetProperty(Expression.SetProperty setProperty)
        {
            Constant propName = _module.AddConstant(new Constant.String(setProperty.Property));

            Evaluate(setProperty.Value);
            Evaluate(setProperty.Left);

            _function.Emit(Opcode.STPROP, propName);
        }

        public void VisitWhile(Statement.WhileStatement whileStmt)
        {
            int conditionIdx = _function.Instructions.Count;
            Evaluate(whileStmt.Condition);

            int jmpIdx = _function.Instructions.Count;
            _function.Emit(Opcode.JMPF, 0);

            Evaluate(whileStmt.Body);
            _function.Emit(Opcode.JMPB, _function.Instructions.Count - conditionIdx);

            _function.Instructions[jmpIdx].Argument = (uint)(_function.Instructions.Count - jmpIdx);
        }

        public void VisitFor(Statement.ForStatement forStmt)
        {
            _env.PushFrame();
            Evaluate(forStmt.Initializer);

            int conditionIdx = _function.Instructions.Count;
            Evaluate(forStmt.Condition);

            int jmpIdx = _function.Instructions.Count;
            _function.Emit(Opcode.JMPF, 0);

            Evaluate(forStmt.Body);
            Evaluate(forStmt.Increment);
            _function.Emit(Opcode.JMPB, _function.Instructions.Count - conditionIdx);

            _function.Instructions[jmpIdx].Argument = (uint)(_function.Instructions.Count - jmpIdx);
            _env.PopFrame();
        }

        public void VisitDictValue(Expression.DictValue dictValue)
        {
            foreach(var pair in dictValue.Pairs)
            {
                Evaluate(pair.value);
                Evaluate(pair.key);
            }

            _function.Emit(Opcode.LDOBJ, (byte)dictValue.Pairs.Count);
        }

        public void VisitIterator(Expression.Iterator iteratorValue)
        {
            Evaluate(iteratorValue.Value);
            _function.Emit(Opcode.ITER);
        }
    }

    public class GeneratorException : Exception
    {
        public new string Source;
        public int Line;

        public GeneratorException(string source, int line, string message) : base(message)
        {
            Source = source;
            Line = line;
        }
    }
}
