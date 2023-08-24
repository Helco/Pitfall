namespace Pitfall;

internal class LevelConvert
{
    static readonly string[] BannedFiles = new[]
    {
        "files.list",
    };

    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\levels");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);
            var level = new Level(reader);
            Console.WriteLine(name);
        }
        DynTypeInfo.Statistics.Print();
    }
}
