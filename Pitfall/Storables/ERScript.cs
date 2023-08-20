using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Pitfall.Storables;

[StorableType]
public class ERScript : EResource
{
    public enum Op : byte
    {
        Exit,
        Copy,  // pushes a copy of another value on the stack
        Create, // pushes an empty value of a specific type
        Global, // pushes a global variable of a specific type (creates it if necessary)
        GlobalExt, // probably should only occur after Global?
        Pop, // removes a number of values from the stack
        Cond, // converts a value on the stack to a condition flag
        Jump, // three different modes: ifFlag, ifNot, goto
        Call,
        Nop9, // maybe debug ops, code was removed
        NopA,
        NopB,
        NopC,
        CrashD,
        CrashE,
        CrashF
    }

    public readonly record struct Instruction(Op Op, uint Arg1, uint Arg2)
    {
        /* Prefix legend:
         *   &     a slot on the stack (from top to bottom)
         *   $     a global variable ID
         *   #     a script instruction
         * <none>  a number
         */

        public override string ToString() => Op switch
        {
            Op.Copy => $"copy &{Arg1}",
            Op.Create => $"create {(ScriptDataType)Arg1}",
            Op.Global => $"global {(ScriptDataType)Arg1} ${Arg2:X8}",
            Op.Pop => $"pop {Arg1}",
            Op.Cond => $"cond &{Arg1}",
            Op.Jump when Arg1 == 0 => $"ifnot goto #{Arg2:D4}",
            Op.Jump when Arg1 == 1 => $"if goto #{Arg2:D4}",
            Op.Jump => $"goto #{Arg2:D4}",
            Op.Call when ScriptFunctions.TryGetFunction((int)Arg1, (int)Arg2, out var func) => $"call {func.Name}({func.ArgString})",
            Op.Call => $"call unknown {Arg1}->{Arg2}",
            _ => Op.ToString()
        };
    }

    public IReadOnlyList<Instruction> Instructions { get; private set; } = Array.Empty<Instruction>();
    public IReadOnlyList<EScriptData> InitialStack { get; private set; } = Array.Empty<EScriptData>();
    public IReadOnlyList<string> Strings { get; private set; } = Array.Empty<string>();

    public static ERScript ReadScript(BinaryReader reader)
    {
        var storable = reader.ReadStorable();
        if (storable is ERScript script)
            return script;
        throw new InvalidDataException($"Expected an ERScript but got {storable?.GetType()?.Name ?? "<null>"}");
    }

    public override void Read(BinaryReader reader)
    {
        if (ReadVersion == 1)
            base.Read(reader);
        if (ReadVersion is not 0 and not 1)
            throw new NotSupportedException($"Unsupported ERScript read version: {ReadVersion}");

        // cannot use ReadArray because of the GlobalExt special case
        var instructions = new List<Instruction>(reader.ReadInt32());
        for (int i = 0; i < instructions.Capacity; i++)
        {
            instructions.Add(ReadInstruction(reader));
            if (instructions.Last().Op == Op.Global)
                i++;
        }
        Instructions = instructions;

        InitialStack = reader.ReadArray(reader.ReadInt32(), r => r.ReadStorable() as EScriptData
            ?? throw new InvalidDataException($"Invalid data in ERScript"));
        Strings = reader.ReadArray(reader.ReadInt32(), Utils.ReadCString);
    }

    private static Instruction ReadInstruction(BinaryReader reader, bool shouldBeExt = false)
    {
        var total = reader.ReadUInt32();
        var op = (Op)(total & 0xF);
        if (shouldBeExt && op != Op.GlobalExt)
            throw new InvalidDataException($"Expected GlobalExt op after global");
        var arg1 = total >> 4;
        var arg2 = 0u;
        switch(op)
        {
            case Op.Global:
                var ext = ReadInstruction(reader, shouldBeExt: true);
                arg2 = arg1 | (ext.Arg1 << 16);
                arg1 = (arg1 >> 16) & 0xFF;
                break;
            case Op.Jump:
                arg2 = arg1 & 0xFFFFF;
                arg1 = (arg1 & 0xF_FF_FF_FF) >> 20;
                break;
            case Op.Call:
                arg2 = arg1 & 0xFFFFF;
                arg1 >>= 20;
                break;
        }
        return new Instruction(op, arg1, arg2);
    }

    public void Disassemble(TextWriter writer)
    {
        if (Name != "")
        {
            writer.WriteLine(Name);
            writer.WriteLine();
        }


        if (Instructions.Any())
        {
            writer.WriteLine("Instructions:");
            foreach (var (idx, instr) in Instructions.Indexed())
            {
                writer.Write(idx.ToString("D4"));
                writer.Write(": ");
                writer.WriteLine(instr);
            }
            writer.WriteLine();
        }

        if (InitialStack.Any())
        {
            writer.WriteLine("Initial stack:");
            foreach (var (i, data) in InitialStack.Indexed())
            {
                writer.Write(i.ToString("D4"));
                writer.Write(": ");
                writer.WriteLine(data);
            }
            writer.WriteLine();
        }

        if (Strings.Any())
        {
            writer.WriteLine("Strings:");
            foreach (var (i, str) in Strings.Indexed())
            {
                writer.Write(i.ToString("D4"));
                writer.Write(": ");
                writer.WriteLine(HttpUtility.JavaScriptStringEncode(str));
            }
            writer.WriteLine();
        }
    }
}
