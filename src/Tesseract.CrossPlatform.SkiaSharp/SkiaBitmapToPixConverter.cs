using System;
using SkiaSharp;

namespace Tesseract
{
    /// <summary>
    /// Converts a SkiaSharp <see cref="SKBitmap"/> to a <see cref="Pix"/>.
    /// </summary>
    /// <remarks>
    /// Scoped to just the two pixel layouts a decoded image realistically comes back as: 32bpp
    /// RGBA (<see cref="SKColorType.Rgba8888"/>/<see cref="SKColorType.Bgra8888"/>, whichever
    /// SkiaSharp's platform default is) and 8bpp grayscale (<see cref="SKColorType.Gray8"/>).
    /// Anything else (SkiaSharp has no public indexed/palette bitmap API the way
    /// System.Drawing.Common's 1/4/8bpp indexed <c>PixelFormat</c>s do) is normalised to
    /// Rgba8888 via <see cref="SKBitmap.Copy(SKColorType)"/> first, so any valid
    /// <see cref="SKBitmap"/> still converts -- just not always at its original bit depth.
    /// </remarks>
    public class SkiaBitmapToPixConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="img"/> to a <see cref="Pix"/>.
        /// </summary>
        public unsafe Pix Convert(SKBitmap img)
        {
            if (img == null) throw new ArgumentNullException(nameof(img));

            if (img.ColorType == SKColorType.Gray8) {
                return ConvertGray8(img);
            }

            if (img.ColorType == SKColorType.Rgba8888 || img.ColorType == SKColorType.Bgra8888) {
                return Convert32(img);
            }

            using (var normalized = img.Copy(SKColorType.Rgba8888)) {
                if (normalized == null) {
                    throw new InvalidOperationException(
                        String.Format("Source bitmap's color type {0} could not be normalised to Rgba8888.", img.ColorType));
                }
                return Convert32(normalized);
            }
        }

        private unsafe Pix Convert32(SKBitmap img)
        {
            var pix = Pix.Create(img.Width, img.Height, 32);
            try {
                var pixData = pix.GetData();
                var isBgra = img.ColorType == SKColorType.Bgra8888;
                var height = img.Height;
                var width = img.Width;
                var rowBytes = img.RowBytes;
                var imgBase = (byte*)img.GetPixels().ToPointer();

                for (int y = 0; y < height; y++) {
                    byte* imgLine = imgBase + (y * rowBytes);
                    uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                    for (int x = 0; x < width; x++) {
                        byte* pixelPtr = imgLine + (x << 2);
                        byte r, g, b, a;
                        if (isBgra) {
                            b = pixelPtr[0]; g = pixelPtr[1]; r = pixelPtr[2]; a = pixelPtr[3];
                        } else {
                            r = pixelPtr[0]; g = pixelPtr[1]; b = pixelPtr[2]; a = pixelPtr[3];
                        }
                        PixData.SetDataFourByte(pixLine, x, PixData.EncodeAsRGBA(r, g, b, a));
                    }
                }
                return pix;
            } catch {
                pix.Dispose();
                throw;
            }
        }

        private unsafe Pix ConvertGray8(SKBitmap img)
        {
            var pix = Pix.Create(img.Width, img.Height, 8);
            try {
                var pixData = pix.GetData();
                var height = img.Height;
                var width = img.Width;
                var rowBytes = img.RowBytes;
                var imgBase = (byte*)img.GetPixels().ToPointer();

                for (int y = 0; y < height; y++) {
                    byte* imgLine = imgBase + (y * rowBytes);
                    uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                    for (int x = 0; x < width; x++) {
                        PixData.SetDataByte(pixLine, x, imgLine[x]);
                    }
                }
                return pix;
            } catch {
                pix.Dispose();
                throw;
            }
        }
    }
}
