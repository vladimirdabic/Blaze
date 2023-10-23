using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Lexer;
using VD.Blaze.Parser;

namespace Blaze_Compiler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer();
            Parser parser = new Parser();

            string program = File.ReadAllText("test.blz");

            try
            {

                var tokens = lexer.Lex(program, "test.blz");
                var tree = parser.Parse(tokens);

                foreach (var t in ((Statement.Definitions)tree).Statements)
                {
                    Console.WriteLine(t);
                }
            } 
            catch (LexerException e)
            {
                Console.WriteLine($"[{e.Source}:{e.Line}] {e.Message}");
            }
            catch (ParserException e)
            {
                Console.WriteLine($"[{e.Source}:{e.Line}] {e.Message}");
            }

            Console.ReadKey();
        }
    }
}
