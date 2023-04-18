# Asset conversion tools for Pitfall - The Lost Expedition

## TexConvert
You first need to extract raw textures from their archives. You can do so with [UltiNaruto's PitfallArcTool](https://github.com/UltiNaruto/PitfallARCTool).  
Converted textures will be stored as files of the chosen format in the output directory.

```
Usage:
  TexConvert [options]

Options:
  --input <input>                         The path to a texture or directory containing texture files
  --output <output>                       The output directory [default: current directory]
  --format                                The output image format [default: Png] <Bmp|Dds|Gif|Jpeg|Jpg|Pbm|Png|Tga|Tiff|Webp>
  --vflip                                 Flips the image for human consumption [default: True]
  --combine-textures                      Combines natively-split textures into single image [default: True]
  --version                               Show version information
  -?, -h, --help                          Show help and usage information
  ```
  
## ModelConvert
**WIP**
