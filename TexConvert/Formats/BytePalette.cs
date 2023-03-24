using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class BytePalette : IPixelFormat
{
    public virtual int GetDataSize(in TextureHeader header) => header.PixelCount + 256 * 4;

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
}
