using Pitfall.Storables;

namespace ScriptDisassembler;

internal class ScriptDisassembler
{
    static readonly string[] BannedFiles = new[]
    {
        "files.list",
        "0-evan" // ERHavokWorld(0)
    };

    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\scripts");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);
            var script = ERScript.ReadScript(reader);

            using var writer = new StreamWriter("out/" + name + ".txt");
            script.Disassemble(writer);
        }
    }
}
