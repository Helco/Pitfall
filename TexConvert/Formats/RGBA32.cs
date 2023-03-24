using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class RGBA32 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header) => header.PixelCount * 4;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var header_ = header;
        var pixels = Enumerable
            .Range(0, header.PixelCount)
            .Select(i => ZOrder2DIndex(i, header_))
            .Select(i => new Rgba32(
                data[i * 4 + 0],
                data[i * 4 + 1],
                data[i * 4 + 2],
                data[i * 4 + 3]))
            .ToArray();
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
