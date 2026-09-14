using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tesseract.Tests.Leptonica
{
    [TestFixture]
    public class LoadFromFileTests : AssertionHelper
    {
        // Same convention Tesseract.Tests.SkiaSharp's own ConvertSkiaBitmapToPixTests.cs
        // uses (AppContext.BaseDirectory, not TesseractTestBase's TestFilePath - this
        // fixture doesn't inherit that base class): Data/ is copied next to the test
        // assembly's own output, and this file is also compiled directly into the
        // AnyUnit-based wasm test runner, where AppContext.BaseDirectory resolves to
        // the same staged fixture root.
        private static string TestFilePath(string path)
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", path);
        }

        // Was an empty "// TODO :)" body: it ran, "passed" trivially, and asserted
        // nothing - AnyUnit's own [ResultKind.NoError] honesty check (a test can't fake
        // "passed" without ever calling Assert) is what actually surfaced this as a
        // real, if minor, coverage gap rather than quietly staying "green" forever.
        // [RequiresImageCodecs]: JPEG decode goes through Leptonica, not available in
        // the browser-wasm native build (see that attribute's own remarks).
        [Test]
        [RequiresImageCodecs]
        public void CanLoadJpeg()
        {
            var jpegFile = TestFilePath("processing/w91frag.jpg");
            using (var pix = Pix.LoadFromFile(jpegFile)) {
                Assert.That(pix, Is.Not.Null);
                Assert.That(pix.Width, Is.GreaterThan(0));
                Assert.That(pix.Height, Is.GreaterThan(0));
            }
        }
    }
}
