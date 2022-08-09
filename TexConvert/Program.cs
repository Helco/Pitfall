using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static TexConvert.Program;

namespace TexConvert;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
unsafe struct DDSHeader
{
    public uint magic;
    public uint dwSize;
    public uint dwFlags;
    public uint dwHeight;
    public uint dwWidth;
    public uint dwPitchOrLinearSize;
    public uint dwDepth;
    public uint dwMipMapCount;
    public fixed uint dwReserved1[11];
    public DDSPixelFormat ddspf;
    public uint dwCaps;
    public uint dwCaps2;
    public uint dwCaps3;
    public uint dwCaps4;
    public uint dwReserved2;
};

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct DDSPixelFormat
{
    public uint dwSize;
    public uint dwFlags;
    public uint dwFourCC;
    public uint dwRGBBitCount;
    public uint dwRBitMask;
    public uint dwGBitMask;
    public uint dwBBitMask;
    public uint dwABitMask;
}

internal static class Program
{
    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");
        foreach (var file in Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\textures"))
        {
            try
            {
                Convert(file);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(file)}: {e.Message}");
            }
        }
    }

    private static void Convert(string inFile)
    {
        var name = Path.GetFileNameWithoutExtension(inFile);
        using var reader = new BinaryReader(new FileStream(inFile, FileMode.Open, FileAccess.Read));
        if (reader.ReadUInt32() != 0x4C465854)
            throw new Exception("Invalid magic");

        while (reader.ReadByte() != 0) ;

        IPixelFormat format = reader.ReadUInt16() switch
        {
            0x48E => new DXT1(),
            0x890 => new DXT2(),
            0x88D => new BytePalette(),
            0x208C => new RGBA32(),
            var f => throw new Exception($"Unknown format {f:X4}")
        };

        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        reader.ReadUInt16();
        reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        var data = reader.ReadBytes(format.GetSize(width, height));

        var dds = new DDSHeader()
        {
            magic = 0x20534444,
            dwSize = 124,
            dwFlags = 1 | 2 | 4 | 0x1000 | format.Flag,
            dwHeight = height,
            dwWidth = width,
            dwMipMapCount = 1,
            dwPitchOrLinearSize = format.GetPitchOrLinear(width, height),
            ddspf = format.PixelFormat
        };
        var ddsSpan = MemoryMarshal.CreateSpan(ref dds, 1);

        var outFile = Path.Join("out", format.GetType().Name, name + ".dds");
        Directory.CreateDirectory(Path.Join("out", format.GetType().Name));
        using var writer = new BinaryWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write));
        writer.Write(MemoryMarshal.Cast<DDSHeader, byte>(ddsSpan));
        writer.Write(format.Transform(width, height, data));
        //Console.WriteLine($"{name} SUCCESS!");
    }

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

interface IPixelFormat
{
    DDSPixelFormat PixelFormat { get; }
    uint Flag { get; }
    int GetSize(int width, int height);
    uint GetPitchOrLinear(int width, int height);
    byte[] Transform(int width, int height, byte[] data);
}

class DXT1 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 4,
        dwFourCC = ToFourCC("DXT1")
    };
    public uint Flag => 0x80000;
    public int GetSize(int width, int height) => width * height * 8 / 4 / 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)GetSize(width, height);
    public byte[] Transform(int width, int height, byte[] data) => data;
}

class DXT2 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 4,
        dwFourCC = ToFourCC("DXT2")
    };
    public uint Flag => 0x80000;
    public int GetSize(int width, int height) => width * height * 16 / 4 / 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)GetSize(width, height);
    public byte[] Transform(int width, int height, byte[] data) => data;
}

class BytePalette : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 1 | 0x40,
        dwRGBBitCount = 32,
        dwRBitMask = 0x00_00_00_FF,
        dwGBitMask = 0x00_00_FF_00,
        dwBBitMask = 0x00_FF_00_00,
        dwABitMask = 0xFF_00_00_00,
    };
    public uint Flag => 8;
    public int GetSize(int width, int height) => width * height + 256 * 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)(width * 4);
    public byte[] Transform(int width, int height, byte[] data)
    {
        var palette = data.AsSpan(width * height);
        var output = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            uint j = zorder2DIndex(i, width, height);

            output[i * 4 + 0] = palette[data[j] * 4 + 0];
            output[i * 4 + 1] = palette[data[j] * 4 + 1];
            output[i * 4 + 2] = palette[data[j] * 4 + 2];
            output[i * 4 + 3] = palette[data[j] * 4 + 3];
        }
        return output;
    }
}


class RGBA32 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 1 | 0x40,
        dwRGBBitCount = 32,
        dwRBitMask = 0x00_00_00_FF,
        dwGBitMask = 0x00_00_FF_00,
        dwBBitMask = 0x00_FF_00_00,
        dwABitMask = 0xFF_00_00_00,
    };
    public uint Flag => 8;
    public int GetSize(int width, int height) => width * height * 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)(width * 4);
    public byte[] Transform(int width, int height, byte[] data)
    {
        var output = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            uint j = zorder2DIndex(i, width, height);

            output[i * 4 + 0] = data[j * 4 + 0];
            output[i * 4 + 1] = data[j * 4 + 1];
            output[i * 4 + 2] = data[j * 4 + 2];
            output[i * 4 + 3] = data[j * 4 + 3];
        }
        return output;
    }
}
