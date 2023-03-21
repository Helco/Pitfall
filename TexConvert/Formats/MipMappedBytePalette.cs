namespace TexConvert.Formats;

class MipMappedBytePalette : BytePalette
{
    private readonly int mipmaps;

    public MipMappedBytePalette(int mipmaps) => this.mipmaps = mipmaps;

    public override int GetSize(int width, int height)
    {
        int result = 256 * 4;
        for (int i = 0; i < mipmaps; i++)
        {
            result += width * height;
            width /= 2;
            height /= 2;
        }
        return result;
    }
}
