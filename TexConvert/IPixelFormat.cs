using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TexConvert;

interface IPixelFormat
{
    int GetDataSize(in TextureHeader header);
    Image<Rgba32>? Convert(in TextureHeader header, byte[] data) => null;    
}
