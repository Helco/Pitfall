using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TexConvert;

internal static class TexConvert
{
    private enum OutputFormat
    {
        Bmp,
        Pbm,
        Png,
        Jpg,
        Jpeg,
        Gif,
        Tiff,
        Tga,
        Webp,
        Dds,
    }

    static void Main(string[] args)
    {
        var inputOption = new Option<FileInfo>("--input", "The path to a texture or directory containing texture files")
            .LegalFilePathsOnly();
        var outputOption = new Option<FileInfo?>("--output", "The output directory [default: current directory]")
            .LegalFilePathsOnly();
        outputOption.IsRequired = false;
        var formatOption = new Option<OutputFormat>("--format", "The output image format");
        formatOption.SetDefaultValue(OutputFormat.Png);
        var vflipOption = new Option<bool>("--vflip", "Flips the image for human consumption");
        vflipOption.SetDefaultValue(true);

        var root = new RootCommand("Convert Pitfall textures")
        {
            inputOption,
            outputOption,
            formatOption,
            vflipOption
        };

        root.TreatUnmatchedTokensAsErrors = true;
        root.Handler = CommandHandler.Create<ConvertOptions>(HandleConvertCommand);
        root.Invoke(args);
    }

    private class ConvertOptions
    {
        public FileInfo Input { get; set; } = null!;
        public FileInfo? Output { get; set; }
        public OutputFormat Format { get; set; }
        public bool VFlip { get; set; }
    }

    private static void HandleConvertCommand(ConvertOptions options)
    {
        var inputFiles = (Directory.Exists(options.Input.FullName)
            ? Directory.GetFiles(options.Input.FullName)
            : new[] { options.Input.FullName })
            .Where(f => !f.EndsWith("files.list"));

        var outputDir = options.Output?.FullName ?? ".";
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

#if SYNC_CONVERSION
        foreach (var inFile in inputFiles)
            Convert(inFile, output);
#else
        Parallel.ForEach(inputFiles, inFile => Convert(inFile, outputDir, options));
#endif
    }

    private static object OutputMutex = new();

    private static void Convert(string inFile, string outputDir, ConvertOptions options)
    {
        var name = Path.GetFileName(inFile);
        var outFile = Path.Join(outputDir, name);
        var message = "Converting " + name;
        try
        {
            var image = ReadTexture(inFile);
            switch(options.Format)
            {
                case OutputFormat.Bmp: image.SaveAsBmp(outFile + ".bmp"); break;
                case OutputFormat.Pbm: image.SaveAsPbm(outFile + ".pbm"); break;
                case OutputFormat.Png: image.SaveAsPng(outFile + ".png"); break;
                case OutputFormat.Jpg:
                case OutputFormat.Jpeg: image.SaveAsJpeg(outFile + ".jpg"); break;
                case OutputFormat.Gif: image.SaveAsGif(outFile + ".gif"); break;
                case OutputFormat.Tiff: image.SaveAsTiff(outFile + ".tiff"); break;
                case OutputFormat.Tga: image.SaveAsTga(outFile + ".tga"); break;
                case OutputFormat.Webp: image.SaveAsWebp(outFile + ".webp"); break;
                case OutputFormat.Dds: SaveAsDds(image, outFile + ".dds"); break;
                default:
                    throw new NotImplementedException($"Unimplemented output format: " + options.Format);
            }
        }
        catch (Exception e)
        {
            message += " - " + e.Message;
#if DEBUG
            throw;
#endif
        }

        lock (OutputMutex)
            Console.WriteLine(message);
    }

    private static Image<Rgba32> ReadTexture(string inFile)
    {
        using var reader = new EndianReader(new FileStream(inFile, FileMode.Open, FileAccess.Read));
        var header = new TextureHeader(reader);
        IPixelFormat format = SelectPixelFormat(header);
        var size = format.GetDataSize(header);
        var data = reader.ReadBytes(size);
        if (data.Length != size)
            throw new InvalidDataException($"Could only read {data.Length} out of {size} expected bytes");

        var image = format.Convert(header, data);
        if (image == null)
            throw new NotSupportedException("Pixel format does not support proper conversion yet");
        return image;
    }

    private static IPixelFormat SelectPixelFormat(in TextureHeader hdr) => hdr.FormatId switch
    {
        // done
        0x048E => new Formats.DXT1(), // PC
        0x0890 => new Formats.DXT3(), // PC
        0x088D => new Formats.BytePalette(), // PC
        0x208C => new Formats.RGBA32(), // PC
        0x0120 => new Formats.NintendoRGBA32(),
        0x8304 => new Formats.NintendoC4(),
        0x8408 => new Formats.NintendoC8(),
        0x8104 => new Formats.NintendoCMPR(),
        0x8804 => new Formats.NintendoCMPRAlpha(),
        0x8904 => new Formats.TwoPalette4(), // Nintendo
        0x8A08 => new Formats.TwoPalette8(), // Nintendo
        0x2001 => new Formats.SonyRGBA32(),
        0x0400 => new Formats.SonyPalette4(),
        0x0800 => new Formats.SonyPalette8(),
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

    public static Rgba32[] ConvertNintendoPalette(byte subFormat, ReadOnlySpan<ushort> words)
    {
        var colors = new Rgba32[words.Length];
        if (subFormat == 5)
        {
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Convert5553(Swap(words[i]));
        }
        else if (subFormat == 6)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var a = (byte)(words[i] & 0xFF);
                var l = (byte)(words[i] >> 8);
                colors[i] = new(l, l, l, a);
            }
        }
        else
            throw new NotSupportedException($"Unsupported sub format: {subFormat:X2}");
        return colors;
    }

    public static byte ExpandSony(byte b) => (byte)Math.Clamp(b * 255 / 0x80, 0, 255);

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

    private static void SaveAsDds(Image<Rgba32> image, string outFile)
    {
        var dds = new DDSHeader()
        {
            magic = 0x20534444,
            dwSize = 124,
            dwFlags = 1 | 2 | 4 | 8 | 0x1000,
            dwHeight = (uint)image.Height,
            dwWidth = (uint)image.Width,
            dwMipMapCount = 1,
            dwPitchOrLinearSize = (uint)(image.Width * 4),
            ddspf = new()
            {
                dwSize = 32,
                dwFlags = DDSPixelFlags.AlphaPixels | DDSPixelFlags.RGB,
                dwRGBBitCount = 32,
                dwRBitMask = 0x00_00_00_FF,
                dwGBitMask = 0x00_00_FF_00,
                dwBBitMask = 0x00_FF_00_00,
                dwABitMask = 0xFF_00_00_00,
            }
        };
        var ddsSpan = MemoryMarshal.CreateSpan(ref dds, 1);
        
        using var writer = new BinaryWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write));
        writer.Write(MemoryMarshal.Cast<DDSHeader, byte>(ddsSpan));
        for (int y = 0; y < image.Height; y++)
        {
            var rowSpan = image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y);
            writer.Write(MemoryMarshal.AsBytes(rowSpan));
        }
    }
}
