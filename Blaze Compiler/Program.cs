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
using Blaze_Compiler.ArgParse;

namespace Blaze_Compiler
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var argIt = new ArgumentIterator(args);

            string source_file = null;
            bool debug = false;

            while (argIt.Available())
            {
                var arg = argIt.Next();
                switch (arg)
                {
                    case "-s":
                        source_file = argIt.Next();

                        if (source_file is null)
                        {
                            Console.WriteLine($"Expected blaze source file name after '-s'");
                            return;
                        }

                        break;

                    case "-d":
                        debug = true;
                        break;

                    default:
                        Console.WriteLine($"Unknown argument '{arg}'");
                        return;
                }
            }

            if (source_file is null)
            {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} [options]");
                Console.WriteLine($"Options:");
                Console.WriteLine($"    -s [file]       Blaze source file to compile");
                Console.WriteLine($"    -d              Compiles as a debug module");
                return;
            }

            Lexer lexer = new Lexer();
            Parser parser = new Parser();
            Generator generator = new Generator();

            if(!File.Exists(source_file))
            {
                Console.WriteLine("Source file not found");
                return;
            }

            string program = File.ReadAllText(source_file);

            try
            {

                var tokens = lexer.Lex(program, source_file);
                var tree = parser.Parse(tokens);
                Module module = generator.Generate(tree, source_file, debug);

                var module_file_name = Path.GetFileNameWithoutExtension(source_file) + ".blzm";

                using (FileStream stream = File.Open(module_file_name, FileMode.Create, FileAccess.Write))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    module.ToBinary(writer);
                }

                Console.WriteLine($"Compiled to '{module_file_name}'");
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
        }
    }
}
