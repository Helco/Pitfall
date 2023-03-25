using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class TwoPalette8 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        var (width, height) = (header.Width, header.Height);
        int result = 256 * 2 * Math.Max(1, (int)header.Mipmaps);
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
        var palette = MemoryMarshal.Cast<byte, ushort>(data[^(256 * 2 * 2)..]);

        var pixels = new Rgba32[header.PixelCount];
        for (int i = 0; i < header.PixelCount; i++)
        {
            var j = Block2DIndex(i, header.Width, 8, 4);
            pixels[i] = ConvertTwoPal8888(palette[data[j]], palette[data[j] + 256]);
        }

        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
