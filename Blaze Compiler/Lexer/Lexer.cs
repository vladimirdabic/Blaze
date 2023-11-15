using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Lexer
{
    public class Lexer
    {
        private List<Token> _tokens;
        private int _start, _current, _line;
        private string _source, _context;

        // Reserved words
        private static readonly Dictionary<string, TokenType> s_reservedWords = new Dictionary<string, TokenType>()
        {
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "while", TokenType.WHILE },
            { "for", TokenType.FOR },
            { "break", TokenType.BREAK },
            { "return", TokenType.RETURN },
            { "var", TokenType.VAR },
            { "func", TokenType.FUNC },
            { "class", TokenType.CLASS },
            { "private", TokenType.PRIVATE },
            { "public", TokenType.PUBLIC },
            { "extern", TokenType.EXTERN },
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "null", TokenType.NULL },
            { "try", TokenType.TRY },
            { "catch", TokenType.CATCH },
            { "throw", TokenType.THROW },
            { "static", TokenType.STATIC },
            { "event", TokenType.EVENT },
            { "new", TokenType.NEW },
        };


        /// <summary>
        /// Scans the string and returns a list of tokens
        /// </summary>
        /// <param name="source">The string to lex</param>
        /// <param name="context">Source of string used in token locations</param>
        /// <returns>A list of <see cref="Token"/>(s)</returns>
        public List<Token> Lex(string source, string context)
        {
            _source = source;
            _context = context;
            _line = 1;
            _tokens = new List<Token>();

            while(Available())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, _line, _context));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            
            switch(c)
            {
                case '+': _tokens.Add(new Token(Match('+') ? TokenType.DOUBLE_PLUS : TokenType.PLUS, _line, _context)); break;
                case '-': _tokens.Add(new Token(Match('-') ? TokenType.DOUBLE_MINUS: TokenType.MINUS, _line, _context)); break;
                case '*': _tokens.Add(new Token(TokenType.STAR, _line, _context)); break;
                case '/': 
                    if(Match('/'))
                    {
                        while (Available())
                            if (Match('\n'))
                            {
                                _line++;
                                break;
                            }
                            else
                                _current++;
                    }
                    else
                    {
                        _tokens.Add(new Token(TokenType.SLASH, _line, _context));
                    }
                    break;

                case '(': _tokens.Add(new Token(TokenType.OPEN_PAREN, _line, _context)); break;
                case ')': _tokens.Add(new Token(TokenType.CLOSE_PAREN, _line, _context)); break;
                case '{': _tokens.Add(new Token(TokenType.OPEN_BRACE, _line, _context)); break;
                case '}': _tokens.Add(new Token(TokenType.CLOSE_BRACE, _line, _context)); break;
                case '[': _tokens.Add(new Token(TokenType.OPEN_SQUARE, _line, _context)); break;
                case ']': _tokens.Add(new Token(TokenType.CLOSE_SQUARE, _line, _context)); break;

                case ';': _tokens.Add(new Token(TokenType.SEMICOLON, _line, _context)); break;
                case ':': _tokens.Add(new Token(TokenType.COLON, _line, _context)); break;
                case ',': _tokens.Add(new Token(TokenType.COMMA, _line, _context)); break;
                case '.': _tokens.Add(new Token(TokenType.DOT, _line, _context)); break;

                case '=': _tokens.Add(new Token(Match('=') ? TokenType.DOUBLE_EQUALS : TokenType.EQUALS, _line, _context)); break;
                case '!': _tokens.Add(new Token(Match('=') ? TokenType.NOT_EQUALS : TokenType.BANG, _line, _context)); break;
                case '<': _tokens.Add(new Token(Match('=') ? TokenType.LESS_EQUALS: TokenType.LESS, _line, _context)); break;
                case '>': _tokens.Add(new Token(Match('=') ? TokenType.GREATER_EQUALS : TokenType.GREATER, _line, _context)); break;
                
                case '&': _tokens.Add(new Token(Match('&') ? TokenType.AND : TokenType.AMPERSAND, _line, _context)); break;
                case '|': _tokens.Add(new Token(Match('|') ? TokenType.OR : TokenType.PIPE, _line, _context)); break;

                case '\n':
                    _line++;
                    break;

                // Ignore
                case ' ':
                case '\t':
                case '\r':
                    break;

                case '"':
                    ScanString();
                    break;

                default:
                    if (char.IsLetter(c) || c == '_')
                        ScanIdentifier();
                    else if (char.IsDigit(c))
                        ScanNumber();
                    else
                        throw new LexerException(_context, _line, $"Unexpected character '{c}'");
                    break;
            }
        }

        private void ScanNumber()
        {
            while (char.IsDigit(Peek()))
                _current++;

            // Decimal number
            if (Match('.'))
            {
                while (char.IsDigit(Peek()))
                    _current++;
            }

            string num_str = _source.Substring(_start, _current - _start);
            double number = double.Parse(num_str);

            _tokens.Add(new Token(TokenType.NUMBER, number, _line, _context));
        }

        private void ScanIdentifier()
        {
            while(IsAlphaNum(Peek()))
                _current++;

            string identifier = _source.Substring(_start, _current - _start);

            // Reserved word or identifier
            TokenType type = s_reservedWords.ContainsKey(identifier) ? s_reservedWords[identifier] : TokenType.IDENTIFIER;
            _tokens.Add(new Token(type, identifier, _line, _context));
        }

        private void ScanString()
        {
            string str = string.Empty;

            while(Peek() != '"' && Peek() != '\0')
            {
                char c = Advance();

                // escape
                if(c == '\\')
                {
                    char escape = Advance();

                    switch(escape)
                    {
                        case '\\': str += '\\'; break;
                        case '"': str += '"'; break;
                        case 'n': str += '\n'; break;
                        case 't': str += '\t'; break;
                        case 'r': str += '\r'; break;
                        case 'b': str += '\b'; break;

                        // TODO: \xFF  hexadecimal ascii escape
                    }
                }
                else
                {
                    str += c;
                }
            }

            if(Peek() == '\0')
                throw new LexerException(_context, _line, $"Unclosed string");

            // Consume closing double quotes
            _current++;
            _tokens.Add(new Token(TokenType.STRING, str, _line, _context));
        }


        // Helper functions
        private bool IsAlphaNum(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private bool Match(char c)
        {
            if(Peek() == c)
            {
                _current++;
                return true;
            }

            return false;
        }

        private char Peek()
        {
            if (!Available()) return '\0';
            return _source[_current];
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private bool Available()
        {
            return _current < _source.Length;
        }
    }

    public class LexerException : Exception
    {
        public new string Source;
        public int Line;
        
        public LexerException(string source, int line, string message) : base(message)
        {
            Source = source;
            Line = line;
        }
    }
}
