using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Constant
    {
        public ConstantType Type;
        public int Index;

        public Constant(ConstantType type)
        {
            Type = type;
            Index = 0; // maybe -1 idk yet
        }

        public virtual void WriteData(BinaryWriter writer) { }
        public virtual void ReadData(BinaryReader reader) { }
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            WriteData(writer);
        }

        public void FromBinary(BinaryReader reader)
        {
            Type = (ConstantType)reader.ReadByte();
            ReadData(reader);
        }


        // Implemented constants
        public class Number : Constant
        {
            public double Value;
            public Number(double num) : base(ConstantType.NUMBER)
            {
                Value = num;
            }

            public override void WriteData(BinaryWriter writer)
            {
                writer.Write(Value);
            }

            public override void ReadData(BinaryReader reader)
            {
                Value = reader.ReadDouble();
            }
        }

        public class String : Constant
        {
            public string Value;
            public String(string value) : base(ConstantType.STRING)
            {
                Value = value;
            }

            public override void WriteData(BinaryWriter writer)
            {
                writer.Write((ushort)Value.Length);
                writer.Write(Value.ToCharArray());
            }

            public override void ReadData(BinaryReader reader)
            {
                ushort size = reader.ReadUInt16();
                char[] str = new char[size];

                for(int i = 0; i < size; i++)
                {
                    str[i] = reader.ReadChar();
                }

                Value = new string(str);
            }
        }
    }

    public enum ConstantType
    {
        NUMBER, STRING
    }
}
