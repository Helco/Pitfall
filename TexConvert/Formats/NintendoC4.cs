using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NintendoC4 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        var curPixelCount = header.PixelCount;
        var result = 16 * 2;
        for (int i = 0; i < Math.Max(1, header.Mipmaps); i++)
        {
            result += curPixelCount / 2;
            curPixelCount /= 4;
        }
        return result;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var pixels = new Rgba32[header.PixelCount];
        var paletteWords = MemoryMarshal.Cast<byte, ushort>(data[^(16 * 2)..]);
        var palette = ConvertNintendoPalette(header.SubFormat, paletteWords);

        for (int i = 0; i < header.PixelCount; i++)
        {
            var j = Block2DIndex(i, header.Width, 8, 8);
            var palI = i % 2 == 1
                ? data[j / 2] & 0xf
                : data[j / 2] >> 4;
            pixels[i] = palette[palI];
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
