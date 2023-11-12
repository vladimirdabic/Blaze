using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter.Environment
{
    public abstract class BaseEnv
    {
        public BaseEnv Parent;

        /// <summary>
        /// Defines a variable in this environment
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Value (Optional)</param>
        /// <returns>The variable</returns>
        public abstract IVariable DefineVariable(string name, IValue value = null);

        /// <summary>
        /// Gets a variable from this environment by its name
        /// </summary>
        /// <param name="name">Variable name</param>
        public abstract IVariable GetVariable(string name);

        /// <summary>
        /// Gets a variable from this environment by its ID
        /// </summary>
        /// <param name="name">Variable name</param>
        public abstract IVariable GetVariable(int index);


        public BaseEnv(BaseEnv parent)
        {
            Parent = parent;
        }

        public BaseEnv GetParent(int uplevel = 0)
        {
            return uplevel == 0 ? this : Parent.GetParent(uplevel - 1);
        }

        public void SetParent(BaseEnv parent)
        {
            Parent = parent;
        }
    }

    public interface IVariable
    {
        void SetValue(IValue value);
        IValue GetValue();
    }
}
