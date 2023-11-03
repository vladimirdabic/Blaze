using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter
{
    public class BooleanValue : IValue
    {
        public bool Value;

        public BooleanValue(bool value) {  Value = value; }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            if(other is BooleanValue otherBool)
                return Value == otherBool.Value;

            return false;
        }

        public string GetName()
        {
            return "boolean";
        }
    }
}
