using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;
using VD.Blaze.Parser;
using VD.Blaze.Lexer;
using static VD.Blaze.Parser.Statement;
using System.Runtime.Remoting.Activation;

namespace VD.Blaze.Generator
{
    public class Generator : Expression.IVisitor, Statement.IVisitor
    {
        private Module.Module _module;
        private Dictionary<string, Variable> _variables;
        private Dictionary<string, Function> _functions;

        private Stack<Function> _functionStack;
        private Function _function;
        private string _source;
        private int _line;

        private LocalEnvironment _localEnv;

        public Generator()
        {
            _functionStack = new Stack<Function>();
            _variables = new Dictionary<string, Variable>();
            _functions = new Dictionary<string, Function>();
        }

        public Module.Module Generate(Statement statement, string source)
        {
            _module = new Module.Module();
            _functionStack.Clear();
            _variables.Clear();
            _functions.Clear();
            _function = null;
            _localEnv = null;
            _source = source;
            _line = 1;

            Evaluate(statement);

            return _module;
        }

        private void Evaluate(Statement stmt)
        {
            if(stmt.Line != 0)
                _line = stmt.Line;
            
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
            _functions[topFuncDef.Name] = _function;
            _localEnv = new LocalEnvironment(_localEnv);

            // Setup arg indicies
            for(int i = 0;  i < topFuncDef.Args.Count; i++)
            {
                _localEnv.Args[topFuncDef.Args[i]] = i;
            }

            foreach (Statement stmt in topFuncDef.Body)
            {
                Evaluate(stmt);
            }

            if(_functionStack.Count != 0)
                _function = _functionStack.Pop();

            _localEnv = _localEnv.Parent;
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

                default:
                    throw new NotImplementedException();
            }
        }

        public void VisitExprStmt(ExprStmt exprStmt)
        {
            Evaluate(exprStmt.Expr);
            _function.Emit(Opcode.POP, 1);
        }

        public void VisitVariable(Expression.Variable variable)
        {
            string name = (string)variable.Data.Value;

            (LocalVariable local, int level) = _localEnv.GetLocal(name);
            
            if(local is not null)
            {
                if (level != 0)
                    _function.Emit(Opcode.EXTENDED_ARG, (byte)level);

                _function.Emit(Opcode.LDLOCAL, local);
                return;
            }

            if(_localEnv.Args.ContainsKey(name))
            {
                _function.Emit(Opcode.LDARG, (byte)_localEnv.Args[name]);
                return;
            }

            if (_variables.ContainsKey(name))
            {
                Variable moduleVar = _variables[name];
                _function.Emit(Opcode.LDVAR, moduleVar);
                return;
            }

            throw new GeneratorException(_source, _line, $"Undefined variable '{name}'");
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
