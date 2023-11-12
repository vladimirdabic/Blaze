using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public interface IValueCallable
    {
        void Call(Interpreter interpreter, List<IValue> args);
    }
}
