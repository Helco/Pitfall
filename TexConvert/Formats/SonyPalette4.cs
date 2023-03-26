using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class SonyPalette4 : IPixelFormat
{
    public virtual int GetDataSize(in TextureHeader header)
    {
        var curPixelCount = header.PixelCount;
        var result = 16 * 4;
        for (int i = 0; i < Math.Max(1, header.Mipmaps); i++)
        {
            result += curPixelCount / 2;
            curPixelCount /= 4;
        }
        return result;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var paletteBytes = data[^(16 * 4)..];
        var palette = Enumerable
            .Range(0, 16)
            .Select(i => new Rgba32(
                paletteBytes[i * 4 + 0],
                paletteBytes[i * 4 + 1],
                paletteBytes[i * 4 + 2],
                ExpandSony(paletteBytes[i * 4 + 3])))
            .ToArray();

        var header_ = header;
        var pixels = Enumerable
            .Range(0, header.PixelCount)
            .Select(i => palette[i % 2 == 0
                ? data[i / 2] & 0xf
                : data[i / 2] >> 4])
            .ToArray();
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
