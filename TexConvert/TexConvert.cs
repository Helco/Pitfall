using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TexConvert;

internal static class TexConvert
{
    static void Main(string[] args)
    {
        var baseDir = @"C:\dev\Pitfall\game";
        var dirs = new[] { "pc", "wii", "gc", "ps2" };
        var texDir = "textures";

        var distinctFormats = dirs
            .Select(d => Path.Combine(baseDir, d, texDir))
            .SelectMany(Directory.GetFiles)
            .Where(f => Path.GetFileName(f) != "files.list")
            .Select(f =>
            {
                using var reader = new EndianReader(new FileStream(f, FileMode.Open, FileAccess.Read));
                var header = new TextureHeader(reader);
                return header.FormatId;
            })
            .Distinct()
            .ToArray();
        Array.ForEach(distinctFormats, f => Console.WriteLine(f.ToString("X4")));
    }

    static void Main2(string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            Console.WriteLine("usage: TexConvert.exe <input directory or file> [output directory]");
            return;
        }

        var inputFiles = Directory.Exists(args[0])
            ? Directory.GetFiles(args[0])
            : new[] { args[0] };
        var output = args.Length > 1 ? args[1] : ".";
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);
        
        foreach (var inFile in inputFiles)
        {
            var name = Path.GetFileName(inFile);
            Console.Write("Converting " + name);
            try
            {
                Convert(inFile, Path.Join(output, name + ".dds"));
            }
            catch(Exception e)
            {
                Console.Write(" - " + e.Message);
            }
            Console.WriteLine();
        }   
    }

    private static void Convert(string inFile, string outFile)
    {
        using var reader = new EndianReader(new FileStream(inFile, FileMode.Open, FileAccess.Read));
        var header = new TextureHeader(reader);
        IPixelFormat format = SelectPixelFormat(header);
        var data = reader.ReadBytes(format.GetSize(header));

        using BinaryWriter writer = WriteDDS(outFile, header, format, data);
    }

    private static BinaryWriter WriteDDS(string outFile, in TextureHeader header, IPixelFormat format, byte[] data)
    {
        var dds = new DDSHeader()
        {
            magic = 0x20534444,
            dwSize = 124,
            dwFlags = 1 | 2 | 4 | 0x1000 | format.Flag,
            dwHeight = header.Height,
            dwWidth = header.Width,
            dwMipMapCount = 1,
            dwPitchOrLinearSize = format.GetPitchOrLinear(header),
            ddspf = format.PixelFormat
        };
        var ddsSpan = MemoryMarshal.CreateSpan(ref dds, 1);
        var writer = new BinaryWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write));
        writer.Write(MemoryMarshal.Cast<DDSHeader, byte>(ddsSpan));
        writer.Write(format.Transform(header, data));
        return writer;
    }

    private static IPixelFormat SelectPixelFormat(in TextureHeader hdr) => hdr.FormatId switch
    {
        0x048E => new Formats.DXT1(),
        0x0890 => new Formats.DXT2(),
        0x088D when hdr.HasMipmaps => new Formats.MipMappedBytePalette((int)hdr.Mipmaps),
        0x088D => new Formats.BytePalette(),
        0x208C => new Formats.RGBA32(),
        0x8904 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x8804 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x8408 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x8104 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x8A08 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x0120 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x8304 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x0800 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x0400 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        0x2001 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"),
        _ => throw new Exception($"Unknown format {hdr.FormatId:X4}")
    };

    // https://www.codeproject.com/Articles/10613/C-RIFF-Parser
    public static uint ToFourCC(string FourCC)
    {
        if (FourCC.Length != 4)
        {
            throw new Exception("FourCC strings must be 4 characters long " + FourCC);
        }
  
        uint result = ((uint)FourCC[3]) << 24
                    | ((uint) FourCC[2]) << 16
                    | ((uint) FourCC[1]) << 8
                    | ((uint) FourCC[0]);
  
        return result;
    }

    // https://stackoverflow.com/questions/12157685/z-order-curve-coordinates
    static readonly uint[] B = { 0x55555555, 0x33333333, 0x0F0F0F0F, 0x00FF00FF };
    static readonly int[] S = { 1, 2, 4, 8 };
    public static uint zorder2D(uint x, uint y)
    {

        x = (x | (x << S[3])) & B[3];
        x = (x | (x << S[2])) & B[2];
        x = (x | (x << S[1])) & B[1];
        x = (x | (x << S[0])) & B[0];

        y = (y | (y << S[3])) & B[3];
        y = (y | (y << S[2])) & B[2];
        y = (y | (y << S[1])) & B[1];
        y = (y | (y << S[0])) & B[0];
        return x | (y << 1);
    }

    public static uint zorder2DIndex(int i, int width, int height)
    {
        int x = i % width;
        int y = i / width;
        if (height > width)
            (x, y) = (y, x);
        return zorder2D((uint)x, (uint)y);
    }

}
