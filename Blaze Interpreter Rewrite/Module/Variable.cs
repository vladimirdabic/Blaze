using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Variable
    {
        public Module ParentModule;
        public VariableType Type;
        public Constant Name;

        public Variable(Module module, VariableType type, Constant name)
        {
            Type = type;
            Name = name;
            ParentModule = module;
        }

        public Variable(Module module) : this(module, VariableType.PRIVATE, null)
        {
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write((byte)Type);
            // Cannot really be null though
            bw.Write((ushort)(Name is null ? 0 : Name.Index + 1));
        }

        public void FromBinary(BinaryReader br)
        {
            Type = (VariableType)br.ReadByte();

            ushort name_idx = br.ReadUInt16();
            Name = name_idx == 0 ? null : ParentModule.Constants[name_idx - 1];
        }
    }

    public enum VariableType
    {
        PRIVATE, PUBLIC, EXTERNAL
    }
}
