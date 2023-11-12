using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public interface IValueIndexable
    {
        IValue GetAtIndex(IValue index);
        void SetAtIndex(IValue index, IValue value);
    }

    public class IndexOutOfBounds : Exception { }
    public class IndexNotFound : Exception { }
}
