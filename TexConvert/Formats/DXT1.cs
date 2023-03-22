using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class DXT1 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = DDSPixelFlags.FourCC,
        dwFourCC = ToFourCC("DXT1")
    };
    public uint Flag => 0x80000;
    public int GetSize(int width, int height) => width * height * 8 / 4 / 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)GetSize(width, height);
    public byte[] Transform(int width, int height, byte[] data) => data;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var dec = new BcDecoder();
        return dec.DecodeRawToImageRgba32(data, header.Width, header.Height, CompressionFormat.Bc1);
    }
}
