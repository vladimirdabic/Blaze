using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter
{
    public interface IValueBinOp
    {
        IValue Add(IValue other);
        IValue Subtract(IValue other);
        IValue Multiply(IValue other);
        IValue Divide(IValue other);
        IValue LessThan(IValue other);
        IValue LessThanEquals(IValue other);
    }
}
