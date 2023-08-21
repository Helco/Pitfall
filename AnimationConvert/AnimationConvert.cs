using System.Numerics;
using System.Runtime.InteropServices;
using Pitfall.Storables;

namespace Pitfall;

internal class AnimationConvert
{
    static readonly string[] BannedFiles = new[]
    {
        "files.list"
    };

    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\animatio");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            var anim = new ERAnimation(reader);
        }
    }
}
