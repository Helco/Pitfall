using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class SonyPalette8 : IPixelFormat
{
    public virtual int GetDataSize(in TextureHeader header)
    {
        var curPixelCount = header.PixelCount;
        var result = 256 * 4;
        for (int i = 0; i < Math.Max(1, header.Mipmaps); i++)
        {
            result += curPixelCount;
            curPixelCount /= 4;
        }
        return result;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var paletteBytes = data[^(256 * 4)..];
        var palette = Enumerable
            .Range(0, 256)
            .Select(i => new Rgba32(
                paletteBytes[i * 4 + 0],
                paletteBytes[i * 4 + 1],
                paletteBytes[i * 4 + 2],
                ExpandSony(paletteBytes[i * 4 + 3])))
            .ToArray();

        var header_ = header;
        var pixels = Enumerable
            .Range(0, header.PixelCount)
            .Select(i => palette[data[i]])
            .ToArray();
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
