using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TexConvert.Formats;

class DXT1 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header) => header.PixelCount * 8 / 4 / 4;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var dec = new BcDecoder();
        return dec.DecodeRawToImageRgba32(data, header.Width, header.Height, CompressionFormat.Bc1);
    }
}
