using System.Runtime.InteropServices;

namespace TexConvert;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
unsafe struct DDSHeader
{
    public uint magic;
    public uint dwSize;
    public uint dwFlags;
    public uint dwHeight;
    public uint dwWidth;
    public uint dwPitchOrLinearSize;
    public uint dwDepth;
    public uint dwMipMapCount;
    public fixed uint dwReserved1[11];
    public DDSPixelFormat ddspf;
    public uint dwCaps;
    public uint dwCaps2;
    public uint dwCaps3;
    public uint dwCaps4;
    public uint dwReserved2;
};

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct DDSPixelFormat
{
    public uint dwSize;
    public uint dwFlags;
    public uint dwFourCC;
    public uint dwRGBBitCount;
    public uint dwRBitMask;
    public uint dwGBitMask;
    public uint dwBBitMask;
    public uint dwABitMask;
}
