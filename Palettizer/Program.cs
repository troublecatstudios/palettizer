using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Palettizer {
    class Program {
        // command 1: palette-create
        //     arg[0]: image file path
        //     arg[1]: output image file path
        //     arg[2]: output palette file path
        // takes in a source image and normalizes the colors
        // for each pixel in the source image:
        //   - assign it an index in the palette
        //   - compute the binary representation of the palette color and its index.
        //    COLOR: = (R: 33, G: 31, B: 48)   |   R: 10 00 01, G: 01 11 11, B: 11,00,00
        //    Index: = 0                       |   00 00 00
        //   - modify the last bit in each color channel to match the bit in the index:
        //    COLOR: R: 10 00 00, G: 01 11 00, B: 11,00,00
        //   - replace the color in the image and the palette
        // output the new palette to a file
        // output the new image to a file
        static void Main(string[] args) {
            var cmd = args[0];
            switch (cmd) {
                case "normalize":
                    var outputPath = args.Length > 2 ? args[2] : null;
                    var paletteName = args.Length > 3 ? args[3] : null;
                    DoNormalize(args[1], outputPath, paletteName);
                    break;
                case "help":
                default: ShowHelp(); break;
            }
        }

        private static void DoNormalize(string imagePath, string outputPath = null, string outputPalette = null) {
            outputPath = outputPath ?? "./normalized-image.png";
            outputPalette = outputPalette ?? "./palette.png";

            Color[] sourcePalette = new Color[64];
            Color[] normalPalette = new Color[64];
            Dictionary<Color, int> colorLookup = new Dictionary<Color, int>();

            using FileStream pngStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var image = new Bitmap(pngStream);
            var bmp = new Bitmap(image);
            int width = bmp.Width, height = bmp.Height;
            using var graphics = Graphics.FromImage(bmp);
            graphics.DrawImage(image, 0, 0, width, height);

            int idx = 0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (idx > 63) {
                        Console.WriteLine("ERR! image contains more than 64 unique color values.");
                        return;
                    }
                    var px = bmp.GetPixel(x, y);
                    if (px.A < 1) continue;
                    if (!colorLookup.ContainsKey(px)) {
                        colorLookup.Add(px, idx);
                        sourcePalette[idx] = px;
                        normalPalette[idx] = NormalizeColor(px, idx);
                        idx++;
                    }

                    if (colorLookup.TryGetValue(px, out var normIndex)) {
                        var normColor = normalPalette[normIndex];
                        bmp.SetPixel(x, y, normColor);
                    }
                }
            }

            using var normalPaletteImage = new Bitmap(8, 8);
            for (var y = 0; y < 8; y++) {
                for (var x = 0; x < 8; x++) {
                    idx = (y * 8) + x;
                    normalPaletteImage.SetPixel(x, y, normalPalette[idx]);
                }
            }

            normalPaletteImage.Save(outputPalette);
            bmp.Save(outputPath);
        }

        private static Color NormalizeColor(Color inputColor, int index) {
            int r = inputColor.R;
            int g = inputColor.G;
            int b = inputColor.B;

            int rByte = ((index >> 4) & 0b_0000_0011);
            int gByte = ((index >> 2) & 0b_0000_0011);
            int bByte = ((index) & 0b_0000_0011);

            r = (r >> 2) << 2 & 0b_1111_1100;
            g = (g >> 2) << 2 & 0b_1111_1100;
            b = (b >> 2) << 2 & 0b_1111_1100;

            r = r | (rByte & 0b_0000_0011);
            g = g | (gByte & 0b_0000_0011);
            b = b | (bByte & 0b_0000_0011);

            return Color.FromArgb(inputColor.A, r, g, b);
        }

        private static int GetColorIndex(Color inputColor) {
            int r = (int)inputColor.R;
            int g = (int)inputColor.G;
            int b = (int)inputColor.B;
            byte rByte = (byte)((r << 30) >> 26);
            byte gByte = (byte)((g << 30) >> 28);
            byte bByte = (byte)((b << 30) >> 30);

            int index = (rByte & 0b_110000) | (gByte & 0b_001100) | (bByte & 0b_000011);

            return index;
        }

        private static void ShowHelp(string command = "command") {
            Console.WriteLine($"palettizer <{command}> [options]");
            Console.WriteLine("");

            switch (command) {
                case "normalize":
                    Console.WriteLine("  Options");
                    Console.WriteLine("      ");
                    Console.WriteLine("      [0]:           The path to the image that should be processed.");
                    Console.WriteLine("      [1]:           The output path for the new normalized image.");
                    Console.WriteLine("      [2]:           The output path for generated palette.");
                    break;
                default:
                    Console.WriteLine("  Commands");
                    Console.WriteLine("      ");
                    Console.WriteLine("      help           Displays this help text.");
                    Console.WriteLine("      normalize      Converts color information and generates a base palette for a given image.");
                    break;
            }
        }
    }
}
