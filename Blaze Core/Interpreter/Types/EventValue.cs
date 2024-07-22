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
        public VM ParentVM;

        public EventValue(VM vm)
        {
            Callbacks = new List<IValueCallable>();
            ParentVM = vm;
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
                Callbacks[i].Call(vm, args);

                // Only leave the returned value from the callback
                vm._inConstructor = i != Callbacks.Count - 1;
            }

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
            if (Callbacks.Count == 0) return;
            Call(ParentVM, args);

            // If the VM is not running, run it
            if (ParentVM.Done)
                ParentVM.Execute();
        }
    }
}
