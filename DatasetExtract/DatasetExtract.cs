using Pitfall.Storables;

namespace DatasetExtract;

internal class DatasetExtract
{
    static readonly string[] BannedFiles = new[]
    {
        "files.list"
    };

    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\datasets");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);
            var dataset = new ERDataset(reader);

            Directory.CreateDirectory("out/" + name);
            foreach (var fileset in dataset.FileSets)
            {
                foreach (var (id, data) in fileset.Files)
                {
                    File.WriteAllBytes($"out/{name}/{fileset.Name}_{id:X8}", data);
                }
            }
        }
    }
}
