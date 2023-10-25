using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Module
    {
        public List<Constant> Constants;
        public List<Function> Functions;


        public Function CreateFunction(string name, int num_args)
        {
            Constant name_const = AddConstant(new Constant.String(name));
            Function func = new Function(name_const, num_args);
            Functions.Add(func);
            return func;
        }


        public Constant AddConstant(Constant constant)
        {
            constant.Index = Constants.Count;
            Constants.Add(constant);
            return constant;
        }
    }
}
