using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;

namespace VD.Blaze.Lib
{
    public interface IBlazeModuleFactory
    {
        ModuleEnv CreateModule();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BlazeModuleAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }

        public BlazeModuleAttribute(string name, string version = "1.0")
        {
            Name = name;
            Version = version;
        }
    }
}
