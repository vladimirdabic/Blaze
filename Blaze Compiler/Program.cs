using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Generator;
using VD.Blaze.Lexer;
using VD.Blaze.Parser;
using VD.Blaze.Module;

namespace Blaze_Compiler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer();
            Parser parser = new Parser();
            Generator generator = new Generator();

            string program = File.ReadAllText("test.blz");

            try
            {

                var tokens = lexer.Lex(program, "test.blz");
                var tree = parser.Parse(tokens);
                Module module = generator.Generate(tree, "test.blz");

                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                module.ToBinary(writer);

                File.WriteAllBytes("test.blzm", stream.ToArray());

                Console.WriteLine("Compiled");
            } 
            catch (LexerException e)
            {
                Console.WriteLine($"[{e.Source}:{e.Line}] {e.Message}");
            }
            catch (ParserException e)
            {
                Console.WriteLine($"[{e.Source}:{e.Line}] {e.Message}");
            }
            catch (GeneratorException e)
            {
                Console.WriteLine($"[{e.Source}:{e.Line}] {e.Message}");
            }

            Console.ReadKey();
        }
    }
}
