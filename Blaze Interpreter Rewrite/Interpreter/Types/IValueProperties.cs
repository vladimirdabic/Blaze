using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public interface IValueProperties
    {
        IValue GetProperty(string name);
        void SetProperty(string name, IValue value);
    }

    public class PropertyNotFound : Exception { }
}
