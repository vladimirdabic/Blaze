using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public record struct Instruction(Opcode Id, byte Argument);

    public class Function
    {
        public int Index;
        public Module ParentModule;
        public Constant Name;
        public int NumOfArgs;
        public bool Varargs;
        public byte NumOfLocals { get; private set; }
        public List<Instruction> Instructions;

        public Function(Module module, Constant name, int numOfArgs, bool varargs)
        {
            Name = name;
            NumOfArgs = numOfArgs;
            Instructions = new List<Instruction>();
            Varargs = varargs;
            ParentModule = module;
        }

        public Function(Module module, Constant name, int numOfArgs) : this(module, name, numOfArgs, false) { }
        public Function(Module module) : this(module, null, 0, false) { }

        public void Emit(Opcode instruction, byte argument)
        {
            Instructions.Add(new Instruction(instruction, argument));
        }

        public void Emit(Opcode instruction, Constant constant)
        {
            Instructions.Add(new Instruction(instruction, (byte)constant.Index));
        }

        public void Emit(Opcode instruction, Variable mod_var)
        {
            Instructions.Add(new Instruction(instruction, (byte)mod_var.Name.Index));
        }

        public void Emit(Opcode instruction, Function func)
        {
            Instructions.Add(new Instruction(instruction, (byte)func.Index));
        }

        public void Emit(Opcode instruction, LocalVariable local)
        {
            Instructions.Add(new Instruction(instruction, local.Index));
        }

        public void Emit(Opcode instruction)
        {
            Emit(instruction, 0);
        }

        /*
        public byte DeclareLocal()
        {
            return NumOfLocals++;
        }
        */

        public LocalVariable DeclareLocal()
        {
            return new LocalVariable(this, NumOfLocals++);
        }

        public void ToBinary(BinaryWriter bw)
        {
            // Index 0 means function has no name
            bw.Write((ushort)(Name is null ? 0 : Name.Index + 1));
            bw.Write((byte)NumOfArgs);
            bw.Write(Varargs);
            bw.Write(NumOfLocals);

            if (Instructions.Count == 0 || (Instructions[Instructions.Count - 1].Id != Opcode.RET))
            {
                Instructions.Add(new Instruction(Opcode.LDNULL, 0));
                Instructions.Add(new Instruction(Opcode.RET, 0));
            }

            bw.Write((ushort)Instructions.Count);

            foreach(var inst in Instructions)
            {
                // each instruction is a 16 bit integer (2 bytes)
                // first byte is the instruction, second is the argument
                // 0x03 0x01 (LDARG, 1) = 0x0301
                // ushort instruction = (ushort)((((byte)inst.inst) << 8) | inst.arg);
                // bw.Write(instruction);

                // Big endian order, binarywriter only writes in little endian
                bw.Write((byte)inst.Id);
                bw.Write(inst.Argument);
            }

        }

        public void FromBinary(BinaryReader br)
        {
            ushort name_idx = br.ReadUInt16();
            Name = name_idx == 0 ? null : ParentModule.Constants[name_idx - 1];

            NumOfArgs = br.ReadByte();
            Varargs = br.ReadBoolean();
            NumOfLocals = br.ReadByte();

            int inst_count = br.ReadUInt16();
            for(int i = 0; i < inst_count; i++)
            {
                // ushort inst = br.ReadUInt16();
                // byte arg = (byte)(inst & 0xff);
                // byte inst_id = (byte)(inst >> 8);
                Opcode inst_id = (Opcode)br.ReadByte();
                byte arg = br.ReadByte();
                Instructions.Add(new Instruction(inst_id, arg));
            }
        }
    }

    public enum Opcode
    {
        NOP, POP,
        
        EXTENDED_ARG,

        LDNULL, LDARG, LDCONST, LDLOCAL, LDVAR, LDFUNC, LDCLASS, LDBOOL,
        STLOCAL, STVAR, STARG,

        CALL, RET,
        
        // INTDIV pushes the result and the remainder on the stack [..., RES, REMAINDER]
        ADD, SUB, MUL, DIV, INTDIV,

        THROW, CATCH, TRY_END,

        EQ, LT, LTE, NOT, JMP, JMPA, JMPT, JMPF,
        OR, AND,

        DUP, VARARGS,

        LDARR, LDOBJ, LDINDEX, STINDEX,
    }

    public class LocalVariable
    {
        public Function Owner;
        public byte Index;

        public LocalVariable(Function owner, byte index)
        {
            Owner = owner;
            Index = index;
        }
    }
}
