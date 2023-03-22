﻿using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class RGBA32 : IPixelFormat
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
            uint j = ZOrder2DIndex(i, width, height);

            output[i * 4 + 0] = data[j * 4 + 0];
            output[i * 4 + 1] = data[j * 4 + 1];
            output[i * 4 + 2] = data[j * 4 + 2];
            output[i * 4 + 3] = data[j * 4 + 3];
        }
        return output;
    }
}
