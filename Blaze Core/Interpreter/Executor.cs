using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter
{
    public class Executor
    {
        public List<VM> VMs { get; set; }
        public bool Done { get; set; }
        public event EventHandler<VM> VMFinished;

        public Executor()
        {
            VMs = new List<VM>();
            Done = false;
        }

        public void Execute()
        {
            Done = false;

            foreach (VM vm in VMs)
                vm.Setup();

            while (VMs.Count > 0)
            {
                for (int i = 0; i < VMs.Count; ++i)
                {
                    VM vm = VMs[i];

                    vm.Step();
                    if (vm.Done)
                    {
                        VMFinished?.Invoke(this, vm);
                        VMs.Remove(vm);
                    }
                }
            }

            Done = true;
        }

        public void LoadFunction(VM vm, FunctionValue function, List<IValue> args)
        {
            VMs.Add(vm);
            function.Call(vm, args);
        }
    }
}
