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
                Expression value = Match(TokenType.EQUALS) ? ParseExpression() : null;
                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new Statement.TopVariableDef(visibility, name.Location, (string)name.Value, value);
            }

            if(Match(TokenType.IMPORT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected variable name after 'import'");
                Expression value = Match(TokenType.EQUALS) ? ParseExpression() : null;
                Consume(TokenType.SEMICOLON, "Expected ';' after import statement");

                return new Statement.TopVariableDef(TokenType.EXTERN, name.Location, (string)name.Value, value);
            }

            if (Match(TokenType.EXPORT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected variable name after 'export'");
                Expression value = Match(TokenType.EQUALS) ? ParseExpression() : null;
                Consume(TokenType.SEMICOLON, "Expected ';' after export statement");

                return new Statement.TopVariableDef(TokenType.PUBLIC, name.Location, (string)name.Value, value);
            }

            if (Match(TokenType.FUNC))
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
                    int line = Peek().Location.Line;
                    Statement stmt = ParseStatement();
                    stmt.Line = line;
                    body.Add(stmt);
                }

                Consume(TokenType.CLOSE_BRACE, "Expected '}' to close func body");

                return new Statement.TopFuncDef(visibility, name.Location, (string)name.Value, args, body);
            }

            if(Match(TokenType.CLASS))
            {
                return ParseClass(visibility);
            }

            if(Match(TokenType.STATIC))
            {
                int line = Peek().Location.Line;
                Statement stmt = ParseStatement();
                stmt.Line = line;
                return new Statement.StaticStmt(stmt);
            }

            if(Match(TokenType.EVENT))
            {
                return ParseEvent(true);
            }

            throw new ParserException(Peek().Location.Source, Peek().Location.Line, "Expected a declaration");
        }

        private Statement.TopClassDef ParseClass(TokenType visibility)
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected class name after 'class'");
            string parentName = Match(TokenType.COLON) ? (string)Consume(TokenType.IDENTIFIER, "Expected base class name").Value : null;

            Consume(TokenType.OPEN_BRACE, "Expected class body");

            var members = new List<Token>();
            var funcs = new List<(Token, Expression.FuncValue)>();
            Expression.FuncValue constructor = null;

            while (Available() && !Check(TokenType.CLOSE_BRACE))
            {
                if(Match(TokenType.VAR))
                {
                    Token varname = Consume(TokenType.IDENTIFIER, "Expected member name after 'var'");
                    Consume(TokenType.SEMICOLON, "Expected ';' after class member");
                    members.Add(varname);
                }
                // constructor
                else if(MatchIdentifier((string)name.Value))
                {
                    constructor = ParseFuncValue();
                }
                else if(Match(TokenType.FUNC))
                {
                    Token varname = Consume(TokenType.IDENTIFIER, "Expected method name after 'func'");
                    var func_value = ParseFuncValue();
                    funcs.Add((varname, func_value));
                }
            }

            Consume(TokenType.CLOSE_BRACE, "Expected '}' to close class body");

            return new Statement.TopClassDef(name, parentName, members, constructor, funcs, visibility);
        }

        private Statement.EventDef ParseEvent(bool is_static = false)
        {
            Token loc = Prev();
            Expression event_expr = ParseIndex();
            var event_args = new List<(string, Expression.ListValue)>();

            if (Match(TokenType.OPEN_PAREN))
            {
                if (!Check(TokenType.CLOSE_PAREN))
                {
                    do
                    {
                        Token pos = Peek();
                        Expression.ListValue exprs = null;
                        string arg_name = Match(TokenType.IDENTIFIER) ? (string)Prev().Value : null;

                        if (Match(TokenType.OPEN_SQUARE))
                            exprs = ParseList();

                        if (arg_name is null && exprs is null)
                            throw new ParserException(pos.Location.Source, pos.Location.Line, "Event argument must be a name or a list of allowed values");

                        event_args.Add((arg_name, exprs));

                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.CLOSE_PAREN, "Expected ')' after func arguments");
            }

            Consume(TokenType.OPEN_BRACE, "Expected event body");
            var body = new List<Statement>();

            while (Available() && !Check(TokenType.CLOSE_BRACE))
            {
                int line = Peek().Location.Line;
                Statement stmt = ParseStatement();
                stmt.Line = line;
                body.Add(stmt);
            }

            Consume(TokenType.CLOSE_BRACE, "Expected '}' to close event body");

            return new Statement.EventDef(loc.Location, event_expr, event_args, body, is_static);
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
                    int line = Peek().Location.Line;
                    Statement stmt = ParseStatement();
                    stmt.Line = line;
                    body.Add(stmt);
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

            if (Match(TokenType.WHILE))
            {
                Consume(TokenType.OPEN_PAREN, "Expected '(' after while");
                Expression condition = ParseExpression();
                Consume(TokenType.CLOSE_PAREN, "Expected ')' after while condition");

                Statement body = ParseStatement();

                return new Statement.WhileStatement(condition, body);
            }

            if (Match(TokenType.FOR))
            {
                Consume(TokenType.OPEN_PAREN, "Expected '(' after for");

                // Foreach
                if(MatchSequence(TokenType.VAR, TokenType.IDENTIFIER, TokenType.COLON))
                {
                    Token var_name = _tokens[_current - 2];
                    Expression iterable = ParseExpression();
                    Consume(TokenType.CLOSE_PAREN, "Expected ')' to close '(' in the for statement");

                    Statement body_fe = ParseStatement();

                    return new Statement.ForEachStatement((string)var_name.Value, iterable, body_fe);
                }

                Statement initializer = ParseStatement();
                Expression condition = ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after for condition");
                Expression increment = ParseExpression();

                Consume(TokenType.CLOSE_PAREN, "Expected ')' to close '(' in the for statement");

                Statement body = ParseStatement();

                return new Statement.ForStatement(initializer, condition, increment, body);
            }

            if (Match(TokenType.THROW))
            {
                Expression value = Check(TokenType.SEMICOLON) ? null : ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after throw statement");

                return new Statement.Throw(value);
            }

            if (Match(TokenType.EVENT))
            {
                return ParseEvent();
            }

            if (Match(TokenType.BREAK))
            {
                Consume(TokenType.SEMICOLON, "Expected ';' after break statement");
                return new Statement.Break();
            }

            if (Match(TokenType.CONTINUE))
            {
                Consume(TokenType.SEMICOLON, "Expected ';' after continue statement");
                return new Statement.Continue();
            }

            Expression expr = ParseExpression();

            // Only allow Assignments and Function Calls
            if(!(expr is Expression.Call || expr is Expression.AssignVariable || expr is Expression.SetIndex || expr is Expression.SetProperty))
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
                else if(left is Expression.GetIndex getIndex)
                {
                    Expression value = ParseExpression();
                    left = new Expression.SetIndex(getIndex.Left, getIndex.Index, value);
                }
                else if (left is Expression.GetProperty getProperty)
                {
                    Expression value = ParseExpression();
                    left = new Expression.SetProperty(getProperty.Left, getProperty.Property, value);
                }
            }

            return left;
        }

        private Expression ParseBinaryOperation(int precedence)
        {
            Expression left = ParseSuffixOp();

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

        private Expression ParseSuffixOp()
        {
            Expression left = ParseCall();

            while(Match(TokenType.DOUBLE_PLUS, TokenType.DOUBLE_MINUS))
            {
                Token op = Prev();
                left = new Expression.SingleOperatorExpr(op.Type, new Expression.SingleOpWrapper(left, true));
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
                    Expression idx = ParseExpression();
                    Consume(TokenType.CLOSE_SQUARE, "Expected ']' after index");

                    left = new Expression.GetIndex(left, idx);
                }
                else if(Match(TokenType.DOT))
                {
                    Token idx = Consume(TokenType.IDENTIFIER, "Expected property after '.'");

                    left = new Expression.GetProperty(left, (string)idx.Value);
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
            // Temporary, might leave it in
            if (MatchIdentifier("iter"))
            {
                Expression value = ParseCall();
                return new Expression.Iterator(value);
            }

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

            if (Match(TokenType.EVENT))
                return new Expression.EventValue();

            if (Match(TokenType.BANG, TokenType.MINUS, TokenType.DOUBLE_PLUS, TokenType.DOUBLE_MINUS))
            {
                Token op = Prev();
                Expression expr = ParseIndex();
                return new Expression.SingleOperatorExpr(op.Type, new Expression.SingleOpWrapper(expr, false));
            }

            if (Match(TokenType.NEW))
            {
                Expression callee = ParseIndex();
                var args = new List<Expression>();

                if(Match(TokenType.OPEN_PAREN))
                {
                    if (!Check(TokenType.CLOSE_PAREN))
                    {
                        while (true)
                        {
                            args.Add(ParseExpression());
                            if (Check(TokenType.CLOSE_PAREN) || !Available()) break;
                            Consume(TokenType.COMMA, "Expected ',' after constructor argument");
                        }
                    }

                    Consume(TokenType.CLOSE_PAREN, "Expected ')' after constructor arguments");
                }

                return new Expression.New(callee, args);
            }

            if (Match(TokenType.OPEN_SQUARE))
            {
                return ParseList();
            }

            if (Match(TokenType.OPEN_BRACE))
            {
                var pairs = new List<(Expression, Expression)>();

                if (!Check(TokenType.CLOSE_BRACE))
                {
                    while (true)
                    {
                        var key = ParseExpression();
                        Consume(TokenType.COLON, "Expected ':' and entry value");
                        var value = ParseExpression();

                        pairs.Add((key, value));

                        if (Check(TokenType.CLOSE_BRACE) || !Available()) break;
                        Consume(TokenType.COMMA, "Expected ',' after dictionary entry");
                    }
                }

                Consume(TokenType.CLOSE_BRACE, "Expected '}' to close dictionary");

                return new Expression.DictValue(pairs);
            }

            if (Match(TokenType.FUNC))
            {
                return ParseFuncValue();
            }

            throw new ParserException(Peek().Location.Source, Peek().Location.Line, "Expected an expression");
        }

        private Expression.ListValue ParseList()
        {
            List<Expression> exprs = new List<Expression>();

            if (!Check(TokenType.CLOSE_SQUARE))
            {
                while (true)
                {
                    exprs.Add(ParseExpression());
                    if (Check(TokenType.CLOSE_SQUARE) || !Available()) break;
                    Consume(TokenType.COMMA, "Expected ',' after list value");
                }
            }

            Consume(TokenType.CLOSE_SQUARE, "Expected ']' after list values");

            return new Expression.ListValue(exprs);
        }
        private Expression ParseIndex()
        {
            Expression left = ParsePrimary();

            while (true)
            {
                if (Match(TokenType.OPEN_SQUARE))
                {
                    Expression idx = ParseExpression();
                    Consume(TokenType.CLOSE_SQUARE, "Expected ']' after index");

                    left = new Expression.GetIndex(left, idx);
                }
                else if (Match(TokenType.DOT))
                {
                    Token idx = Consume(TokenType.IDENTIFIER, "Expected property after '.'");

                    left = new Expression.GetProperty(left, (string)idx.Value);
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Expression.FuncValue ParseFuncValue()
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
                int line = Peek().Location.Line;
                Statement stmt = ParseStatement();
                stmt.Line = line;
                body.Add(stmt);
            }

            Consume(TokenType.CLOSE_BRACE, "Expected '}' to close func body");

            return new Expression.FuncValue(keyword.Location, name, args, body);
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

        private bool MatchIdentifier(string value)
        {
            if ((Peek().Type == TokenType.IDENTIFIER) && ((string)Peek().Value) == value)
            {
                _current++;
                return true;
            }

            return false;
        }

        private bool MatchSequence(params TokenType[] types)
        {
            for(int i = 0; i < types.Length; i++)
            {
                TokenType type = types[i];

                if (Peek(i).Type != type) return false;
            }

            _current += types.Length;
            return true;
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

        private Token Peek(int offset)
        {
            return _tokens[_current + offset];
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
