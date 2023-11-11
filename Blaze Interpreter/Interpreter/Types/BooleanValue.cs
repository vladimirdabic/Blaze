using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class BooleanValue : IValue
    {
        public bool Value;

        public BooleanValue(bool value) {  Value = value; }

        public bool AsBoolean()
        {
            return Value;
        }

        public string AsString()
        {
            return $"{Value}";
        }

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

        public override bool Equals(object obj)
        {
            if(obj is BooleanValue otherBool)
                return Value == otherBool.Value;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetName()
        {
            return "boolean";
        }
    }
}
