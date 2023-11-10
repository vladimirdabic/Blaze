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
            void VisitExprStmt(ExprStmt exprStmt);
            void VisitDefinitions(Definitions definitions);
            void VisitTopVarDef(TopVariableDef topVarDef);
            void VisitTopFuncDef(TopFuncDef topFuncDef);
            void VisitReturn(Return returnStmt);
            void VisitLocalVarDef(LocalVariableDef localVarDef);
            void VisitBlock(Block block);
            void VisitTryCatch(TryCatch tryCatch);
            void VisitThrow(Throw throwStmt);
            void VisitIf(IfStatement ifStmt);
            void VisitStaticStmt(StaticStmt staticStmt);
            void VisitTopEventDef(TopEventDef topEventDef);
            void VisitWhile(WhileStatement whileStmt);
            void VisitFor(ForStatement forStmt);
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
                visitor.VisitExprStmt(this);
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
                visitor.VisitDefinitions(this);
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
                visitor.VisitTopVarDef(this);
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
                visitor.VisitTopFuncDef(this);
            }
        }

        public class TopEventDef : Statement
        {
            public TokenLocation Location;
            public Expression Event;
            public List<(string name, Expression value)> Args;
            public List<Statement> Body;

            public TopEventDef(TokenLocation location, Expression _event, List<(string name, Expression value)> args, List<Statement> body)
            {
                Event = _event;
                Location = location;
                Args = args;
                Body = body;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitTopEventDef(this);
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
                visitor.VisitReturn(this);
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
                visitor.VisitLocalVarDef(this);
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
                visitor.VisitBlock(this);
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
                visitor.VisitTryCatch(this);
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
                visitor.VisitThrow(this);
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
                visitor.VisitIf(this);
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
                visitor.VisitWhile(this);
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
                visitor.VisitFor(this);
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
                visitor.VisitStaticStmt(this);
            }
        }
    }
}
