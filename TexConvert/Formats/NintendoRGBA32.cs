using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NintendoRGBA32 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = DDSPixelFlags.AlphaPixels | DDSPixelFlags.RGB,
        dwRGBBitCount = 32,
        dwRBitMask = 0x00_00_00_FF,
        dwGBitMask = 0x00_00_FF_00,
        dwBBitMask = 0x00_FF_00_00,
        dwABitMask = 0xFF_00_00_00,
    };
    public uint Flag => 8;
    public int GetSize(int width, int height) => width * height * 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)(width * 4);
    public byte[] Transform(int width, int height, byte[] data)
    {
        var output = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            uint j  = (uint)i;

            output[i * 4 + 0] = data[j * 4 + 2];
            output[i * 4 + 1] = data[j * 4 + 1];
            output[i * 4 + 2] = data[j * 4 + 0];
            output[i * 4 + 3] = data[j * 4 + 3];
        }
        return output;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        // Note this is not the weird bit-interleaved Nintendo RGBA32 format, yay
        var pixels = new Rgba32[header.PixelCount];
        for (int i = 0; i < header.PixelCount; i++)
        {
            pixels[i] = new Rgba32(
                data[i * 4 + 2],
                data[i * 4 + 1],
                data[i * 4 + 0],
                data[i * 4 + 3]);
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
