﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
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

        public void Emit(Opcode instruction, uint argument)
        {
            Instructions.Add(new Instruction(instruction, argument));
        }

        public void Emit(Opcode instruction, int argument)
        {
            Emit(instruction, (uint)argument);
        }

        public void Emit(Opcode instruction, Constant constant)
        {
            Emit(instruction, constant.Index);
        }

        public void Emit(Opcode instruction, Variable mod_var)
        {
            Emit(instruction, mod_var.Name.Index);
        }

        public void Emit(Opcode instruction, Function func)
        {
            Emit(instruction, func.Index);
        }

        public void Emit(Opcode instruction, LocalVariable local)
        {
            Emit(instruction, local.Index);
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

            if (Instructions.Count == 0 || (Instructions[Instructions.Count - 1].Opcode != Opcode.RET))
            {
                Instructions.Add(new Instruction(Opcode.LDNULL, 0));
                Instructions.Add(new Instruction(Opcode.RET, 0));
            }

            bw.Write((ushort)Instructions.Count);

            foreach(var inst in Instructions)
            {
                // Write out EXTENDED_ARG instructions
                uint mask = 0xFF000000;
                bool found = false;

                for(int i = 0; i < 3; i++)
                {
                    mask >>= 8;
                    byte ext_arg = (byte)((inst.Argument & mask) >> ((3 - i) * 8));

                    if(ext_arg != 0)
                        found = true;

                    if (found)
                    {
                        bw.Write((byte)Opcode.EXTENDED_ARG);
                        bw.Write(ext_arg);
                    }
                }


                bw.Write((byte)inst.Opcode);
                bw.Write((byte)(inst.Argument & 0xFF));
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
                Opcode opcode = (Opcode)br.ReadByte();
                uint arg = br.ReadByte();

                while (opcode == Opcode.EXTENDED_ARG)
                {
                    i++;
                    opcode = (Opcode)br.ReadByte();
                    arg = (arg << 8) | br.ReadByte();
                }

                Instructions.Add(new Instruction(opcode, arg));
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

        EQ, LT, LTE, NOT, JMP, JMPB, JMPA, JMPT, JMPF,
        OR, AND,

        DUP, VARARGS,

        LDLIST, LDOBJ, LDINDEX, STINDEX, LDPROP, STPROP,
        LDEVENT, ITER,
    }

    public class Instruction
    {
        public Opcode Opcode;
        public uint Argument;

        public Instruction(Opcode opcode, uint argument)
        {
            Opcode = opcode;
            Argument = argument;
        }
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