using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter.Types
{
    public interface IValueNew
    {
        void New(VM vm, IValue instance, List<IValue> args);
    }
}
