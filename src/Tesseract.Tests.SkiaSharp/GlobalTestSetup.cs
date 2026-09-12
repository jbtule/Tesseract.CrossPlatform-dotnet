using System;
using System.IO;
using System.Runtime.InteropServices;
using InteropDotNet;
using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;

namespace Tesseract.Tests.SkiaSharp
{
    /// <summary>
    /// Points native library resolution at a pinned tesseract/leptonica build before any test
    /// runs, same convention (and same reasoning) as Tesseract.Tests's own GlobalTestSetup:
    /// TESSERACT_NATIVE_DIR env var first, then this repo's stage/&lt;rid&gt;/native/, then no
    /// override (default OS search) if neither exists. Duplicated rather than shared because
    /// this project deliberately doesn't reference Tesseract.Tests (see this project's own
    /// csproj comment).
    ///
    /// Ported from real NUnit: [SetUpFixture]/[OneTimeSetUp] get no Assert/Log injected by
    /// AnyUnit unless the fixture itself implements IAssertionHelper (unlike a per-test
    /// fixture) - inheriting AssertionHelper here gets a real Log; TestContext.Progress.
    /// WriteLine -> Log.WriteLine, TestContext.CurrentContext.WorkDirectory -> AppContext.
    /// BaseDirectory (see Tesseract.Tests's own TesseractTestBase for the identical fix,
    /// same reasoning).
    /// </summary>
    [SetUpFixture]
    internal class GlobalTestSetup : AssertionHelper
    {
        [OneTimeSetUp]
        public void SetNativeSearchPath()
        {
            var explicitDir = Environment.GetEnvironmentVariable("TESSERACT_NATIVE_DIR");
            if (!string.IsNullOrEmpty(explicitDir) && Directory.Exists(explicitDir)) {
                LibraryLoader.CustomSearchPath = explicitDir;
                Log.WriteLine($"Using native libraries from TESSERACT_NATIVE_DIR: {explicitDir}");
                return;
            }

            var rid = RuntimeInformation.RuntimeIdentifier;
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var i = 0; i < 12 && dir != null; i++, dir = dir.Parent) {
                if (File.Exists(Path.Combine(dir.FullName, "versions.env"))) break;
            }

            if (dir == null) {
                Log.WriteLine(
                    "Could not find repo root (versions.env) from the test working directory -- " +
                    "falling back to the OS's normal library search.");
                return;
            }

            var stagedDir = Path.Combine(dir.FullName, "stage", rid, "native");
            if (Directory.Exists(stagedDir)) {
                LibraryLoader.CustomSearchPath = stagedDir;
                Log.WriteLine($"Using native libraries from repo-pinned build: {stagedDir}");
                return;
            }

            Log.WriteLine(
                $"No TESSERACT_NATIVE_DIR set and no staged build found at \"{stagedDir}\" -- " +
                "falling back to the OS's normal library search.");
        }
    }
}
