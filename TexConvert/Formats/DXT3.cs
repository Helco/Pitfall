using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using BCnEncoder.ImageSharp;

namespace TexConvert.Formats;

class DXT3 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header) => header.PixelCount * 16 / 4 / 4;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var dec = new BcDecoder();
        return dec.DecodeRawToImageRgba32(data, header.Width, header.Height, CompressionFormat.Bc2);
    }
}
