using System;
using System.IO;
using System.Reflection.PortableExecutable;

namespace Pitfall;

internal class Program
{
    static void Main(string[] args)
    {
        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");
        foreach (var file in files)
        {
            if (file.Contains("files.list"))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            new Model(reader);
        }
    }
}
