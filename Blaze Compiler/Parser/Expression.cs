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
            void VisitBinaryOp(BinaryOperation binOp);
            void VisitVariable(Variable variable);
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
    }
}
