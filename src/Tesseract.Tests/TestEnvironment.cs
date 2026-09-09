using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InteropDotNet;
using NUnit.Framework;

namespace Tesseract.Tests
{
    /// <summary>
    /// Locates repo-relative paths and the pinned tesseract version from wherever the test
    /// assembly happens to be running, without hardcoding a fixed number of "../.." hops --
    /// a fixed hop count broke twice already (once when this project absorbed
    /// Tesseract.NetCore31Tests, once historically) as soon as the directory depth changed.
    /// Walks up from the test run's working directory instead, looking for a distinctive
    /// marker at each level.
    /// </summary>
    internal static class TestEnvironment
    {
        private static readonly Lazy<string> repoRoot = new Lazy<string>(
            () => FindAncestor(TestContext.CurrentContext.WorkDirectory,
                d => File.Exists(Path.Combine(d, "versions.env"))));

        private static readonly Lazy<string> projectSourceRoot = new Lazy<string>(
            () => FindAncestor(TestContext.CurrentContext.WorkDirectory,
                d => Directory.Exists(Path.Combine(d, "Results")) && Directory.Exists(Path.Combine(d, "Data"))));

        private static readonly Lazy<string> pinnedTesseractVersion = new Lazy<string>(ComputePinnedVersion);

        /// <summary>
        /// The outer tesseract-nuget-platforms repo root (identified by versions.env, the
        /// single source of truth for what tesseract version this repo builds/ships).
        /// </summary>
        public static string RepoRoot => repoRoot.Value;

        /// <summary>
        /// This project's own source directory (identified by its Results/ and Data/
        /// subfolders) -- distinct from the build output directory the tests actually run
        /// from, since TestResultPath below deliberately reads/writes golden fixture files
        /// in source, not the copied build output.
        /// </summary>
        public static string ProjectSourceRoot => projectSourceRoot.Value;

        /// <summary>
        /// The tesseract version this repo is pinned to build (versions.env's
        /// PACKAGE_VERSION, first 3 segments -- the 4th is this repo's own packaging
        /// revision, not part of the tesseract version). Real tests that assert on the
        /// exact tesseract version (CanGetVersion) should check against this rather than a
        /// hardcoded literal, so they track a version bump automatically instead of quietly
        /// going stale.
        /// </summary>
        public static string PinnedTesseractVersion => pinnedTesseractVersion.Value;

        private static string FindAncestor(string start, Func<string, bool> isMatch, int maxLevels = 12)
        {
            var dir = new DirectoryInfo(start);
            for (var i = 0; i < maxLevels && dir != null; i++, dir = dir.Parent)
            {
                if (isMatch(dir.FullName))
                    return dir.FullName;
            }
            throw new DirectoryNotFoundException(
                $"Could not find an ancestor of \"{start}\" matching the expected marker within {maxLevels} levels.");
        }

        private static string ComputePinnedVersion()
        {
            var versionsEnvPath = Path.Combine(RepoRoot, "versions.env");
            var line = File.ReadAllLines(versionsEnvPath).FirstOrDefault(l => l.StartsWith("PACKAGE_VERSION="));
            if (line == null)
                throw new InvalidOperationException($"{versionsEnvPath} has no PACKAGE_VERSION= line.");

            var full = line.Substring("PACKAGE_VERSION=".Length).Trim();
            // <tesseract-version>.<packaging-revision> -- only the first 3 segments are the
            // actual tesseract version; the 4th is this repo's own packaging-only revision
            // (see versions.env's own comment).
            return string.Join(".", full.Split('.').Take(3));
        }
    }

    /// <summary>
    /// Points the wrapper's native library resolution at a specific tesseract/leptonica
    /// build before any test runs, so results are reproducible against a known build
    /// instead of silently drifting with whatever happens to be on the dev machine's
    /// $PATH/Homebrew/apt (which is what caused a real batch of golden-fixture and
    /// CanGetVersion failures against tesseract 5.5.3 vs. this repo's pinned 5.5.2).
    ///
    /// Resolution order: TESSERACT_NATIVE_DIR env var (explicit override, what CI sets to
    /// the native build it just produced for that job's RID) -- then this repo's own
    /// stage/&lt;rid&gt;/native/ convention (populated by scripts/build-native.sh/.ps1; a
    /// dev who's run that locally gets pinned-version behavior with zero extra setup) --
    /// then, if neither exists, no override at all: LibraryLoader falls through to its
    /// normal default resolution (whatever's on the system), same as it always has.
    /// </summary>
    [SetUpFixture]
    internal class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void SetNativeSearchPath()
        {
            var explicitDir = Environment.GetEnvironmentVariable("TESSERACT_NATIVE_DIR");
            if (!string.IsNullOrEmpty(explicitDir) && Directory.Exists(explicitDir))
            {
                LibraryLoader.CustomSearchPath = explicitDir;
                TestContext.Progress.WriteLine($"Using native libraries from TESSERACT_NATIVE_DIR: {explicitDir}");
                return;
            }

            var rid = RuntimeInformation.RuntimeIdentifier;
            var stagedDir = Path.Combine(TestEnvironment.RepoRoot, "stage", rid, "native");
            if (Directory.Exists(stagedDir))
            {
                LibraryLoader.CustomSearchPath = stagedDir;
                TestContext.Progress.WriteLine($"Using native libraries from repo-pinned build: {stagedDir}");
                return;
            }

            TestContext.Progress.WriteLine(
                $"No TESSERACT_NATIVE_DIR set and no staged build found at \"{stagedDir}\" " +
                "(run scripts/build-native.sh/.ps1 for this RID to produce one) -- falling back " +
                "to the OS's normal library search. CanGetVersion and the golden-fixture tests " +
                "may fail if what's found there doesn't match this repo's pinned tesseract version.");
        }
    }
}
