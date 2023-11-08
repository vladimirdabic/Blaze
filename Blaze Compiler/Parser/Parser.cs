using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _current;

        private static readonly Dictionary<TokenType, PrecedenceInfo> _operatorPrecedence = new Dictionary<TokenType, PrecedenceInfo>()
        {
            { TokenType.OR, new PrecedenceInfo(0, PrecAssoc.LEFT) },
            { TokenType.AND, new PrecedenceInfo(5, PrecAssoc.LEFT) },

            { TokenType.DOUBLE_EQUALS, new PrecedenceInfo(10, PrecAssoc.LEFT) },
            { TokenType.NOT_EQUALS, new PrecedenceInfo(10, PrecAssoc.LEFT) },

            { TokenType.GREATER, new PrecedenceInfo(20, PrecAssoc.LEFT) },
            { TokenType.LESS, new PrecedenceInfo(20, PrecAssoc.LEFT) },
            { TokenType.GREATER_EQUALS, new PrecedenceInfo(20, PrecAssoc.LEFT) },
            { TokenType.LESS_EQUALS, new PrecedenceInfo(20, PrecAssoc.LEFT) },
            
            { TokenType.PLUS, new PrecedenceInfo(30, PrecAssoc.LEFT) },
            { TokenType.MINUS, new PrecedenceInfo(30, PrecAssoc.LEFT) },
            { TokenType.STAR, new PrecedenceInfo(40, PrecAssoc.LEFT) },
            { TokenType.SLASH, new PrecedenceInfo(40, PrecAssoc.LEFT) },
        };

        /// <summary>
        /// Generates an abstract syntax tree from a list of tokens
        /// </summary>
        /// <param name="tokens">The list of tokens to use</param>
        /// <returns>An abstract syntax tree</returns>
        public Statement Parse(List<Token> tokens)
        {
            _tokens = tokens;
            _current = 0;

            List<Statement> definitions = new List<Statement>();

            while(Available())
            {
                int line = Peek().Location.Line;
                Statement topDef = ParseTopDef();
                topDef.Line = line;
                definitions.Add(topDef);
            }

            return new Statement.Definitions(definitions);
        }

        private Statement ParseTopDef()
        {
            // private is the default visibility
            TokenType visibility = TokenType.PRIVATE;

            if(Match(TokenType.PRIVATE, TokenType.PUBLIC, TokenType.EXTERN))
                visibility = Prev().Type;

            if(Match(TokenType.VAR))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected variable name after 'var'");
                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new Statement.TopVariableDef(visibility, name.Location, (string)name.Value);
            }

            if(Match(TokenType.FUNC))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected function name after 'func'");
                Consume(TokenType.OPEN_PAREN, "Expected function arguments after func name");

                var args = new List<string>();

                if (!Check(TokenType.CLOSE_PAREN))
                {
                    do
                    {
                        Token arg = Consume(TokenType.IDENTIFIER, "Expected func argument name");
                        args.Add((string)arg.Value);
                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.CLOSE_PAREN, "Expected ')' after func arguments");
                Consume(TokenType.OPEN_BRACE, "Expected func body");

                var body = new List<Statement>();

                while (Available() && !Check(TokenType.CLOSE_BRACE))
                {
                    body.Add(ParseStatement());
                }

                Consume(TokenType.CLOSE_BRACE, "Expected '}' to close func body");

                return new Statement.TopFuncDef(visibility, name.Location, (string)name.Value, args, body);
            }

            if(Match(TokenType.STATIC))
            {
                Statement stmt = ParseStatement();
                return new Statement.StaticStmt(stmt);
            }

            throw new ParserException(Peek().Location.Source, Peek().Location.Line, "Expected a declaration");
        }

        private Statement ParseStatement()
        {
            if(Match(TokenType.RETURN))
            {
                Expression value = ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after return statement");

                return new Statement.Return(value);
            }

            if (Match(TokenType.VAR))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected variable name after 'var'");
                Expression value = Match(TokenType.EQUALS) ? ParseExpression() : null;
                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new Statement.LocalVariableDef(name.Location, (string)name.Value, value);
            }

            if (Match(TokenType.OPEN_BRACE))
            {
                var body = new List<Statement>();

                while (Available() && !Check(TokenType.CLOSE_BRACE))
                {
                    body.Add(ParseStatement());
                }

                Consume(TokenType.CLOSE_BRACE, "Expected '}' after statement block");
                return new Statement.Block(body);
            }

            if (Match(TokenType.TRY))
            {
                Statement tryStmt = ParseStatement();
                Consume(TokenType.CATCH, "Expected catch after try");
                string name = null;

                if(Match(TokenType.OPEN_PAREN))
                {
                    name = (string)Consume(TokenType.IDENTIFIER, "Expected catch variable name").Value;
                    Consume(TokenType.CLOSE_PAREN, "Expected ')' after catch variable name");
                }

                Statement catchStmt = ParseStatement();

                return new Statement.TryCatch(tryStmt, catchStmt, name);
            }

            if(Match(TokenType.IF))
            {
                Consume(TokenType.OPEN_PAREN, "Expected '(' after if");
                Expression condition = ParseExpression();
                Consume(TokenType.CLOSE_PAREN, "Expected ')' after if condition");

                Statement body = ParseStatement();
                Statement elseBody = Match(TokenType.ELSE) ? ParseStatement() : null;

                return new Statement.IfStatement(condition, body, elseBody);
            }

            if (Match(TokenType.THROW))
            {
                Expression value = Check(TokenType.SEMICOLON) ? null : ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after throw statement");

                return new Statement.Throw(value);
            }

            Expression expr = ParseExpression();

            // Only allow Assignments and Function Calls
            if(!(expr is Expression.Call || expr is Expression.AssignVariable))
                throw new ParserException(Peek().Location.Source, Peek().Location.Line, "Expected a statement");

            Consume(TokenType.SEMICOLON, "Expected ';' after statement");
            return new Statement.ExprStmt(expr);
        }

        private Expression ParseExpression()
        {
            return ParseAssign();
        }

        private Expression ParseAssign()
        {
            Expression left = ParseBinaryOperation(0);

            while(Match(TokenType.EQUALS))
            {
                if(left is Expression.Variable variable)
                {
                    Expression value = ParseExpression();
                    left = new Expression.AssignVariable(variable.Data.Location, (string)variable.Data.Value, value);
                }
                // TODO: Assign to indexing operations
            }

            return left;
        }

        private Expression ParseBinaryOperation(int precedence)
        {
            Expression left = ParseCall();

            while(true)
            {
                if (!Available()) break;
                Token op = Peek();
                if (!_operatorPrecedence.ContainsKey(op.Type)) break; // Is not an operator so break

                var precData = _operatorPrecedence[op.Type];
                if (precData.Level < precedence) break;  // Break if level is smaller than current level

                Advance(); // Consume the operator token finally

                // If the associativity is left, increase by 1
                // Example: Parsing addition, this means parse the right value with higher precedence (multiplication, division, etc...)
                // 1 + 2 * 20 + 3 = (1 + (2 * 20)) + 3
                // If the associativity is right then the left value will have the same precedence (exponents)
                // 1^2 + 3^2^3 = (1^2) + (3^(2^3)) 
                int next_prec = precData.Assoc == PrecAssoc.LEFT ? precData.Level + 1 : precData.Level;

                Expression right = ParseBinaryOperation(next_prec);
                left = new Expression.BinaryOperation(left, right, op.Type);
            }

            return left;
        }

        // Parse call and indexing
        private Expression ParseCall() {
            Expression left = ParsePrimary();

            while(true)
            {
                if(Match(TokenType.OPEN_PAREN))
                {
                    var args = new List<Expression>();
                    
                    if(!Check(TokenType.CLOSE_PAREN))
                    {
                        while (true)
                        {
                            args.Add(ParseExpression());
                            if (Check(TokenType.CLOSE_PAREN) || !Available()) break;
                            Consume(TokenType.COMMA, "Expected ',' after function argument");
                        }
                    }

                    Consume(TokenType.CLOSE_PAREN, "Expected ')' after function arguments");

                    left = new Expression.Call(left, args);
                }
                else if(Match(TokenType.OPEN_SQUARE))
                {
                    // TODO: Indexing
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Expression ParsePrimary()
        {
            if (Match(TokenType.NUMBER))
                return new Expression.Number((double)Prev().Value);

            if (Match(TokenType.STRING))
                return new Expression.String((string)Prev().Value);

            if (Match(TokenType.IDENTIFIER))
                return new Expression.Variable(Prev());

            if (Match(TokenType.NULL))
                return new Expression.Null();

            if (Match(TokenType.TRUE, TokenType.FALSE))
                return new Expression.Boolean(Prev().Type == TokenType.TRUE);

            if (Match(TokenType.OPEN_PAREN))
            {
                Expression expr = ParseExpression();
                Consume(TokenType.CLOSE_PAREN, "Expected ')'");
                return expr;
            }

            if (Match(TokenType.FUNC))
            {
                Token keyword = Prev();
                string name = Check(TokenType.IDENTIFIER) ? (string)Advance().Value : null;
                Consume(TokenType.OPEN_PAREN, "Expected function arguments after func");

                var args = new List<string>();

                if (!Check(TokenType.CLOSE_PAREN))
                {
                    do
                    {
                        Token arg = Consume(TokenType.IDENTIFIER, "Expected func argument name");
                        args.Add((string)arg.Value);
                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.CLOSE_PAREN, "Expected ')' after func arguments");
                Consume(TokenType.OPEN_BRACE, "Expected func body");

                var body = new List<Statement>();

                while (Available() && !Check(TokenType.CLOSE_BRACE))
                {
                    body.Add(ParseStatement());
                }

                Consume(TokenType.CLOSE_BRACE, "Expected '}' to close func body");

                return new Expression.FuncValue(keyword.Location, name, args, body);
            }

            throw new ParserException(Peek().Location.Source, Peek().Location.Line, "Expected an expression");
        }

        // Helper functions
        private Token Consume(TokenType type, string message)
        {
            Token token = Advance();

            if (token.Type != type)
                throw new ParserException(token.Location.Source, token.Location.Line, message);

            return token;
        }

        private bool Match(params TokenType[] types)
        {
            foreach(TokenType t in types)
            {
                if(Peek().Type == t)
                {
                    _current++;
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            return Peek().Type == type;
        }

        private Token Advance()
        {
            return _tokens[_current++];
        }

        private Token Prev()
        {
            return _tokens[_current - 1];
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private bool Available()
        {
            return Peek().Type != TokenType.EOF;
        }
    }


    public class ParserException : Exception
    {
        public new string Source;
        public int Line;

        public ParserException(string source, int line, string message) : base(message)
        {
            Source = source;
            Line = line;
        }
    }

    // Precedence Associativity
    public enum PrecAssoc
    {
        LEFT, RIGHT
    }

    public record struct PrecedenceInfo(int Level, PrecAssoc Assoc);
}
