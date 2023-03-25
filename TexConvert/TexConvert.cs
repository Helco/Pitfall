using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TexConvert;

internal static class TexConvert
{
    static void Main(string[] args) => MainSingle(args);

    static void MainSingle(string[] args)
    {
        ConvertToPNG(@"C:\dev\Pitfall\game\ps2\textures\kg_tree_branch01", "out.png");
    }

    static void MainScan(string[] args)
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
                return (path: f, format: header.FormatId);
            })
            .ToLookup(t => t.format, t => t.path);

        foreach (var path in distinctFormats[0x2001])
            Console.WriteLine(path);
    }

    static void MainCLI(string[] args)
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
        
        foreach (var inFile in inputFiles.Where(f => !f.EndsWith("files.list")))
        {
            var name = Path.GetFileName(inFile);
            Console.Write("Converting " + name);
            try
            {
                ConvertToPNG(inFile, Path.Join(output, name + ".png"));
            }
            catch(Exception e)
            {
                Console.Write(" - " + e.Message);
#if DEBUG
                throw;
#endif
            }
            Console.WriteLine();
        }   
    }

    private static void ConvertToPNG(string inFile, string outFile)
    {
        using var reader = new EndianReader(new FileStream(inFile, FileMode.Open, FileAccess.Read));
        var header = new TextureHeader(reader);
        IPixelFormat format = SelectPixelFormat(header);
        var size = format.GetDataSize(header);
        var data = reader.ReadBytes(size);
        if (data.Length != size)
            throw new InvalidDataException($"Could only read {data.Length} out of {size} expected bytes");

        using var image = format.Convert(header, data);
        if (image == null)
            throw new NotSupportedException("Pixel format does not support proper conversion yet");
        image.Mutate(p => p.Flip(FlipMode.Vertical));
        image.SaveAsPng(outFile);
    }

    private static IPixelFormat SelectPixelFormat(in TextureHeader hdr) => hdr.FormatId switch
    {
        // done
        0x048E => new Formats.DXT1(), // PC
        0x0890 => new Formats.DXT3(), // PC
        0x088D when hdr.HasMipmaps => new Formats.MipMappedBytePalette(), // PC
        0x088D => new Formats.BytePalette(), // PC
        0x208C => new Formats.RGBA32(), // PC
        0x0120 => new Formats.NintendoRGBA32(),
        0x8304 => new Formats.NintendoC4(),
        0x8104 => new Formats.NintendoCMPR(),
        0x8804 => new Formats.NintendoCMPRAlpha(),
        0x8904 => new Formats.TwoPalette4(), // Nintendo
        0x8A08 => new Formats.TwoPalette8(), // Nintendo
        0x8408 => new Formats.NintendoC8(), // Nintendo
        0x2001 => new Formats.SonyRGBA32(),

        // not ready
        0x0800 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"), // PS2: patterns like RGBA32 but size like RGB24. weird trailing zero block
        0x0400 => throw new NotSupportedException($"Known but unsupported format {hdr.FormatId:X4}"), // PS2: looks like 4 bit palette with 32 bit colors (and alpha is max 0x80)
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
    public static uint ZOrder2D(uint x, uint y)
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

    public static uint ZOrder2DIndex(int i, in TextureHeader hdr) => ZOrder2DIndex(i, hdr.Width, hdr.Height);
    public static uint ZOrder2DIndex(int i, int width, int height)
    {
        int x = i % width;
        int y = i / width;
        if (height > width)
            (x, y) = (y, x);
        return ZOrder2D((uint)x, (uint)y);
    }

    public static int Block2DIndex(int i, int texW, int blockW, int blockH)
    {
        int texX = i % texW;
        int texY = i / texW;
        int blockX = texX / blockW;
        int blockY = texY / blockH;
        int innerX = texX % blockW;
        int innerY = texY % blockH;
        int texBlockW = (texW + blockW - 1) / blockW;
        return (blockY * texBlockW + blockX) * blockW * blockH + innerY * blockW + innerX;
    }

    public static byte Expand(byte b, int fromBits)
    {
        if (fromBits < 4 || fromBits > 7)
            throw new ArgumentOutOfRangeException("Does not support this bit count for expansion");
        var bitsLeft = 8 - fromBits;
        b &= (byte)((1 << fromBits) - 1);
        return (byte)((b << bitsLeft) | (b >> (4 - bitsLeft)));
    }

    public static byte Expand1(byte b) => (byte)(((b & 1) == 0) ? 0 : 0xff);
    public static byte Expand3(byte b)
    {
        b &= 7;
        return (byte)((b << 5) | (b << 2) | (b >> 1));
    }
    public static byte Expand4(byte b) => Expand(b, 4);
    public static byte Expand5(byte b) => Expand(b, 5);
    public static byte Expand6(byte b) => Expand(b, 6);
    public static byte Expand7(byte b) => Expand(b, 7);

    public static Rgba32 Convert565(ushort c) => new Rgba32(
        Expand5((byte)(c >> 11)),
        Expand6((byte)(c >> 5)),
        Expand5((byte)(c >> 0)),
        0xff);

    public static Rgba32 Convert5551(ushort c) => new Rgba32(
        Expand5((byte)(c >> 10)),
        Expand5((byte)(c >> 5)),
        Expand5((byte)(c >> 0)),
        Expand1((byte)(c >> 15)));

    public static Rgba32 Convert5553(ushort c)
    {
        if ((c & 0x8000) > 0)
            return Convert5551(c);
        return new Rgba32(
            Expand4((byte)(c >> 8)),
            Expand4((byte)(c >> 4)),
            Expand4((byte)(c >> 0)),
            Expand3((byte)(c >> 12)));
    }

    public static byte ExpandSony(byte b) => (byte)(b * 255 / 0x80);

    public static Rgba32 ConvertTwoPal8888(ushort c0, ushort c1) => new Rgba32(
        (byte)(c0 >> 8),
        (byte)(c0 >> 0),
        (byte)(c1 >> 8),
        (byte)(c1 >> 0));

    public static ushort Swap(ushort raw) => (ushort) ((raw << 8) | (raw >> 8));

    public static uint Swap(uint raw)
    {
        raw = (raw >> 16) | (raw << 16);
        raw = ((raw & 0xFF00FF00) >> 8) | ((raw & 0x00FF00FF) << 8);
        return raw;
    }

}
