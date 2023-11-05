using Blaze_Interpreter;
using System;
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

        private Function _staticFunction;
        private readonly Dictionary<string, Constant> _stringConstantMap;

        public Module()
        {
            Constants = new List<Constant>();
            Variables = new List<Variable>();
            Functions = new List<Function>();

            _stringConstantMap = new Dictionary<string, Constant>();
            _staticFunction = CreateAnonymousFunction(0);
        }

        public Function CreateFunction(string name, int num_args, VariableType visibility = VariableType.PRIVATE)
        {
            Constant name_const = AddConstant(new Constant.String(name));
            Function func = new Function(this, name_const, num_args);
            func.Index = Functions.Count;
            Functions.Add(func);

            Variable func_var = new Variable(this, visibility, name_const);
            Variables.Add(func_var);

            // Initialize in the static function so parent modules can access it
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

            constant.Index = Constants.Count;
            Constants.Add(constant);
            return constant;
        }


        public void FromBinary(BinaryReader br)
        {
            // Clear default static function
            Functions.Clear();

            uint id = br.ReadUInt32();

            if (id != 0x6D7A6C62)
                throw new Exception("File is not a Blaze module file");

            byte major = br.ReadByte();
            byte minor = br.ReadByte();

            ushort const_count = br.ReadUInt16();
            for(int i = 0;  i < const_count; i++)
            {
                ConstantType type = (ConstantType)br.ReadByte();
                Constant constant = null;
                
                switch(type)
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
                func.FromBinary(br);
                Functions.Add(func);
            }

            _staticFunction = Functions[0];

            ushort class_count = br.ReadUInt16();
        }

        public void ToBinary(BinaryWriter bw)
        {
            // 0x626C7A6D ("blzm")
            // Since BinaryWriter writes in little endian, the number must be backwards
            // 0x6D7A6C62
            bw.Write((uint)0x6D7A6C62);
            // 01 00  (major, minor) (BinaryWriter is little endian so 0x0001)
            bw.Write((ushort)0x0001);

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
            bw.Write((ushort)0);
        }

        public void PrintToConsole()
        {
            Console.WriteLine("== CONSTANTS ==");
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

                Console.WriteLine($"{name} (# args: {func.NumOfArgs}, # locals: {func.NumOfLocals}, Varargs: {func.Varargs})");
                foreach (var inst in func.Instructions)
                {
                    Console.WriteLine($"    {inst.Id} {inst.Argument}");
                }
            }
        }
    }
}
