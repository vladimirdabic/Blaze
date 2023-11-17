using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Class
    {
        public int Index;
        public Module ParentModule;

        public Constant Name;
        // public Constant ParentName;
        public List<Constant> Members;
        public Function Constructor;

        public Class(Module parentModule, Constant name)
        {
            ParentModule = parentModule;
            Name = name;
            Members = new List<Constant>();
            Constructor = null;
            // ParentName = null;
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write((ushort)(Name is null ? 0 : Name.Index + 1));
            bw.Write((byte)Members.Count);
            
            foreach (var member in Members)
            {
                bw.Write((ushort)member.Index);
            }

            bw.Write((ushort)Constructor.Index);
        }

        public void FromBinary(BinaryReader br)
        {
            ushort name_idx = br.ReadUInt16();
            Name = name_idx == 0 ? null : ParentModule.Constants[name_idx - 1];

            Members = new List<Constant>();
            int member_count = br.ReadByte();
            
            for(int i = 0; i < member_count; i++)
            {
                Members.Add(ParentModule.Constants[br.ReadUInt16()]);
            }

            ushort ctor_idx = br.ReadUInt16();
            Constructor = ParentModule.Functions[ctor_idx];
        }
    }
}
