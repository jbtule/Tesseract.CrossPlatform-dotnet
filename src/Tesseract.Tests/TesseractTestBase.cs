using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tesseract.Tests
{
    // Real NUnit's Assert/TestContext are fully static, callable from anywhere;
    // AnyUnit's Assert is an instance property a fixture only gets by inheriting
    // AssertionHelper (injected per-test by the harness) - so this shared base
    // (every concrete fixture in this suite derives from it) has to inherit it
    // too for Assert.* to resolve in any of them.
    public abstract class TesseractTestBase : AssertionHelper
    {
        /// <summary>
        /// Determines how test differences are handled
        /// </summary>
        static ITestDifferenceHandler testDifferenceHandler = new FailTestDifferenceHandler();

        protected static TesseractEngine CreateEngine(string lang = "eng", EngineMode mode = EngineMode.Default)
        {
            var datapath = DataPath;
            return new TesseractEngine(datapath, lang, mode);
        }

        protected static string DataPath
        {
            get {  return AbsolutePath("tessdata"); }
        }

        protected static string AbsolutePath(string relativePath)
        {
            // Real NUnit's TestContext.CurrentContext.WorkDirectory - AnyUnit has no
            // TestContext equivalent (no per-test ambient state at all beyond the
            // Assert/Log an AssertionHelper subclass gets). The test working directory
            // is just the output directory either way; AppContext.BaseDirectory is the
            // portable, framework-agnostic way to get it.
            return Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        #region File Helpers

        protected static string TestFilePath(string path)
        {
            var basePath = AbsolutePath("Data");

            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        protected static string TestResultPath(string path)
        {
            var basePath = Path.Combine(TestEnvironment.ProjectSourceRoot, "Results");

            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        protected static string TestResultRunDirectory(string path)
        {
            var runPath = AbsolutePath(
                String.Format("Runs/{0:yyyyMMddTHHmmss}", TestRun.Current.StartedAt)
            );
            var testResultRunDirectory = Path.Combine(runPath, path);        
            Directory.CreateDirectory(testResultRunDirectory);

            return testResultRunDirectory;
        }
        
        protected static string TestResultRunFile(string path)
        {
            // Path.GetDirectoryName returns null only for a root path (e.g. "C:\") or an
            // empty input -- never for the relative "subdir/file.ext"-shaped paths this is
            // actually called with, but it's nullable regardless. Empty string is the
            // correct fallback: "no subdirectory", same as an already-empty GetDirectoryName
            // result for a bare filename.
            var testRunDirectory = TestResultRunDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            var testFileName = Path.GetFileName(path);

            return Path.GetFullPath(Path.Combine(testRunDirectory, testFileName));
        }

        protected static Pix LoadTestPix(string filename)
        {
            var testFilename = TestFilePath(filename);
            return Pix.LoadFromFile(testFilename);
        }

        protected static void CheckResult(string resultFilename)
        {
            CheckResult(resultFilename, testDifferenceHandler);
        }

        /// <summary>
        /// Same as <see cref="CheckResult(string)"/>, but with an explicit
        /// <see cref="ITestDifferenceHandler"/> override -- for the handful of golden
        /// fixtures (confidence-score-bearing output) that need
        /// <see cref="ConfidenceTolerantTestDifferenceHandler"/> instead of the default
        /// exact-match <see cref="FailTestDifferenceHandler"/>.
        /// </summary>
        protected static void CheckResult(string resultFilename, ITestDifferenceHandler handler)
        {
            var actualResultFilename = TestResultRunFile(resultFilename);
            var expectedResultFilename = TestResultPath(resultFilename);

            handler.Execute(actualResultFilename, expectedResultFilename);
        }

        #endregion File Helpers
    }
}
