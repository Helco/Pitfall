using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Pitfall.Storables;

namespace Pitfall;

public readonly record struct ScriptFunction(string Name, IReadOnlyList<ScriptDataType> Args, ScriptDataType ReturnType)
{
    public string ArgString
    {
        get
        {
            var result = new string(Args.Select(a => (char)a).ToArray());
            if (ReturnType != ScriptDataType.Invalid)
                result += $"->{(char)ReturnType}";
            return result;
        }
    }
}

public static class ScriptFunctions
{
    internal struct JsonScriptFunction
    {
        public string name { get; set; }
        public string args { get; set; }
        public int returnType { get; set; }
    }

    private static IReadOnlyList<IReadOnlyList<ScriptFunction>>? tables = null;

    public static IReadOnlyList<IReadOnlyList<ScriptFunction>> Tables
    {
        get
        {
            if (tables != null)
                return tables;

            var assembly = typeof(ScriptFunctions).Assembly;
            var jsonTables = JsonSerializer.Deserialize<JsonScriptFunction[][]>(
                assembly.GetManifestResourceStream("Pitfall.ScriptTables.json")!);

            tables = jsonTables!.Select(jsonTable => jsonTable.Select(jsonFunc =>
            {
                var args = string.IsNullOrEmpty(jsonFunc.args) || jsonFunc.args == "0"
                    ? Array.Empty<ScriptDataType>()
                    : jsonFunc.args.Select(ch => (ScriptDataType)ch).ToArray();
                var returnType = jsonFunc.returnType == ' '
                    ? ScriptDataType.Invalid
                    : (ScriptDataType)jsonFunc.returnType;
                return new ScriptFunction(jsonFunc.name, args, returnType);
            }).ToArray() as IReadOnlyList<ScriptFunction>).ToArray();
            return tables;
        }
    }

    public static bool TryGetFunction(int table, int func, out ScriptFunction function)
    {
        function = default;
        if (table < Tables.Count && func < Tables[table].Count)
        {
            function = Tables[table][func];
            return true;
        }
        return false;
    }
}
