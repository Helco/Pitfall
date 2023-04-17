using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class TwoPalette4 : ISplitBAPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        var curPixelCount = header.PixelCount;
        var result = 32 * 2;
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
        var palette = MemoryMarshal.Cast<byte, ushort>(data[^(32 * 2)..]);

        for (int i = 0; i < header.PixelCount; i++)
        {
            var index = data[Block2DIndex(i, header.Width, 8, 8) / 2];
            if (i % 2 == 1)
                index &= 0xf;
            else
                index >>= 4;

            pixels[i] = ConvertTwoPal8888(palette[index], palette[index + 16]);
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
