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
            string module_name = null;
            bool debug = false;
            bool dump = false;

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

                    case "-m":
                        module_name = argIt.Next();

                        if(module_name is null)
                        {
                            Console.WriteLine($"Expected module name after '-m'");
                            return;
                        }

                        break;

                    case "-c":
                        dump = true;
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
                Console.WriteLine($"    -m [name]       Use a custom module name");
                Console.WriteLine($"    -d              Compiles as a debug module");
                Console.WriteLine($"    -c              Print the contents of a module file (source file must be blzm)");
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

            if (dump)
            {
                try
                {
                    // Load module
                    Module module = new Module();

                    MemoryStream stream = new MemoryStream(File.ReadAllBytes(source_file));
                    BinaryReader reader = new BinaryReader(stream);

                    module.FromBinary(reader);
                    module.PrintToConsole();

                    return;
                }
                catch (FileLoadException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            string program = File.ReadAllText(source_file);

            try
            {

                var tokens = lexer.Lex(program, source_file);
                var tree = parser.Parse(tokens);
                Module module = generator.Generate(tree, source_file, debug, module_name);

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
