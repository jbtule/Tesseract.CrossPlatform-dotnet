using System;
using System.IO;
using System.Runtime.InteropServices;
using InteropDotNet;
using NUnit.Framework;

namespace Tesseract.Tests.SkiaSharp
{
    /// <summary>
    /// Points native library resolution at a pinned tesseract/leptonica build before any test
    /// runs, same convention (and same reasoning) as Tesseract.Tests's own GlobalTestSetup:
    /// TESSERACT_NATIVE_DIR env var first, then this repo's stage/&lt;rid&gt;/native/, then no
    /// override (default OS search) if neither exists. Duplicated rather than shared because
    /// this project deliberately doesn't reference Tesseract.Tests (see this project's own
    /// csproj comment).
    /// </summary>
    [SetUpFixture]
    internal class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void SetNativeSearchPath()
        {
            var explicitDir = Environment.GetEnvironmentVariable("TESSERACT_NATIVE_DIR");
            if (!string.IsNullOrEmpty(explicitDir) && Directory.Exists(explicitDir)) {
                LibraryLoader.CustomSearchPath = explicitDir;
                TestContext.Progress.WriteLine($"Using native libraries from TESSERACT_NATIVE_DIR: {explicitDir}");
                return;
            }

            var rid = RuntimeInformation.RuntimeIdentifier;
            var dir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            for (var i = 0; i < 12 && dir != null; i++, dir = dir.Parent) {
                if (File.Exists(Path.Combine(dir.FullName, "versions.env"))) break;
            }

            if (dir == null) {
                TestContext.Progress.WriteLine(
                    "Could not find repo root (versions.env) from the test working directory -- " +
                    "falling back to the OS's normal library search.");
                return;
            }

            var stagedDir = Path.Combine(dir.FullName, "stage", rid, "native");
            if (Directory.Exists(stagedDir)) {
                LibraryLoader.CustomSearchPath = stagedDir;
                TestContext.Progress.WriteLine($"Using native libraries from repo-pinned build: {stagedDir}");
                return;
            }

            TestContext.Progress.WriteLine(
                $"No TESSERACT_NATIVE_DIR set and no staged build found at \"{stagedDir}\" -- " +
                "falling back to the OS's normal library search.");
        }
    }
}
