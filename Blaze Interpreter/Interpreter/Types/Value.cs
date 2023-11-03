using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter
{
    public interface IValue
    {
        /// <summary>
        /// Gets the Blaze type name of the value
        /// </summary>
        string GetName();

        /// <summary>
        /// Gets the string representation of the value
        /// </summary>
        string ToString();

        /// <summary>
        /// Compares the value to another value
        /// </summary>
        /// <param name="other">The other value</param>
        bool Equals(IValue other);

        /// <summary>
        /// Copies the value
        /// </summary>
        IValue Copy();
    }
}
