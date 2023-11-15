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

        public void Visit(Expression.Number number)
        {
            Constant constant = _module.AddConstant(new Constant.Number(number.Value));
            _function.Emit(Opcode.LDCONST, constant);
        }

        public void Visit(Statement.Definitions definitions)
        {
            foreach (var definition in definitions.Statements)
            {
                Evaluate(definition);
            }
        }

        public void Visit(Statement.TopFuncDef topFuncDef)
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

        public void Visit(Statement.TopVariableDef topVarDef)
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
                {
                    EnterFunction(staticFunc);
                    Evaluate(topVarDef.Value);
                    LeaveFunction();
                }
                else
                    staticFunc.Emit(Opcode.LDNULL);
                
                staticFunc.Emit(Opcode.STVAR, variable);
            }
        }

        public void Visit(Expression.String str)
        {
            Constant constant = _module.AddConstant(new Constant.String(str.Value));
            _function.Emit(Opcode.LDCONST, constant);
        }

        public void Visit(Expression.BinaryOperation binOp)
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

        public void Visit(Statement.ExprStmt exprStmt)
        {
            Evaluate(exprStmt.Expr);
            _function.Emit(Opcode.POP, 1);
        }

        public void Visit(Expression.Variable variable)
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

                if (envVar is ClassEnv.Variable classVar)
                {
                    // if (level != 0)
                    //   _function.Emit(Opcode.EXTENDED_ARG, (byte)level);

                    _function.Emit(Opcode.LDVAR, classVar.Constant.Index);
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

        public void Visit(Expression.Call call)
        {
            for(int i = call.Arguments.Count - 1; i >= 0; i--)
            {
                Evaluate(call.Arguments[i]);
            }

            Evaluate(call.Callee);

            _function.Emit(Opcode.CALL, (byte)call.Arguments.Count);
        }

        public void Visit(Statement.Return returnStmt)
        {
            if(returnStmt.Value is null)
                _function.Emit(Opcode.LDNULL);
            else
                Evaluate(returnStmt.Value);

            _function.Emit(Opcode.RET);
        }

        public void Visit(Statement.LocalVariableDef localVarDef)
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

        public void Visit(Expression.AssignVariable assignVar)
        {
            Evaluate(assignVar.Value);
            _function.Emit(Opcode.DUP);

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

                if (envVar is ClassEnv.Variable classVar)
                {
                    _function.Emit(Opcode.STVAR, classVar.Constant.Index);
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

        public void Visit(Expression.Boolean boolean)
        {
            _function.Emit(Opcode.LDBOOL, (byte)(boolean.Value ? 1 : 0));
        }

        public void Visit(Expression.Null nullExpr)
        {
            _function.Emit(Opcode.LDNULL);
        }

        public void Visit(Statement.Block block)
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

        public void Visit(Statement.TryCatch tryCatch)
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

        public void Visit(Statement.IfStatement ifStmt)
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

        public void Visit(Expression.FuncValue funcValue)
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

        public void Visit(Statement.Throw throwStmt)
        {
            if (throwStmt.Value is null)
                _function.Emit(Opcode.LDNULL);
            else
                Evaluate(throwStmt.Value);

            _function.Emit(Opcode.THROW);
        }

        public void Visit(Statement.StaticStmt staticStmt)
        {
            // Static function
            EnterFunction(_module.Functions[0]);
            Evaluate(staticStmt.Stmt);
            LeaveFunction();
        }

        public void Visit(Expression.EventValue eventValue)
        {
            _function.Emit(Opcode.LDEVENT);
        }

        public void Visit(Expression.ListValue listValue)
        {
            // Push values backwards
            for(int i = listValue.Values.Count - 1;  i >= 0; i--)
            {
                Evaluate(listValue.Values[i]);
            }

            _function.Emit(Opcode.LDLIST, (byte)listValue.Values.Count);
        }

        public void Visit(Statement.TopEventDef topEventDef)
        {
            throw new NotImplementedException();
        }

        public void Visit(Expression.GetIndex getIndex)
        {
            Evaluate(getIndex.Index);
            Evaluate(getIndex.Left);

            _function.Emit(Opcode.LDINDEX);
        }

        public void Visit(Expression.SetIndex setIndex)
        {
            Evaluate(setIndex.Value);
            _function.Emit(Opcode.DUP);
            Evaluate(setIndex.Index);
            Evaluate(setIndex.Left);

            _function.Emit(Opcode.STINDEX);
        }

        public void Visit(Expression.GetProperty getProperty)
        {
            Constant propName = _module.AddConstant(new Constant.String(getProperty.Property));
            Evaluate(getProperty.Left);

            _function.Emit(Opcode.LDPROP, propName);
        }

        public void Visit(Expression.SetProperty setProperty)
        {
            Constant propName = _module.AddConstant(new Constant.String(setProperty.Property));

            Evaluate(setProperty.Value);
            _function.Emit(Opcode.DUP);
            Evaluate(setProperty.Left);

            _function.Emit(Opcode.STPROP, propName);
        }

        public void Visit(Statement.WhileStatement whileStmt)
        {
            int conditionIdx = _function.Instructions.Count;
            Evaluate(whileStmt.Condition);

            int jmpIdx = _function.Instructions.Count;
            _function.Emit(Opcode.JMPF, 0);

            Evaluate(whileStmt.Body);
            _function.Emit(Opcode.JMPB, _function.Instructions.Count - conditionIdx);

            _function.Instructions[jmpIdx].Argument = (uint)(_function.Instructions.Count - jmpIdx);
        }

        public void Visit(Statement.ForStatement forStmt)
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

        public void Visit(Expression.DictValue dictValue)
        {
            foreach(var pair in dictValue.Pairs)
            {
                Evaluate(pair.value);
                Evaluate(pair.key);
            }

            _function.Emit(Opcode.LDOBJ, (byte)dictValue.Pairs.Count);
        }

        public void Visit(Expression.Iterator iteratorValue)
        {
            Evaluate(iteratorValue.Value);
            _function.Emit(Opcode.ITER);
        }

        public void Visit(Statement.ForEachStatement forEachStmt)
        {
            _env.PushFrame();

            LocalVariable iterator_variable = _function.DeclareLocal();
            var variable = new FuncEnv.Variable(forEachStmt.VariableName, iterator_variable);
            _env.DefineVariable(forEachStmt.VariableName, variable);

            Evaluate(forEachStmt.Value);
            _function.Emit(Opcode.ITER);
            int cond_idx = _function.Instructions.Count;
            _function.Emit(Opcode.DUP);

            Constant available_prop = _module.AddConstant(new Constant.String("available"));
            Constant next_prop = _module.AddConstant(new Constant.String("next"));

            _function.Emit(Opcode.LDPROP, available_prop);

            int jmpf_idx = _function.Instructions.Count;
            _function.Emit(Opcode.JMPF, 0);

            _function.Emit(Opcode.DUP);
            _function.Emit(Opcode.LDPROP, next_prop);
            _function.Emit(Opcode.STLOCAL, iterator_variable);
            Evaluate(forEachStmt.Body);
            _function.Emit(Opcode.JMPB, _function.Instructions.Count - cond_idx);

            _function.Instructions[jmpf_idx].Argument = (uint)(_function.Instructions.Count - jmpf_idx);

            _env.PopFrame();
        }

        public void Visit(Statement.TopClassDef topClassDef)
        {
            // Setup class and local env
            string class_name = (string)topClassDef.Name.Value;
            Class cls = _module.CreateClass(class_name, GetVisibility(topClassDef.Visibility));

            Variable variable = _module.Variables[_module.Variables.Count - 1];
            _variables[class_name] = variable;

            var cls_env = new ClassEnv(_env);
            _env = cls_env;

            foreach(var member in topClassDef.Members)
            {
                var name = (string)member.Value;
                Constant var_name = _module.AddConstant(new Constant.String(name));
                cls.Members.Add(var_name);
                _env.DefineVariable(name, new ClassEnv.Variable(name, var_name));
            }

            Function constructor = _module.CreateAnonymousFunction(topClassDef.Constructor is not null ? topClassDef.Constructor.Args.Count + 1 : 1);
            cls.Constructor = constructor;

            foreach (var func in topClassDef.Functions)
            {
                string name = (string)func.name.Value;

                Constant var_name = _module.AddConstant(new Constant.String(name));
                cls.Members.Add(var_name);
                cls_env.DefineVariable(name, new ClassEnv.Variable(name, var_name));

                Function function = _module.CreateAnonymousFunction(func.func.Args.Count);

                EnterFunction(function);
                GenerateFunctionBody(func.func);
                LeaveFunction();

                constructor.Emit(Opcode.LDFUNC, function);
                constructor.Emit(Opcode.STVAR, var_name);
            }

            if(topClassDef.Constructor is not null)
            {
                var ctor = topClassDef.Constructor;
                
                EnterFunction(constructor);
                GenerateFunctionBody(ctor);
                LeaveFunction();
            }

            _env = _env.Parent;
        }

        public void Visit(Expression.New newValue)
        {
            for (int i = newValue.Arguments.Count - 1; i >= 0; i--)
            {
                Evaluate(newValue.Arguments[i]);
            }
            Evaluate(newValue.Callee);

            _function.Emit(Opcode.NEW, newValue.Arguments.Count);
        }

        private void GenerateFunctionBody(Expression.FuncValue func)
        {

            for (int i = 0; i < func.Args.Count; i++)
            {
                ((FuncEnv)_env).Arguments[func.Args[i]] = i;
            }

            foreach (var stmt in func.Body)
            {
                Evaluate(stmt);

                if (stmt is Statement.Return) break;
            }
        }

        private void EnterFunction(Function function)
        {
            if (_function is not null)
                _functionStack.Push(_function);

            // Setup function and local env
            _function = function;
            _env = new FuncEnv(_env);
        }

        private void LeaveFunction()
        {
            if (_functionStack.Count != 0)
                _function = _functionStack.Pop();

            _env = _env.Parent;
        }

        private VariableType GetVisibility(TokenType visibility)
        {
            switch (visibility)
            {
                case TokenType.PRIVATE: return VariableType.PRIVATE;
                case TokenType.PUBLIC: return VariableType.PUBLIC;
                case TokenType.EXTERN: return VariableType.EXTERNAL;
            }

            return VariableType.PRIVATE;
        }

        public void Visit(Expression.SingleOperatorExpr singleOpExpr)
        {
            switch(singleOpExpr.Operator)
            {
                case TokenType.DOUBLE_PLUS:
                case TokenType.DOUBLE_MINUS:
                    {
                        var add_tree = new Expression.SingleOpWrapper(new Expression.BinaryOperation(singleOpExpr.Value, new Expression.Number(1), singleOpExpr.Operator == TokenType.DOUBLE_PLUS ? TokenType.PLUS : TokenType.MINUS), !singleOpExpr.Value.SuffixDup);
                        
                        if(singleOpExpr.Value.Value is Expression.Variable varExpr)
                        {
                            Evaluate(new Expression.AssignVariable(varExpr.Data.Location, (string)varExpr.Data.Value, add_tree));
                        }
                        else if (singleOpExpr.Value.Value is Expression.GetIndex idx)
                        {
                            Evaluate(new Expression.SetIndex(idx.Left, idx.Index, add_tree));
                        }
                        else if (singleOpExpr.Value.Value is Expression.GetProperty prop)
                        {
                            Evaluate(new Expression.SetProperty(prop.Left, prop.Property, add_tree));
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        _function.Emit(Opcode.POP, 1);
                    }
                    break;

                case TokenType.MINUS:
                    {
                        Constant num = _module.AddConstant(new Constant.Number(0));
                        _function.Emit(Opcode.LDCONST, num);
                        Evaluate(singleOpExpr.Value);
                        _function.Emit(Opcode.SUB);
                    }
                    break;

                case TokenType.BANG:
                    {
                        Evaluate(singleOpExpr.Value);
                        _function.Emit(Opcode.NOT);
                    }
                    break;

                default:
                    throw new NotImplementedException();

            }
        }

        public void Visit(Expression.SingleOpWrapper singleOpWrapper)
        {
            Evaluate(singleOpWrapper.Value);

            if (singleOpWrapper.SuffixDup)
                _function.Emit(Opcode.DUP);
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
