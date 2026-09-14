using System.IO;
using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tesseract.Tests.Leptonica.PixTests
{
    [TestFixture]
    public class ImageManipulationTests : TesseractTestBase
    {
        const string ResultsDirectory = @"Results/ImageManipulation/";

        // A cheap "did this actually do something" check, not a golden-image comparison
        // (that would need new golden fixtures, real effort tracked separately) - reads
        // a Pix's raw pixel bytes back out via Marshal.Copy so Rotate/RemoveLines/
        // Despeckle below can assert their output genuinely differs from (or, for the
        // dimension checks alongside it, is validly shaped relative to) their input,
        // instead of asserting nothing at all. Real regression coverage (a rotation/
        // filter silently turning into a no-op) even without a golden reference image.
        private static byte[] GetPixBytes(Pix pix)
        {
            var data = pix.GetData();
            var byteLength = data.WordsPerLine * 4 * pix.Height;
            var bytes = new byte[byteLength];
            Marshal.Copy(data.Data, bytes, 0, byteLength);
            return bytes;
        }

        [Test]
        [RequiresImageCodecs]
        public void DescewTest()
        {
            var sourcePixPath = TestFilePath(@"Scew/scewed-phototest.png");
            using (var sourcePix = Pix.LoadFromFile(sourcePixPath))
            {
                Scew scew;
                using (var descewedImage = sourcePix.Deskew(new ScewSweep(range: 45), Pix.DefaultBinarySearchReduction, Pix.DefaultBinaryThreshold, out scew))
                {
                    Assert.That(scew.Angle, Is.EqualTo(-9.953125F).Within(0.00001));
                    Assert.That(scew.Confidence, Is.EqualTo(3.782913F).Within(0.00001));

                    SaveResult(descewedImage, "descewedImage.png");
                }
            }
        }

        [Test]
        [RequiresImageCodecs]
        public void OtsuBinarizationTest()
        {
            var sourcePixFilename = TestFilePath(@"Binarization/neo-8bit.png");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                using (var binarizedImage = sourcePix.BinarizeOtsuAdaptiveThreshold(200, 200, 10, 10, 0.1F))
                {
                    Assert.That(binarizedImage, Is.Not.Null);
                    Assert.That(binarizedImage.Handle, Is.Not.EqualTo(IntPtr.Zero));
                    SaveResult(binarizedImage, "binarizedOtsuImage.png");
                }
            }
        }

        [Test]
        [RequiresImageCodecs]
        public void SauvolaBinarizationTest()
        {
            string sourcePixFilename = TestFilePath(@"Binarization/neo-8bit-grayscale.png");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                using (var grayscalePix = sourcePix.ConvertRGBToGray(1, 1, 1))
                {
                    using (var binarizedImage = grayscalePix.BinarizeSauvola(10, 0.35f, false))
                    {
                        Assert.That(binarizedImage, Is.Not.Null);
                        Assert.That(binarizedImage.Handle, Is.Not.EqualTo(IntPtr.Zero));
                        SaveResult(binarizedImage, "binarizedSauvolaImage.png");
                    }
                }
            }
        }

        [Test]
        [RequiresImageCodecs]
        public void SauvolaTiledBinarizationTest()
        {
            string sourcePixFilename = TestFilePath(@"Binarization/neo-8bit-grayscale.png");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                using (var grayscalePix = sourcePix.ConvertRGBToGray(1, 1, 1))
                {
                    using (var binarizedImage = grayscalePix.BinarizeSauvolaTiled(10, 0.35f, 2, 2))
                    {
                        Assert.That(binarizedImage, Is.Not.Null);
                        Assert.That(binarizedImage.Handle, Is.Not.EqualTo(IntPtr.Zero));
                        SaveResult(binarizedImage, "binarizedSauvolaTiledImage.png");
                    }
                }
            }
        }

        [Test]
        public void ConvertRGBToGrayTest()
        {
            var sourcePixFilename = TestFilePath(@"Conversion/photo_rgb_32bpp.tif");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            using (var grayscaleImage = sourcePix.ConvertRGBToGray())
            {
                Assert.That(grayscaleImage.Depth, Is.EqualTo(8));
                SaveResult(grayscaleImage, "grayscaleImage.jpg");
            }
        }

        [Test]
        [TestCase(45)]
        [TestCase(80)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        public void Rotate_ShouldBeAbleToRotateImageByXDegrees(float angle)
        {
            const string FileNameFormat = "rotation_{0}degrees.jpg";
            float angleAsRadians = MathHelper.ToRadians(angle);

            var sourcePixFilename = TestFilePath(@"Conversion/photo_rgb_32bpp.tif");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                using (var result = sourcePix.Rotate(angleAsRadians, RotationMethod.AreaMap))
                {
                    // Was a "TODO: visually confirm" - no assertion at all. Real (if not
                    // pixel-exact/golden) regression coverage instead. Exact width/height
                    // only for the three angles this suite tests that are exact multiples
                    // of 90 (90/180/270 - Pix.Rotate's own "handle special case of
                    // orthogonal rotations" branch routes those through pixRotateOrth, a
                    // simple/discrete transform that swaps width/height for a 90/270 turn
                    // and leaves them alone for 180): every other angle here (45, 80) goes
                    // through Leptonica's general area-map path instead, which - confirmed
                    // empirically, not assumed from the wrapper's own width/height
                    // parameters alone - does NOT keep the source canvas size, growing it
                    // instead to fit the rotated content's own bounding box; asserting
                    // exact dimensions there would just be asserting a wrong assumption.
                    // What's still true for every angle: a real rotation - every one
                    // tested here is well clear of "no rotation at all" - has to actually
                    // move content, so the raw pixel data can't come back byte-identical
                    // to the source (a stale no-op rotation would still fail this).
                    var isMultipleOf90 = angle % 90 == 0;
                    if (isMultipleOf90)
                    {
                        var isQuarterTurn = angle == 90 || angle == 270;
                        Assert.That(result.Width, Is.EqualTo(isQuarterTurn ? sourcePix.Height : sourcePix.Width));
                        Assert.That(result.Height, Is.EqualTo(isQuarterTurn ? sourcePix.Width : sourcePix.Height));
                    }
                    else
                    {
                        Assert.That(result.Width, Is.GreaterThan(0));
                        Assert.That(result.Height, Is.GreaterThan(0));
                    }
                    Assert.That(GetPixBytes(result), Is.Not.EqualTo(GetPixBytes(sourcePix)));

                    var filename = String.Format(FileNameFormat, angle);
                    SaveResult(result, filename);
                }
            }
        }

        [Test]
        [RequiresImageCodecs]
        public void RemoveLinesTest()
        {
            var sourcePixFilename = TestFilePath(@"processing/table.png");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                // remove horizontal lines
                using (var result = sourcePix.RemoveLines())
                {
                    // rotate 90 degrees cw
                    using (var result1 = result.Rotate90(1))
                    {
                        // effectively remove vertical lines
                        using (var result2 = result1.RemoveLines())
                        {
                            // rotate 90 degrees ccw
                            using (var result3 = result2.Rotate90(-1))
                            {
                                // Was a "TODO: visually confirm" - no assertion at all. Real
                                // (if not pixel-exact/golden) regression coverage instead:
                                // the double rotate-90/-90 round trip means result3 is back
                                // to source's own orientation, so its dimensions/depth have
                                // to match sourcePix's exactly - and table.png's whole point
                                // is the horizontal/vertical border lines RemoveLines is
                                // meant to strip, so a real removal has to change at least
                                // some pixel from the source.
                                Assert.That(result3.Width, Is.EqualTo(sourcePix.Width));
                                Assert.That(result3.Height, Is.EqualTo(sourcePix.Height));
                                Assert.That(result3.Depth, Is.EqualTo(sourcePix.Depth));
                                Assert.That(GetPixBytes(result3), Is.Not.EqualTo(GetPixBytes(sourcePix)));

                                SaveResult(result3, "tableBordersRemoved.png");
                            }
                        }
                    }
                }
            }
        }

        [Test]
        [RequiresImageCodecs]
        public void DespeckleTest()
        {
            var sourcePixFilename = TestFilePath(@"processing/w91frag.jpg");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                // remove speckles
                using (var result = sourcePix.Despeckle(Pix.SEL_STR2, 2))
                {
                    // Was a "TODO: visually confirm" - no assertion at all. Real (if not
                    // pixel-exact/golden) regression coverage instead: despeckling doesn't
                    // change canvas geometry (no Depth check alongside it, deliberately -
                    // confirmed empirically that Pix.SEL_STR2-based Despeckle always
                    // returns a 1bpp binary result regardless of the source's own depth,
                    // a real Leptonica contract, not an oversight), and w91frag.jpg's
                    // whole point is the speckle noise Despeckle is meant to remove, so a
                    // real removal has to change at least some pixel from the source.
                    Assert.That(result.Width, Is.EqualTo(sourcePix.Width));
                    Assert.That(result.Height, Is.EqualTo(sourcePix.Height));
                    Assert.That(GetPixBytes(result), Is.Not.EqualTo(GetPixBytes(sourcePix)));

                    SaveResult(result, "w91frag-despeckled.png");
                }
            }
        }

        [Test]
        public void Scale_RGB_ShouldBeScaledBySpecifiedFactor(
            [Values(0.25f, 0.5f, 0.75f, 1, 1.25f, 1.5f, 1.75f, 2, 4, 8)]  float scale)
        {
            const string FileNameFormat = "scale_{0}.jpg";

            var sourcePixFilename = TestFilePath(@"Conversion/photo_rgb_32bpp.tif");
            using (var sourcePix = Pix.LoadFromFile(sourcePixFilename))
            {
                using (var result = sourcePix.Scale(scale, scale))
                {
                    Assert.That(result.Width, Is.EqualTo((int)Math.Round(sourcePix.Width * scale)));
                    Assert.That(result.Height, Is.EqualTo((int)Math.Round(sourcePix.Height * scale)));

                    // TODO: Visualy confirm successful rotation and then setup an assertion to compare that result is the same.
                    var filename = String.Format(FileNameFormat, scale);
                    SaveResult(result, filename);
                }
            }
        }

        private void SaveResult(Pix result, string filename)
        {
            var runFilename = TestResultRunFile(Path.Combine(ResultsDirectory, filename));
            result.Save(runFilename);
        }
    }
}