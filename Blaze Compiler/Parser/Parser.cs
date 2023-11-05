﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Lexer;

namespace VD.Blaze.Parser
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _current;

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
            Expression left = ParseAdd();

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

        private Expression ParseAdd()
        {
            Expression left = ParseMultiply();

            while(Match(TokenType.PLUS, TokenType.MINUS))
            {
                TokenType op = Prev().Type;
                left = new Expression.BinaryOperation(left, ParseMultiply(), op);
            }

            return left;
        }

        private Expression ParseMultiply()
        {
            Expression left = ParseCall();

            while (Match(TokenType.STAR, TokenType.SLASH))
            {
                TokenType op = Prev().Type;
                left = new Expression.BinaryOperation(left, ParseCall(), op);
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
}
