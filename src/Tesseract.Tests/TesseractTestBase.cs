using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tesseract.Tests
{
    public abstract class TesseractTestBase
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
            return Path.Combine(TestContext.CurrentContext.WorkDirectory, relativePath);
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
            var testRunDirectory = TestResultRunDirectory(Path.GetDirectoryName(path));
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
