using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TexConvert.Formats;

class NintendoRGBA32 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header) => header.PixelCount * 4;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        // Note this is not the weird bit-interleaved Nintendo RGBA32 format, yay
        var pixels = Enumerable
            .Range(0, header.PixelCount)
            .Select(i => new Rgba32(
                data[(i * 4) + 0],
                data[(i * 4) + 1],
                data[(i * 4) + 2],
                data[(i * 4) + 3]))
            .ToArray();
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
