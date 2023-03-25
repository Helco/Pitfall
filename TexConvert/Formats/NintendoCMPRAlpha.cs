using System.Runtime.InteropServices;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NintendoCMPRAlpha : IPixelFormat
{
    private int GetCMPRDataSize(in TextureHeader header) => header.PixelCount * 8 / 4 / 4;
    public int GetDataSize(in TextureHeader header) => GetCMPRDataSize(header) + header.PixelCount / 2;

    private static ushort ReverseBlockPixels(ushort r)
    {
        return (ushort)(
            ((r & 0b0000_0011_0000_0011) << 6) |
            ((r & 0b0000_1100_0000_1100) << 2) |
            ((r & 0b0011_0000_0011_0000) >> 2) |
            ((r & 0b1100_0000_1100_0000) >> 6));
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var cmprSize = GetCMPRDataSize(header);
        var dataWords = MemoryMarshal.Cast<byte, ushort>(data);
        for (int i = 0; i < dataWords.Length; i += 4)
        {
            dataWords[i + 0] = Swap(dataWords[i + 0]);
            dataWords[i + 1] = Swap(dataWords[i + 1]);
            dataWords[i + 2] = ReverseBlockPixels(dataWords[i + 2]);
            dataWords[i + 3] = ReverseBlockPixels(dataWords[i + 3]);
        }

        var deblocked = new byte[data.Length];
        var deblockedQWords = MemoryMarshal.Cast<byte, ulong>(deblocked);
        var dataQWords = MemoryMarshal.Cast<byte, ulong>(data);
        for (int i = 0; i < dataQWords.Length; i++)
        {
            var j = Block2DIndex(i, header.Width / 4, 2, 2);
            deblockedQWords[i] = dataQWords[j];
        }

        var dec = new BcDecoder();
        var image = dec.DecodeRawToImageRgba32(deblocked, header.Width, header.Height, CompressionFormat.Bc1);
        var alpha = dec.DecodeRaw(deblocked.AsSpan(cmprSize).ToArray(), header.Width, header.Height, CompressionFormat.Bc1);

        for (int y = 0; y < header.Height; y++)
        {
            var colorSpan = image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y);
            for (int x = 0; x < header.Width; x++)
                colorSpan[x].A = alpha[y * header.Width + x].g;
        }

        return image;
    }
}
