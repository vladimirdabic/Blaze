using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;

namespace VD.Blaze.Lib
{
    public static class LibraryLoader
    {
        public static void Load(Assembly asm, ModuleEnv parent)
        {
            foreach (var type in asm.GetTypes())
            {
                var attr = type.GetCustomAttribute<BlazeModuleAttribute>();
                if (attr != null && typeof(IBlazeModuleFactory).IsAssignableFrom(type))
                {
                    var factory = (IBlazeModuleFactory)Activator.CreateInstance(type);
                    var module = factory.CreateModule();
                    module.SetParent(parent);
                }
            }
        }

        public static List<ModuleEnv> Load(Assembly asm)
        {
            List<ModuleEnv> modules = new List<ModuleEnv>();

            foreach (var type in asm.GetTypes())
            {
                var attr = type.GetCustomAttribute<BlazeModuleAttribute>();
                if (attr != null && typeof(IBlazeModuleFactory).IsAssignableFrom(type))
                {
                    var factory = (IBlazeModuleFactory)Activator.CreateInstance(type);
                    var module = factory.CreateModule();
                    modules.Add(module);
                }
            }

            return modules;
        }
    }
}
