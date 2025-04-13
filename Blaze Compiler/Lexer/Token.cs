using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Lexer
{
    public class Token
    {
        public TokenLocation Location;
        public TokenType Type;
        public object Value;

        public Token(TokenType type,  object value, int line, string source)
        {
            Location = new TokenLocation()
            {
                Line = line,
                Source = source
            };

            Type = type;
            Value = value;
        }

        public Token(TokenType type, int line, string source) : this(type, null, line, source) {}
        public Token(TokenType type, object value) : this(type, value, 0, null) {}
        public Token(TokenType type) : this(type, null, 0, null) {}


        public override string ToString()
        {
            if(Value is null)
                return $"Token(type={Type}, location='{Location}')";

            return $"Token(type={Type}, value={Value}, location='{Location}')";
        }
    }

    public enum TokenType
    {
        // + - * /
        PLUS, MINUS, STAR, SLASH,
        DOUBLE_MINUS, DOUBLE_PLUS,

        // ( ), { }, [ ]
        OPEN_PAREN, CLOSE_PAREN, OPEN_BRACE, CLOSE_BRACE, OPEN_SQUARE, CLOSE_SQUARE,

        // ; , .
        SEMICOLON, COLON, COMMA, DOT,

        // = ! < > == != <= >=
        EQUALS, BANG, LESS, GREATER, DOUBLE_EQUALS, NOT_EQUALS, LESS_EQUALS, GREATER_EQUALS,
        AMPERSAND, PIPE, AND, OR,

        // 23.2, "test"
        NUMBER, STRING,

        // [a-z_][a-z0-9_]+
        IDENTIFIER,

        // Reserved words
        IF, ELSE, WHILE, FOR, BREAK, CONTINUE, RETURN,
        FUNC, CLASS, VAR, PRIVATE, PUBLIC, EXTERN,
        TRUE, FALSE, NULL,
        TRY, CATCH, THROW, STATIC, EVENT, NEW,
        IMPORT, EXPORT,

        // End of file
        EOF
    }

    public struct TokenLocation
    {
        public string Source;
        public int Line;

        public override string ToString()
        {
            return $"{Source}:{Line}";
        }
    }
}
