﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Module
{
    public class Module
    {
        public readonly List<Constant> Constants;
        public readonly List<Variable> Variables;
        public readonly List<Function> Functions;
        public readonly List<Class> Classes;
        public string Name;
        public bool Debug;
        public (int major, int minor) Version;

        private Function _staticFunction;
        private readonly Dictionary<string, Constant> _stringConstantMap;
        private readonly Dictionary<double, Constant> _numberConstantMap;

        // Is used as the line number for instructions if Debug is enabled
        public int CurrentLine;

        public Module()
        {
            Constants = new List<Constant>();
            Variables = new List<Variable>();
            Functions = new List<Function>();
            Classes = new List<Class>();

            _stringConstantMap = new Dictionary<string, Constant>();
            _numberConstantMap = new Dictionary<double, Constant>();
            _staticFunction = CreateAnonymousFunction(0);

            CurrentLine = 0;
        }

        public Function CreateFunction(string name, int num_args, VariableType visibility = VariableType.PRIVATE)
        {
            Constant name_const = AddConstant(new Constant.String(name));
            Function func = new Function(this, name_const, num_args);
            func.Index = Functions.Count;
            Functions.Add(func);

            Variable func_var = new Variable(this, visibility, name_const);
            Variables.Add(func_var);

            // Initialize in the static function so other modules can access it
            _staticFunction.Emit(Opcode.LDFUNC, func);
            _staticFunction.Emit(Opcode.STVAR, func_var);

            return func;
        }

        public Function CreateAnonymousFunction(int num_args)
        {
            Function func = new Function(this, null, num_args);
            func.Index = Functions.Count;
            Functions.Add(func);
            return func;
        }

        public Class CreateClass(string name, VariableType visibility = VariableType.PRIVATE)
        {
            Constant name_const = AddConstant(new Constant.String(name));
            Class cls = new Class(this, name_const);
            cls.Index = Classes.Count;
            Classes.Add(cls);

            Variable cls_var = new Variable(this, visibility, name_const);
            Variables.Add(cls_var);

            // Initialize in the static function so other modules can access it
            _staticFunction.Emit(Opcode.LDCLASS, cls);
            _staticFunction.Emit(Opcode.STVAR, cls_var);

            return cls;
        }

        public Class CreateAnonymousClass()
        {
            Class cls = new Class(this, null);
            cls.Index = Classes.Count;
            Classes.Add(cls);
            return cls;
        }

        public Variable DefineVariable(string name, VariableType type)
        {
            Constant name_const = AddConstant(new Constant.String(name));
            Variable mod_var = new Variable(this, type, name_const);
            Variables.Add(mod_var);
            return mod_var;
        }


        public Constant AddConstant(Constant constant)
        {
            if(constant is Constant.String str_const)
            {
                if(_stringConstantMap.ContainsKey(str_const.Value))
                    return _stringConstantMap[str_const.Value];

                _stringConstantMap[str_const.Value] = constant;
            }

            if (constant is Constant.Number num_const)
            {
                if (_numberConstantMap.ContainsKey(num_const.Value))
                    return _numberConstantMap[num_const.Value];

                _numberConstantMap[num_const.Value] = constant;
            }

            constant.Index = Constants.Count;
            Constants.Add(constant);
            return constant;
        }

        public Function GetStaticFunction()
        {
            return _staticFunction;
        }

        public void FromBinary(BinaryReader br)
        {
            // Clear default static function
            Functions.Clear();

            uint id = br.ReadUInt32();

            if (id != 0x6D7A6C62)
                throw new FileLoadException("File is not a Blaze module file");

            byte major = br.ReadByte();
            byte minor = br.ReadByte();
            Version = (major, minor);

            ushort len = br.ReadUInt16();
            Name = new string(br.ReadChars(len));

            Debug = br.ReadBoolean();

            ushort const_count = br.ReadUInt16();
            for(int i = 0;  i < const_count; i++)
            {
                ConstantType type = (ConstantType)br.ReadByte();
                Constant constant;

                switch (type)
                {
                    case ConstantType.NUMBER:
                        constant = new Constant.Number(0);
                        break;
                    case ConstantType.STRING:
                        constant = new Constant.String(string.Empty);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                constant.Index = i;
                constant.FromBinary(br);
                Constants.Add(constant);
            }

            ushort var_count = br.ReadUInt16();
            for (int i = 0; i < var_count; i++)
            {
                Variable mod_var = new Variable(this);
                mod_var.FromBinary(br);
                Variables.Add(mod_var);
            }

            ushort func_count = br.ReadUInt16();
            for(int i = 0; i < func_count; i++)
            {
                Function func = new Function(this);
                func.Index = i;
                func.FromBinary(br);
                Functions.Add(func);
            }

            ushort class_count = br.ReadUInt16();
            for (int i = 0; i < class_count; i++)
            {
                Class cls = new Class(this, null);
                cls.Index = i;
                cls.FromBinary(br);
                Classes.Add(cls);
            }

            _staticFunction = Functions[0];
        }

        public void ToBinary(BinaryWriter bw)
        {
            // 0x626C7A6D ("blzm")
            // Since BinaryWriter writes in little endian, the number must be backwards
            // 0x6D7A6C62
            bw.Write((uint)0x6D7A6C62);
            // 01 00  (major, minor) (BinaryWriter is little endian so 0x0001)
            bw.Write((ushort)0x0001);

            bw.Write((ushort)Name.Length);
            bw.Write(Name.ToCharArray());

            bw.Write(Debug);

            // Constants
            bw.Write((ushort)Constants.Count);
            foreach(Constant constant in Constants)
                constant.ToBinary(bw);

            // Variables
            bw.Write((ushort)Variables.Count);
            foreach (Variable mod_var in Variables)
                mod_var.ToBinary(bw);

            // Functions
            bw.Write((ushort)Functions.Count);
            foreach(Function function in Functions)
                function.ToBinary(bw);

            // Classes
            bw.Write((ushort)Classes.Count);
            foreach (Class cls in Classes)
                cls.ToBinary(bw);
        }

        public void PrintToConsole()
        {
            Console.WriteLine("== INFORMATION ==");
            Console.WriteLine($"Version {Version.major}.{Version.minor}");
            Console.WriteLine($"Name '{Name}'");
            Console.WriteLine($"Debug? {Debug}");

            Console.WriteLine("\n== CONSTANTS ==");
            foreach (var constant in Constants)
            {
                switch (constant.Type)
                {
                    case ConstantType.NUMBER:
                        Console.WriteLine($"{constant.Index}: Number({((Constant.Number)constant).Value})");
                        break;

                    case ConstantType.STRING:
                        Console.WriteLine($"{constant.Index}: String({((Constant.String)constant).Value})");
                        break;
                }
            }

            Console.WriteLine("\n== VARIABLES ==");
            for(int i = 0; i < Variables.Count; ++i)
            {
                var mod_var = Variables[i];
                Console.WriteLine($"{i}: {((Constant.String)mod_var.Name).Value} ({mod_var.Type})");
            }

            Console.WriteLine("\n== FUNCTIONS ==");
            foreach (var func in Functions)
            {
                string name = func.Name is null ? "<anonymous>" : ((Constant.String)func.Name).Value;

                Console.WriteLine($"{name} (# args: {func.NumOfArgs}, # locals: {func.NumOfLocals}, Varargs: {func.Varargs}, # instructions: {func.Instructions.Count})");

                int line = 0;
                foreach (var inst in func.Instructions)
                {
                    if(inst.Line != line)
                    {
                        line = (int)inst.Line;
                        Console.WriteLine($"{inst.Line}\t{inst.Opcode} {inst.Argument}");
                    }
                    else
                    {
                        Console.WriteLine($"\t{inst.Opcode} {inst.Argument}");
                    }
                }
            }

            Console.WriteLine("\n == CLASSES ==");
            foreach(var cls in Classes)
            {
                string name = cls.Name is null ? "<anonymous>" : ((Constant.String)cls.Name).Value;

                Console.WriteLine($"{name} (# members: {cls.Members.Count}, constructor idx: {cls.Constructor.Index})");
                foreach(var member in cls.Members)
                {
                    Console.WriteLine($"\tvar {((Constant.String)member).Value}");
                }
            }
        }
    }
}
