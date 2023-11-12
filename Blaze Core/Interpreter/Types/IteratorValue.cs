using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter.Types
{
    public class IteratorValue : IValue, IValueProperties
    {
        public virtual IValue Next() { throw new NotImplementedException(); }
        public virtual bool Available() { throw new NotImplementedException(); }

        public string GetName()
        {
            return "iterator";
        }

        public string AsString()
        {
            return "<iterator>";
        }

        public bool AsBoolean()
        {
            return true;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public IValue Copy()
        {
            return this;
        }

        public IValue GetProperty(string name)
        {
            switch (name)
            {
                case "next":
                    return Next();

                case "available":
                    return new BooleanValue(Available());

                default:
                    break;
            }

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }
    }

    public class ListIterator : IteratorValue
    {
        public ListValue Value;
        private int _index;

        public ListIterator(ListValue value)
        {
            Value = value;
            _index = 0;
        }

        public override IValue Next()
        {
            if (!Available()) return Interpreter.NullInstance;
            return Value.Values[_index++];
        }

        public override bool Available()
        {
            return _index < Value.Values.Count;
        }
    }

    public class DictIterator : IteratorValue
    {
        public DictionaryValue Value;
        private Dictionary<IValue, IValue>.Enumerator _enumerator;
        private bool _hasNext;

        public DictIterator(DictionaryValue value)
        {
            Value = value;
            _enumerator = Value.Entries.GetEnumerator();
            _hasNext = _enumerator.MoveNext();
        }

        public override IValue Next()
        {
            if(_hasNext)
            {
                var pair = _enumerator.Current;
                _hasNext = _enumerator.MoveNext();

                var listPair = new ListValue();
                listPair.Values.Add(pair.Key);
                listPair.Values.Add(pair.Value);

                return listPair;
            }

            return Interpreter.NullInstance;
        }

        public override bool Available()
        {
            return _hasNext;
        }
    }

    public class StringIterator : IteratorValue
    {
        public string Value;
        private int _index;

        public StringIterator(string value)
        {
            Value = value;
            _index = 0;
        }

        public override IValue Next()
        {
            if (!Available()) return Interpreter.NullInstance;
            return new StringValue(Value[_index++].ToString());
        }

        public override bool Available()
        {
            return _index < Value.Length;
        }
    }
}
