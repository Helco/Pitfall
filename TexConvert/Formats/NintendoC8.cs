using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

// TODO: pretty sure we need mipmap handling here

class NintendoC8 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        int result = 256 * 2;
        var (width, height) = (header.Width, header.Height);
        for (int i = 0; i < Math.Max(1, header.Mipmaps); i++)
        {
            result += width * height;
            width /= 2;
            height /= 2;
        }
        return result;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var pixels = new Rgba32[header.PixelCount];
        var paletteWords = MemoryMarshal.Cast<byte, ushort>(data[^(256 * 2)..]);
        var palette = ConvertNintendoPalette(header.SubFormat, paletteWords);

        for (int i = 0; i < header.PixelCount; i++)
        {
            var j = Block2DIndex(i, header.Width, 8, 4);
            pixels[i] = palette[data[j]];
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
