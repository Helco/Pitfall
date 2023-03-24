using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static TexConvert.TexConvert;

namespace TexConvert.Formats;

class NintendoC8 : IPixelFormat
{
    public int GetDataSize(in TextureHeader header)
    {
        var (width, height) = (header.Width, header.Height);
        int result = 256 * 2 * Math.Max(1, (int)header.Mipmaps);
        for (int i = 0; i < Math.Max(1, header.Mipmaps); i++)
        {
            result += width * height;
            width /= 2;
            height /= 2;
        }
        return result;
    }

    public Image<Rgba32> Convert(in TextureHeader header, byte[] data)
    {
        var palette = MemoryMarshal.Cast<byte, ushort>(data[^(256 * 2 * 2)..]);
        var pixels = new Rgba32[header.PixelCount];
        for (int i = 0; i < header.PixelCount; i++)
        {
            var j = Block2DIndex(i, header.Width, 8, 4);
            pixels[i] = Convert4444_(Swap(palette[data[j]]));
        }

        var pixelCount = header.PixelCount;
        var r = Enumerable.Range(0, 256)
            .Select(i => (val: i, count: data.Take(pixelCount).Count(a => a == i)))
            .OrderByDescending(t => t.count)
            .ToArray();
        foreach (var (v, c) in r)
            Console.WriteLine($"{v}: {c}");

        for (int j = 0; j < 512; j++)
        {
            Console.WriteLine(System.Convert.ToString(Swap(palette[j]), 2).PadLeft(16, '0'));
        }

        Console.WriteLine(Swap(palette[4]).ToString("X4"));
        Console.WriteLine(Swap(palette[8]).ToString("X4"));
        Console.WriteLine(Swap(palette[9]).ToString("X4"));
        Console.WriteLine(System.Convert.ToString(Swap(palette[8]), 2).PadLeft(16, '0'));
        Console.WriteLine(System.Convert.ToString(Swap(palette[9]), 2).PadLeft(16, '0'));

        return Image.LoadPixelData(pixels, header.Width, header.Height);
    }
}
