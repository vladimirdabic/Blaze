﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class NullValue : IValue
    {
        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other is NullValue;
        }

        public string GetName()
        {
            return "null";
        }

        public string AsString()
        {
            return "null";
        }

        public bool AsBoolean()
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is NullValue;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
