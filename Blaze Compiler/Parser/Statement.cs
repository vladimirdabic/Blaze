using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public abstract class Statement
    {
        public int Line;

        public interface IVisitor
        {
            void Visit(ExprStmt exprStmt);
            void Visit(Definitions definitions);
            void Visit(TopVariableDef topVarDef);
            void Visit(TopFuncDef topFuncDef);
            void Visit(Return returnStmt);
            void Visit(LocalVariableDef localVarDef);
            void Visit(Block block);
            void Visit(TryCatch tryCatch);
            void Visit(Throw throwStmt);
            void Visit(IfStatement ifStmt);
            void Visit(StaticStmt staticStmt);
            void Visit(EventDef eventDef);
            void Visit(WhileStatement whileStmt);
            void Visit(ForStatement forStmt);
            void Visit(ForEachStatement forEachStmt);
            void Visit(TopClassDef topClassDef);
            void Visit(Break breakStmt);
            void Visit(Continue continueStmt);
        }

        public abstract void Accept(IVisitor visitor);


        public class ExprStmt : Statement
        {
            public Expression Expr;

            public ExprStmt(Expression expr)
            {
                Expr = expr;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Definitions : Statement
        {
            public List<Statement> Statements;

            public Definitions(List<Statement> statements)
            {
                Statements = statements;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class TopVariableDef : Statement
        {
            // Using TokenType for visibility (PRIVATE, PUBLIC, EXTERN)
            public TokenType Visibility;
            public TokenLocation Location;
            public string Name;
            public Expression Value;

            public TopVariableDef(TokenType visibility, TokenLocation location, string name, Expression value)
            {
                Visibility = visibility;
                Name = name;
                Location = location;
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class TopFuncDef : Statement
        {
            // Using TokenType for visibility (PRIVATE, PUBLIC, EXTERN)
            public TokenType Visibility;
            public TokenLocation Location;
            public string Name;
            public List<string> Args;
            public List<Statement> Body;

            public TopFuncDef(TokenType visibility, TokenLocation location, string name, List<string> args, List<Statement> body)
            {
                Visibility = visibility;
                Name = name;
                Location = location;
                Args = args;
                Body = body;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class EventDef : Statement
        {
            public TokenLocation Location;
            public Expression Event;
            public List<(string name, Expression.ListValue values)> Args;
            public List<Statement> Body;
            public bool Static;

            public EventDef(TokenLocation location, Expression _event, List<(string name, Expression.ListValue value)> args, List<Statement> body, bool _static = false)
            {
                Event = _event;
                Location = location;
                Args = args;
                Body = body;
                Static = _static;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Return : Statement
        {
            public Expression Value;

            public Return(Expression value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class LocalVariableDef : Statement
        {
            public TokenLocation Location;
            public string Name;
            public Expression Value;

            public LocalVariableDef(TokenLocation location, string name, Expression value)
            {
                Location = location;
                Name = name;
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Block : Statement
        {
            public List<Statement> Statements;

            public Block(List<Statement> statements)
            {
                Statements = statements;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class TryCatch : Statement
        {
            public Statement TryStmt;
            public Statement CatchStmt;
            public string CatchName;

            public TryCatch(Statement tryStmt, Statement catchStmt, string catchName)
            {
                TryStmt = tryStmt;
                CatchStmt = catchStmt;
                CatchName = catchName;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Throw : Statement
        {
            public Expression Value;

            public Throw(Expression value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class IfStatement : Statement
        {
            public Expression Condition;
            public Statement Body;
            public Statement Else;

            public IfStatement(Expression condition, Statement body, Statement _else)
            {
                Condition = condition;
                Body = body;
                Else = _else;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class WhileStatement : Statement
        {
            public Expression Condition;
            public Statement Body;

            public WhileStatement(Expression condition, Statement body)
            {
                Condition = condition;
                Body = body;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class ForStatement : Statement
        {
            public Statement Initializer;
            public Expression Condition;
            public Expression Increment;
            public Statement Body;

            public ForStatement(Statement initializer, Expression condition, Expression increment, Statement body)
            {
                Condition = condition;
                Body = body;
                Increment = increment;
                Initializer = initializer;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class StaticStmt : Statement
        {
            public Statement Stmt;

            public StaticStmt(Statement statement)
            {
                Stmt = statement;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class ForEachStatement : Statement
        {
            public string VariableName;
            public Expression Value;
            public Statement Body;

            public ForEachStatement(string variableName, Expression value, Statement body)
            {
                VariableName = variableName;
                Value = value;
                Body = body;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class TopClassDef : Statement
        {
            public Token Name;
            public string ParentName;
            public TokenType Visibility;
            public List<Token> Members;
            public Expression.FuncValue Constructor;
            public List<(Token name, Expression.FuncValue func)> Functions; 

            public TopClassDef(Token name, string parentName, List<Token> members, Expression.FuncValue constructor, List<(Token name, Expression.FuncValue func)> functions, TokenType visibility)
            {
                Name = name;
                Members = members;
                Constructor = constructor;
                Functions = functions;
                Visibility = visibility;
                ParentName = parentName;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Break : Statement
        {
            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Continue : Statement
        {
            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}
