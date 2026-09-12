using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tesseract.Tests.ResultIteratorTests
{
    [TestFixture]
    [RequiresImageCodecs] // SetUp loads Ocr/blank.tif
    public class OfAnEmptyPixTests : TesseractTestBase
    {
        // Non-nullable, null-forgiving default: always assigned by Init() before any
        // test runs (NUnit's own SetUp/TearDown contract), so every real usage site is
        // safe without a null check -- the alternative (a nullable property) would just
        // push a spurious "possibly null" warning onto every one of those instead.
        private TesseractEngine Engine { get; set; } = null!;
        private Pix EmptyPix { get; set; } = null!;

        [SetUp]
        public void Init()
        {
            Engine = CreateEngine();
            EmptyPix = LoadTestPix("Ocr/blank.tif");
        }

        [TearDown]
        public void Dispose()
        {
            if (EmptyPix != null) {
                EmptyPix.Dispose();
                EmptyPix = null!;
            }

            if (Engine != null) {
                Engine.Dispose();
                Engine = null!;
            }
        }

        #region Tests

        [Theory]
        public void GetTextReturnNullForEachLevel(PageIteratorLevel level)
        {
            using (var page = Engine.Process(EmptyPix)) {
                using (var iter = page.GetIterator()) {
                    Assert.That(iter.GetText(level), Is.Null);
                }
            }
        }

        #endregion Tests
    }
}