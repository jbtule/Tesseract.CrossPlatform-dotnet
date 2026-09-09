using SkiaSharp;

namespace Tesseract
{
    /// <summary>
    /// Handles converting between <see cref="Pix"/> and SkiaSharp's <see cref="SKBitmap"/>,
    /// and between <see cref="PixColor"/> and <see cref="SKColor"/>. The SkiaSharp-flavored
    /// equivalent of Tesseract.Drawing's <c>PixConverter</c> (named distinctly, not
    /// <c>PixConverter</c>, so both packages can be referenced in the same project without a
    /// type-name collision).
    /// </summary>
    public static class SkiaPixConverter
    {
        private static readonly SkiaBitmapToPixConverter bitmapConverter = new SkiaBitmapToPixConverter();
        private static readonly PixToSkiaBitmapConverter pixConverter = new PixToSkiaBitmapConverter();

        /// <summary>
        /// Converts the specified <paramref name="pix"/> to an <see cref="SKBitmap"/>.
        /// </summary>
        public static SKBitmap ToSkiaBitmap(Pix pix, bool includeAlpha = false)
        {
            return pixConverter.Convert(pix, includeAlpha);
        }

        /// <summary>
        /// Converts the specified <paramref name="img"/> to a <see cref="Pix"/>.
        /// </summary>
        public static Pix ToPix(SKBitmap img)
        {
            return bitmapConverter.Convert(img);
        }

        public static SKColor ToSKColor(this PixColor color)
        {
            return new SKColor(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static PixColor ToPixColor(this SKColor color)
        {
            return new PixColor(color.Red, color.Green, color.Blue, color.Alpha);
        }
    }
}
