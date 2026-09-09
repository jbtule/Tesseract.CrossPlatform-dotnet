using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tesseract.Tests
{
    /// <summary>
    /// Determines what action is taken when the test result doesn't match the expected (reference) result.
    /// </summary>
    public interface ITestDifferenceHandler
    {
        void Execute(string actualResultFilename, string expectedResultFilename);
    }

    /// <summary>
    /// Fails the test if the actual result file doesn't match the expected result (ignoring line ending type(s)).
    /// </summary>
    public class FailTestDifferenceHandler : ITestDifferenceHandler
    {
        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            if (File.Exists(expectedResultFilename))
            {
                var actualResult = TestUtils.NormaliseNewLine(File.ReadAllText(actualResultFilename));
                var expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                if (expectedResult != actualResult)
                {
                    Assert.Fail("Expected results to be \"{0}\" but was \"{1}\".", expectedResultFilename, actualResultFilename);
                }
            }
            else
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
            }
        }
    }

    /// <summary>
    /// Like <see cref="FailTestDifferenceHandler"/>, but decimal numbers in the output are
    /// compared with a tolerance instead of exactly -- everything else (recognized words,
    /// bounding box coordinates, TSV/XML structure) still has to match exactly.
    ///
    /// <remarks>
    /// Exists specifically for tesseract's LSTM confidence scores, which -- confirmed via
    /// real CI runs across all 5 platforms this repo builds for, not assumed -- drift by a
    /// fraction of a percentage point (observed max: 0.5) depending on the CPU/toolchain's
    /// floating-point codegen for the underlying matrix math (a vcpkg-triplet-specific
    /// thing, not simply "ARM vs x64" -- win-arm64 drifts along with the x64 platforms,
    /// while osx-arm64 and linux-arm64 match exactly). Recognized text and bounding boxes
    /// never differ, only these scores -- confirmed by diffing the real per-platform CI
    /// output, not guessed. A byte-exact comparison for confidence-bearing formats (TSV,
    /// ResultIterator's confidence attributes) would mean either maintaining separate golden
    /// files per platform forever, or accepting spurious failures that have nothing to do
    /// with a real regression; this handler expresses the actual invariant instead
    /// ("recognition is right, float noise in the last digit is expected").
    /// </remarks>
    /// </summary>
    public class ConfidenceTolerantTestDifferenceHandler : ITestDifferenceHandler
    {
        // Every decimal-point number in these fixtures is a confidence score (TSV's `conf`
        // column; ResultIterator's `confidence="NN.NN %"` attributes) -- every other numeric
        // field (bounding box coordinates, TSV's level/page/block/par/line/word indices) is
        // a plain integer, so a "has a decimal point" pattern isolates exactly the values
        // this handler exists to tolerate, without needing to know the specific file format.
        private static readonly Regex DecimalNumberPattern = new Regex(@"-?\d+\.\d+", RegexOptions.Compiled);

        private readonly double tolerance;

        public ConfidenceTolerantTestDifferenceHandler(double tolerance = 1.0)
        {
            this.tolerance = tolerance;
        }

        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            if (!File.Exists(expectedResultFilename))
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
                return;
            }

            var actualResult = TestUtils.NormaliseNewLine(File.ReadAllText(actualResultFilename));
            var expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));

            // Structural check first: mask every decimal number out, so a real content
            // change (a different recognized word, a shifted bounding box, a
            // different-shaped line) still fails exactly like FailTestDifferenceHandler
            // would -- only the *values* of the numbers get any leniency, not their
            // presence, count, or position.
            var actualMasked = DecimalNumberPattern.Replace(actualResult, "#");
            var expectedMasked = DecimalNumberPattern.Replace(expectedResult, "#");
            if (actualMasked != expectedMasked)
            {
                Assert.Fail(
                    "Expected results to be \"{0}\" but was \"{1}\" -- and not just by decimal-value drift (structure/text differs).",
                    expectedResultFilename, actualResultFilename);
            }

            var actualNumbers = DecimalNumberPattern.Matches(actualResult)
                .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToList();
            var expectedNumbers = DecimalNumberPattern.Matches(expectedResult)
                .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToList();

            for (int i = 0; i < actualNumbers.Count; i++)
            {
                var delta = Math.Abs(actualNumbers[i] - expectedNumbers[i]);
                if (delta > tolerance)
                {
                    Assert.Fail(
                        "Numeric value #{0} in \"{1}\" differs from \"{2}\" by {3:0.######} (tolerance {4}): expected {5} but was {6}.",
                        i, actualResultFilename, expectedResultFilename, delta, tolerance, expectedNumbers[i], actualNumbers[i]);
                }
            }
        }
    }

    /// <summary>
    /// Launches P4Merge to allow the user to update the reference if required
    /// 
    /// This is handy when updating the underlying tesseract engine dlls and/or tessdata as the output of a number of tests may 
    /// have changed due to internal changes in tesseract.
    /// </summary>
    public class P4MergeTestDifferenceHandler : ITestDifferenceHandler
    {
        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            var actualResult = TestUtils.NormaliseNewLine(File.ReadAllText(actualResultFilename));

            if (File.Exists(expectedResultFilename))
            {
                // Load the expected results and verify that they match
                var expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                if (expectedResult != actualResult)
                {
                    Console.WriteLine($"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\", launching merge tool.");
                    Merge(actualResultFilename, expectedResultFilename);

                    // User may have updated expected results, only fail if it's still different
                    expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                    if (expectedResult != actualResult)
                    {
                        Assert.Fail("Expected results to be \"{0}\" but was \"{1}\".", expectedResultFilename, actualResultFilename);
                    }
                }
            }
            else
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
            }
        }

        /// <summary>
        /// Attempts to merge actual into expected result file
        /// </summary>
        /// <param name="actualResultFileName">This is the file generated by the test</param>
        /// <param name="expectedResultFilename">This is the reference file which may be updated by the configured merge tool</param>
        private void Merge(string actualResultFileName, string expectedResultFilename)
        {
            // Note currently merge tool is hard coded and expected the command line "merge.exe %base %left %right %merged"
            TestUtils.Cmd("p4merge.exe", expectedResultFilename, actualResultFileName, expectedResultFilename, expectedResultFilename);
        }
    }
}
