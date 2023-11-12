using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public abstract class Expression
    {
        public interface IVisitor
        {
            void Visit(Number number);
            void Visit(String str);
            void Visit(Boolean boolean);
            void Visit(Null nullExpr);
            void Visit(BinaryOperation binOp);
            void Visit(Variable variable);
            void Visit(Call call);
            void Visit(AssignVariable assignVar);
            void Visit(FuncValue funcValue);
            void Visit(EventValue eventValue);
            void Visit(ListValue listValue);
            void Visit(GetIndex getIndex);
            void Visit(SetIndex setIndex);
            void Visit(GetProperty getProperty);
            void Visit(SetProperty setProperty);
            void Visit(DictValue dictValue);
            void Visit(Iterator iteratorValue);
            void Visit(New newValue);
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
                visitor.Visit(this);
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
                visitor.Visit(this);
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
                visitor.Visit(this);
            }
        }

        public class Null : Expression
        {

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
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
                visitor.Visit(this);
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
                visitor.Visit(this);
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
                visitor.Visit(this);
            }
        }

        public class New : Expression
        {
            public Expression Callee;
            public List<Expression> Arguments;

            public New(Expression callee, List<Expression> arguments)
            {
                Callee = callee;
                Arguments = arguments;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
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
                visitor.Visit(this);
            }
        }

        public class FuncValue : Expression
        {
            public TokenLocation Location;
            public string Name;
            public List<string> Args;
            public List<Statement> Body;

            public FuncValue(TokenLocation location, string name, List<string> args, List<Statement> body)
            {
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

        public class EventValue : Expression
        {
            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class ListValue : Expression
        {
            public List<Expression> Values;

            public ListValue(List<Expression> values)
            {
                Values = values;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class GetIndex : Expression
        {
            public Expression Left;
            public Expression Index;

            public GetIndex(Expression left, Expression index)
            {
                Left = left;
                Index = index;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class SetIndex : Expression
        {
            public Expression Left;
            public Expression Index;
            public Expression Value;

            public SetIndex(Expression left, Expression index, Expression value)
            {
                Left = left;
                Index = index;
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class GetProperty : Expression
        {
            public Expression Left;
            public string Property;

            public GetProperty(Expression left, string property)
            {
                Left = left;
                Property = property;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class SetProperty : Expression
        {
            public Expression Left;
            public string Property;
            public Expression Value;

            public SetProperty(Expression left, string property, Expression value)
            {
                Left = left;
                Property= property;
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class DictValue : Expression
        {
            public List<(Expression key, Expression value)> Pairs;

            public DictValue(List<(Expression, Expression)> pairs)
            {
                Pairs = pairs;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        public class Iterator : Expression
        {
            public Expression Value;

            public Iterator(Expression value)
            {
                Value = value;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}
