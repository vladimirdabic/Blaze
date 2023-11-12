using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Generator.Environment
{
    public abstract class BaseEnv
    {
        public BaseEnv Parent;

        /// <summary>
        /// Defines a variable in this environment
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>The variable</returns>
        public abstract void DefineVariable(string name, IVariable variable = null);

        /// <summary>
        /// Gets a variable from this environment by its name
        /// </summary>
        /// <param name="name">Variable name</param>
        public abstract (IVariable, int) GetVariable(string name, int level = 0);

        public abstract void PushFrame();

        public abstract void PopFrame();


        public BaseEnv(BaseEnv parent)
        {
            Parent = parent;
        }

        public BaseEnv GetParent(int uplevel = 0)
        {
            return uplevel == 0 ? this : Parent.GetParent(uplevel - 1);
        }

        public interface IVariable 
        {
            string GetName();
        }
    }
}
