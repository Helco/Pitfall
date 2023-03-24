namespace TexConvert.Formats;

class MipMappedBytePalette : BytePalette
{
    public override int GetDataSize(in TextureHeader header)
    {
        int result = 256 * 4;
        var (width, height) = (header.Width, header.Height);
        for (int i = 0; i < header.Mipmaps; i++)
        {
            result += width * height;
            width /= 2;
            height /= 2;
        }
        return result;
    }
}
