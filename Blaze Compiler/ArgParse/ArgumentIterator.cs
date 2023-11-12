using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze_Compiler.ArgParse
{
    internal class ArgumentIterator
    {
        private string[] _args;
        private int current = 0;

        public ArgumentIterator(string[] args)
        {
            _args = args;
        }

        public string Next()
        {
            if (!Available()) return null;

            return _args[current++];
        }

        public bool Available()
        {
            return current < _args.Length;
        }
    }
}
