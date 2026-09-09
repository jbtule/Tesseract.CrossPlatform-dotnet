using System;
using SkiaSharp;

namespace Tesseract
{
    /// <summary>
    /// Converts a <see cref="Pix"/> to a SkiaSharp <see cref="SKBitmap"/>.
    /// </summary>
    /// <remarks>
    /// Supports pix depths 32 (RGB/RGBA) and 8 (grayscale, un-colormapped only -- a
    /// colormapped 8bpp pix throws, matching the scope boundary of
    /// <see cref="SkiaBitmapToPixConverter"/>) and 1 (unpacked into an 8bpp grayscale bitmap --
    /// SkiaSharp has no packed-1bpp <see cref="SKColorType"/> to target directly). Other depths
    /// (2/4/16, colormapped 8bpp) are out of scope for this first cut; see
    /// <see cref="SkiaBitmapToPixConverter"/>'s own remarks for why.
    /// </remarks>
    public class PixToSkiaBitmapConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="pix"/> to a <see cref="SKBitmap"/>.
        /// </summary>
        /// <param name="pix">The source image to be converted.</param>
        /// <param name="includeAlpha">Whether to carry the pix's alpha channel through (32bpp only; ignored otherwise).</param>
        public unsafe SKBitmap Convert(Pix pix, bool includeAlpha = false)
        {
            if (pix == null) throw new ArgumentNullException(nameof(pix));

            switch (pix.Depth) {
                case 32:
                    return Convert32(pix, includeAlpha);
                case 8:
                    if (pix.Colormap != null) {
                        throw new NotSupportedException("Colormapped 8bpp Pix is not supported by PixToSkiaBitmapConverter.");
                    }
                    return ConvertGray8(pix);
                case 1:
                    return Convert1(pix);
                default:
                    throw new InvalidOperationException(String.Format("Pix depth {0} is not supported.", pix.Depth));
            }
        }

        private unsafe SKBitmap Convert32(Pix pix, bool includeAlpha)
        {
            var info = new SKImageInfo(pix.Width, pix.Height, SKColorType.Rgba8888,
                includeAlpha ? SKAlphaType.Unpremul : SKAlphaType.Opaque);
            var bmp = new SKBitmap(info);
            var pixData = pix.GetData();
            var imgBase = (byte*)bmp.GetPixels().ToPointer();
            var rowBytes = bmp.RowBytes;
            var height = pix.Height;
            var width = pix.Width;

            for (int y = 0; y < height; y++) {
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);
                byte* imgLine = imgBase + (y * rowBytes);

                for (int x = 0; x < width; x++) {
                    var color = PixColor.FromRgba(pixLine[x]);
                    byte* pixelPtr = imgLine + (x << 2);
                    pixelPtr[0] = color.Red;
                    pixelPtr[1] = color.Green;
                    pixelPtr[2] = color.Blue;
                    pixelPtr[3] = includeAlpha ? color.Alpha : (byte)255;
                }
            }
            return bmp;
        }

        private unsafe SKBitmap ConvertGray8(Pix pix)
        {
            var info = new SKImageInfo(pix.Width, pix.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            var bmp = new SKBitmap(info);
            var pixData = pix.GetData();
            var imgBase = (byte*)bmp.GetPixels().ToPointer();
            var rowBytes = bmp.RowBytes;
            var height = pix.Height;
            var width = pix.Width;

            for (int y = 0; y < height; y++) {
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);
                byte* imgLine = imgBase + (y * rowBytes);

                for (int x = 0; x < width; x++) {
                    imgLine[x] = (byte)PixData.GetDataByte(pixLine, x);
                }
            }
            return bmp;
        }

        private unsafe SKBitmap Convert1(Pix pix)
        {
            var info = new SKImageInfo(pix.Width, pix.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            var bmp = new SKBitmap(info);
            var pixData = pix.GetData();
            var imgBase = (byte*)bmp.GetPixels().ToPointer();
            var rowBytes = bmp.RowBytes;
            var height = pix.Height;
            var width = pix.Width;

            for (int y = 0; y < height; y++) {
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);
                byte* imgLine = imgBase + (y * rowBytes);

                for (int x = 0; x < width; x++) {
                    var bit = PixData.GetDataBit(pixLine, x);
                    imgLine[x] = bit != 0 ? (byte)255 : (byte)0;
                }
            }
            return bmp;
        }
    }
}
