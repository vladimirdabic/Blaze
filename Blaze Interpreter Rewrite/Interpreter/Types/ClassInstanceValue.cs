using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;

namespace VD.Blaze.Interpreter.Types
{
    public class ClassInstanceValue : IValue, IValueProperties
    {
        public ClassValue Type;
        public ClassEnv Properties;

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return $"<class instance {Type.Name}>";
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public string GetName()
        {
            return Type.Name;
        }

        public IValue GetProperty(string name)
        {
            if(Properties.Members.ContainsKey(name))
                return Properties.Members[name].GetValue();

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            if(Properties.Members.ContainsKey(name))
            {
                Properties.Members[name].SetValue(value);
                return;
            }

            throw new PropertyNotFound();
        }
    }
}
