using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NibblePalette : IPixelFormat
{
    public int GetDataSize(in TextureHeader header) => header.PixelCount / 2 + 32 * 2;

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        /* TODO: This unfortunately is still wrong
         *   - Why is the palette 64 bytes (with a pattern like it contains 32 colors with 16Bit each)?
         *   - The colors are wrong, but the overall shape is not
         *   - Is it correct that the PC version is different (fernleaf is split and rotated to be more correct than PC)
         *   - Is the block size always 8?
         */

        var pixels = new Rgba32[header.PixelCount];
        var palette = MemoryMarshal.Cast<byte, ushort>(data[^(32 * 2)..]);

        //data = data.SelectMany(b => new[] { (byte)(b & 0xf), (byte)(b >> 4) }).ToArray();

        for (int i = 0; i < header.PixelCount; i++)
        {
            var index = data[Block2DIndex(i, header.Width, 8, 8) / 2];
            if (i % 2 == 1)
                index &= 0xf;
            else
                index >>= 4;

            pixels[i] = Convert5553(Swap(palette[index + 16])); //+ (1 - (i % 2))]));
        }
        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
