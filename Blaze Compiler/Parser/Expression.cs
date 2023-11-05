using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public abstract class Expression
    {
        public interface IVisitor
        {
            void VisitNumber(Number number);
            void VisitString(String str);
            void VisitBool(Boolean boolean);
            void VisitNull(Null nullExpr);
            void VisitBinaryOp(BinaryOperation binOp);
            void VisitVariable(Variable variable);
            void VisitCall(Call call);
            void VisitAssignVar(AssignVariable assignVar);
        }

        public abstract void Accept(IVisitor visitor);



        public class Number : Expression
        {
            public double Value;

            public Number(double value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitNumber(this);
            }
        }

        public class String : Expression
        {
            public string Value;

            public String(string value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitString(this);
            }
        }

        public class Boolean : Expression
        {
            public bool Value;

            public Boolean(bool value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitBool(this);
            }
        }

        public class Null : Expression
        {

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitNull(this);
            }
        }

        public class Variable : Expression
        {
            public Token Data;

            public Variable(Token data)
            {
                Data = data;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitVariable(this);
            }
        }

        public class BinaryOperation : Expression
        {
            public Expression Left;
            public Expression Right;
            public TokenType Operator;

            public BinaryOperation(Expression left, Expression right, TokenType op)
            {
                Left = left;
                Right = right;
                Operator = op;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitBinaryOp(this);
            }
        }

        public class Call : Expression
        {
            public Expression Callee;
            public List<Expression> Arguments;

            public Call(Expression callee, List<Expression> arguments)
            {
                Callee = callee;
                Arguments = arguments;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitCall(this);
            }
        }

        public class AssignVariable : Expression
        {
            public TokenLocation Location;
            public string Name;
            public Expression Value;

            public AssignVariable(TokenLocation location, string name, Expression value)
            {
                Location = location;
                Name = name;
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitAssignVar(this);
            }
        }
    }
}
