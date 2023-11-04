using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class NullValue : IValue
    {
        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other is NullValue;
        }

        public string GetName()
        {
            return "null";
        }

        public string AsString()
        {
            return $"null";
        }
    }
}
