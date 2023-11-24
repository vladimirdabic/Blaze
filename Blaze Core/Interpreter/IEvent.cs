using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter
{
    public interface IEvent
    {
        void Attach(IValueCallable callback);
        void Raise(List<IValue> args);
    }
}
