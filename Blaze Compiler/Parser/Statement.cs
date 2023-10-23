using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public abstract class Statement
    {
        public interface IVisitor
        {
            void VisitDefinitions(Definitions definitions);
            void VisitTopVarDef(TopVariableDef topVarDef);
            void VisitTopFuncDef(TopFuncDef topVarDef);
        }

        public abstract void Accept(IVisitor visitor);


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

            public TopVariableDef(TokenType visibility, TokenLocation location, string name)
            {
                Visibility = visibility;
                Name = name;
                Location = location;
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
    }
}
