using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter
{
    public class NumberValue : IValue, IValueBinOp
    {
        public double Value;

        public NumberValue(double value) { Value = value; }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            if(other == this) return true;
            if (other is NumberValue otherNum) return Value == otherNum.Value;

            return false;
        }

        public string GetName()
        {
            return "number";
        }

        public IValue Add(IValue other)
        {
            if(other is NumberValue otherNum)
                return new NumberValue(Value + otherNum.Value);

            return null;
        }

        public IValue Subtract(IValue other)
        {
            if (other is NumberValue otherNum)
                return new NumberValue(Value - otherNum.Value);

            return null;
        }

        public IValue Multiply(IValue other)
        {
            if (other is NumberValue otherNum)
                return new NumberValue(Value * otherNum.Value);

            return null;
        }

        public IValue Divide(IValue other)
        {
            if (other is NumberValue otherNum)
                return new NumberValue(Value / otherNum.Value);

            return null;
        }

        public IValue LessThan(IValue other)
        {
            if (other is NumberValue otherNum)
                return new BooleanValue(Value < otherNum.Value);

            return null;
        }

        public IValue LessThanEquals(IValue other)
        {
            if (other is NumberValue otherNum)
                return new BooleanValue(Value <= otherNum.Value);
            
            return null;
        }

    }
}
