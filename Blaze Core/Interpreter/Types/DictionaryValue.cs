using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class DictionaryValue : IValue, IValueIndexable, IValueProperties, IValueIterable
    {
        public Dictionary<IValue, IValue> Entries;

        public DictionaryValue()
        {
            Entries = new Dictionary<IValue, IValue>();
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return "<dict>";
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public IValue GetAtIndex(IValue index)
        {
            return Entries.ContainsKey(index) ? Entries[index] : VM.NullInstance;
        }

        public IteratorValue GetIterator()
        {
            return new DictIterator(this);
        }

        public string GetName()
        {
            return "dict";
        }

        public IValue GetProperty(string name)
        {
            throw new PropertyNotFound();
        }

        public void SetAtIndex(IValue index, IValue value)
        {
            Entries[index] = value;
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }
    }
}
