using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NintendoC4 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = DDSPixelFlags.AlphaPixels | DDSPixelFlags.RGB,
        dwRGBBitCount = 32,
        dwRBitMask = 0x00_00_00_FF,
        dwGBitMask = 0x00_00_FF_00,
        dwBBitMask = 0x00_FF_00_00,
        dwABitMask = 0xFF_00_00_00,
    };
    public uint Flag => 8;
    public virtual int GetSize(int width, int height) => width * height / 2 + 16 * 2;
    public uint GetPitchOrLinear(int width, int height) => (uint)(width * 4);
    public byte[] Transform(int width, int height, byte[] data)
    {
        var palette = data[^(256 * 4)..];
        var output = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            uint j = ZOrder2DIndex(i, width, height);

            output[i * 4 + 0] = palette[data[j] * 4 + 0];
            output[i * 4 + 1] = palette[data[j] * 4 + 1];
            output[i * 4 + 2] = palette[data[j] * 4 + 2];
            output[i * 4 + 3] = palette[data[j] * 4 + 3];
        }
        return output;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var pixels = new Rgba32[header.PixelCount];
        var palette = MemoryMarshal.Cast<byte, ushort>(data[^(16 * 2)..]);
        for (int i = 0; i < header.PixelCount; i++)
        {
            var j = Block2DIndex(i, header.Width, 8, 8);
            var palI = i % 2 == 1
                ? data[j / 2] & 0xf
                : data[j / 2] >> 4;
            pixels[i] = Convert5553(Swap(palette[palI]));
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
