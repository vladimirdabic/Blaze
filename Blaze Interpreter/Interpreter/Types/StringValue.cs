using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class StringValue : IValue, IValueBinOp
    {
        public string Value;

        public StringValue(string value) { Value = value; }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            if (other is StringValue otherStr) return Value == otherStr.Value;

            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj is  StringValue otherStr)
                return Value == otherStr.Value;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetName()
        {
            return "string";
        }

        public string AsString()
        {
            return $"{Value}";
        }

        public IValue Add(IValue other)
        {
            if (other is NumberValue otherNum)
                // Add string and number
                return new StringValue(Value + otherNum.Value);

            if (other is StringValue otherStr)
                // Add strings
                return new StringValue(Value + otherStr.Value);

            return null;
        }

        public IValue Subtract(IValue other)
        {
            return null;
        }

        public IValue Multiply(IValue other)
        {
            if (other is NumberValue otherNum)
                return new StringValue(new StringBuilder(Value.Length * (int)otherNum.Value).Insert(0, Value, (int)otherNum.Value).ToString());

            return null;
        }

        public IValue Divide(IValue other)
        {
            return null;
        }

        public IValue LessThan(IValue other)
        {
            return null;
        }

        public IValue LessThanEquals(IValue other)
        {
            return null;
        }

        public bool AsBoolean()
        {
            return true;
        }
    }
}