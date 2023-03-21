using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class BytePalette : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 1 | 0x40,
        dwRGBBitCount = 32,
        dwRBitMask = 0x00_00_00_FF,
        dwGBitMask = 0x00_00_FF_00,
        dwBBitMask = 0x00_FF_00_00,
        dwABitMask = 0xFF_00_00_00,
    };
    public uint Flag => 8;
    public virtual int GetSize(int width, int height) => width * height + 256 * 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)(width * 4);
    public byte[] Transform(int width, int height, byte[] data)
    {
        var palette = data[^(256 * 4)..];
        var output = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            uint j = zorder2DIndex(i, width, height);

            output[i * 4 + 0] = palette[data[j] * 4 + 0];
            output[i * 4 + 1] = palette[data[j] * 4 + 1];
            output[i * 4 + 2] = palette[data[j] * 4 + 2];
            output[i * 4 + 3] = palette[data[j] * 4 + 3];
        }
        return output;
    }
}
