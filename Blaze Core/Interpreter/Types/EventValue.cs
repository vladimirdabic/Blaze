using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class EventValue : IValue, IValueCallable, IEvent
    {
        public List<IValueCallable> Callbacks;
        public VM VM;

        public EventValue(VM vm)
        {
            Callbacks = new List<IValueCallable>();
            VM = vm;
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return "<event>";
        }

        public void Attach(IValueCallable callback)
        {
            Callbacks.Add(callback);
        }

        public void Call(VM vm, List<IValue> args)
        {
            for(int i = Callbacks.Count - 1; i >= 0; i--)
            {
                Callbacks[i].Call(VM, args);
            }

            // If the VM is not running, run it
            if (VM.Done)
                VM.Execute();
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
            return "event";
        }

        public void Raise(List<IValue> args)
        {
            Call(VM, args);
        }
    }
}
