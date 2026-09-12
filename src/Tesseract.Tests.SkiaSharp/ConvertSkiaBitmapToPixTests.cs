using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using SkiaSharp;
using System;
using System.IO;

namespace Tesseract.Tests.SkiaSharp
{
    [TestFixture]
    public class ConvertSkiaBitmapToPixTests : AssertionHelper
    {
        // AppContext.BaseDirectory, not TestContext.CurrentContext.WorkDirectory (AnyUnit has
        // no TestContext equivalent) -- matches Tesseract.Tests's own TesseractTestBase.
        // AbsolutePath fix, same reasoning: this file is also compiled directly into the
        // AnyUnit-based wasm test runner, where the working directory is deliberately set to
        // a staged fixture root before tests run and AppContext.BaseDirectory resolves there
        // too (it's just the output directory either way).
        private static string TestFilePath(string path)
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", path);
        }

        [Test]
        public void Convert_SkiaBitmapToPix_Rgba8888()
        {
            var sourceFile = TestFilePath("Binarization/neo-32bit.png");
            var converter = new SkiaBitmapToPixConverter();
            using (var source = SKBitmap.Decode(sourceFile)) {
                using (var dest = converter.Convert(source)) {
                    Assert.That(dest.Depth, Is.EqualTo(32));
                    AssertAreEquivalent(source, dest);
                }
            }
        }

        [Test]
        public void Convert_SkiaBitmapToPix_Gray8()
        {
            var sourceFile = TestFilePath("Binarization/neo-8bit-grayscale.png");
            using (var decoded = SKBitmap.Decode(sourceFile))
            using (var source = decoded.Copy(SKColorType.Gray8)) {
                Assert.That(source, Is.Not.Null, "Could not normalise source fixture to Gray8.");
                var converter = new SkiaBitmapToPixConverter();
                using (var dest = converter.Convert(source)) {
                    Assert.That(dest.Depth, Is.EqualTo(8));
                    AssertAreEquivalent(source, dest);
                }
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Convert_PixToSkiaBitmap_32bpp(bool includeAlpha)
        {
            // Source Pix built via SkiaBitmapToPixConverter (SKBitmap.Decode), not
            // Pix.LoadFromFile -- deliberately avoids Leptonica's own PNG codec so this test
            // exercises only the two converters under test and runs identically under
            // browser-wasm, where Leptonica has none (see the codec-drop decision in the WASM
            // backlog plan).
            var sourceFile = TestFilePath("Binarization/neo-32bit.png");
            var toPix = new SkiaBitmapToPixConverter();
            using (var decoded = SKBitmap.Decode(sourceFile))
            using (var source = toPix.Convert(decoded)) {
                var converter = new PixToSkiaBitmapConverter();
                using (var dest = converter.Convert(source, includeAlpha)) {
                    Assert.That(dest.ColorType, Is.EqualTo(SKColorType.Rgba8888));
                    AssertAreEquivalent(dest, source, includeAlpha);
                }
            }
        }

        [Test]
        public void Convert_PixToSkiaBitmap_Gray8()
        {
            var sourceFile = TestFilePath("Binarization/neo-8bit-grayscale.png");
            var toPix = new SkiaBitmapToPixConverter();
            using (var decoded = SKBitmap.Decode(sourceFile))
            using (var gray = decoded.Copy(SKColorType.Gray8))
            using (var source = toPix.Convert(gray)) {
                Assert.That(source.Depth, Is.EqualTo(8));
                Assert.That(source.Colormap, Is.Null);
                var converter = new PixToSkiaBitmapConverter();
                using (var dest = converter.Convert(source)) {
                    Assert.That(dest.ColorType, Is.EqualTo(SKColorType.Gray8));
                    AssertAreEquivalent(dest, source, false);
                }
            }
        }

        // Mirrors ConvertBitmapToPixTests' own AssertAreEquivalent: a corner-pixel + metadata
        // spot check, not a full-image diff -- that's the existing project's convention for
        // these converter round-trip tests.
        private void AssertAreEquivalent(SKBitmap bmp, Pix pix, bool checkAlpha = false)
        {
            Assert.That(pix.Width, Is.EqualTo(bmp.Width));
            Assert.That(pix.Height, Is.EqualTo(bmp.Height));

            var height = pix.Height;
            var width = pix.Width;
            for (int y = 0; y < height; y += height) {
                for (int x = 0; x < width; x += width) {
                    PixColor sourcePixel = bmp.GetPixel(x, y).ToPixColor();
                    PixColor destPixel = GetPixel(pix, x, y);
                    if (checkAlpha) {
                        Assert.That(destPixel, Is.EqualTo(sourcePixel), $"Expected pixel at <{x},{y}> to be same in both source and dest.");
                    } else {
                        Assert.That(destPixel, Is.EqualTo(sourcePixel).Using<PixColor>((c1, c2) => (c1.Red == c2.Red && c1.Blue == c2.Blue && c1.Green == c2.Green) ? 0 : 1), $"Expected pixel at <{x},{y}> to be same in both source and dest.");
                    }
                }
            }
        }

        private unsafe PixColor GetPixel(Pix pix, int x, int y)
        {
            var pixDepth = pix.Depth;
            var pixData = pix.GetData();
            var pixLine = (uint*)pixData.Data + pixData.WordsPerLine * y;
            if (pixDepth == 8) {
                byte grayscale = (byte)PixData.GetDataByte(pixLine, x);
                return new PixColor(grayscale, grayscale, grayscale);
            } else if (pixDepth == 32) {
                return PixColor.FromRgba(PixData.GetDataFourByte(pixLine, x));
            } else {
                throw new ArgumentException(String.Format("Bit depth of {0} is not supported by this test.", pix.Depth), nameof(pix));
            }
        }
    }
}
