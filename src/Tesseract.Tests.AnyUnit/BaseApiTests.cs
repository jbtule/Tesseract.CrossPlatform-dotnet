using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Diagnostics;

namespace Tesseract.Tests
{
    [TestFixture]
    public class BaseApiTests : AssertionHelper
    {
        [Test]
        public void CanGetVersion()
        {
            var version = Interop.TessApi.BaseApiGetVersion();
            Assert.That(version, Does.StartWith(TestEnvironment.PinnedTesseractVersion));
        }
    }
}