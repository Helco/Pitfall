using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class BytePalette : IPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        int result = 256 * 4;
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
        var paletteBytes = data[^(256 * 4)..];
        var palette = Enumerable
            .Range(0, 256)
            .Select(i => new Rgba32(
                paletteBytes[i * 4 + 0],
                paletteBytes[i * 4 + 1],
                paletteBytes[i * 4 + 2],
                paletteBytes[i * 4 + 3]))
            .ToArray();

        var header_ = header;
        var pixels = Enumerable
            .Range(0, header.PixelCount)
            .Select(i => ZOrder2DIndex(i, header_))
            .Select(i => palette[data[i]])
            .ToArray();
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
