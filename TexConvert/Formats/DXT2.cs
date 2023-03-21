using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class DXT2 : IPixelFormat
{
    public DDSPixelFormat PixelFormat => new()
    {
        dwSize = 32,
        dwFlags = 4,
        dwFourCC = ToFourCC("DXT2")
    };
    public uint Flag => 0x80000;
    public int GetSize(int width, int height) => width * height * 16 / 4 / 4;
    public uint GetPitchOrLinear(int width, int height) => (uint)GetSize(width, height);
    public byte[] Transform(int width, int height, byte[] data) => data;
}
