using AnyUnit;
using AnyUnit.Run;
using AnyUnit.Style.Nunit;
using AnyUnit.Constraints;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tesseract.Tests
{
    // These handlers are plain helper objects (not [TestFixture]s), invoked from a
    // fixture's test method via TesseractTestBase.CheckResult - real NUnit's Assert.Fail
    // is fully static and callable from anywhere, but AnyUnit's Assert is an instance
    // property only a fixture gets (injected per-test by the harness; see AssertionHelper).
    // Assert.Fail(message) just throws AssertionException(message) internally either way,
    // so throwing it directly here is exactly equivalent, without needing an Assert
    // instance these classes have no way to obtain.

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
                    throw new AssertionException($"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\".");
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
                throw new AssertionException(
                    $"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\" -- and not just by decimal-value drift (structure/text differs).");
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
                    throw new AssertionException(
                        $"Numeric value #{i} in \"{actualResultFilename}\" differs from \"{expectedResultFilename}\" by {delta:0.######} (tolerance {tolerance}): expected {expectedNumbers[i]} but was {actualNumbers[i]}.");
                }
            }
        }
    }

    /// <summary>
    /// Like <see cref="FailTestDifferenceHandler"/>, but lines are sorted before comparing --
    /// for output whose line *order* isn't part of the invariant being tested, only which
    /// lines are present and what they say. Optionally, specific named lines (identified by
    /// their tab-separated first field, i.e. tesseract's variable name for CanPrintVariables)
    /// can be excluded from the comparison entirely on both sides -- for a *named, understood*
    /// set of differences, not a blanket tolerance.
    ///
    /// <remarks>
    /// Exists specifically for CanPrintVariables (tesseract's full registered-parameter dump).
    ///
    /// The ordering behavior: confirmed via a real diff against an actual (if unofficial,
    /// ad-hoc) alternate build -- of ~608 lines, ~580 were identical content in a different
    /// order, not different content. Tesseract registers each parameter via a static
    /// initializer scattered across many translation units; the C++ standard doesn't
    /// guarantee initialization order *across* TUs, so a different linker/toolchain can
    /// legitimately produce a different final registration order with zero behavioral
    /// difference.
    ///
    /// The named-exclusion behavior: exists for comparing a build with deliberately different
    /// compiled-in features (e.g. the browser-wasm native build, which builds with
    /// -DDISABLE_CURL=ON and -DGRAPHICS_DISABLED=ON, per the WASM backlog plan's codec-drop
    /// decision) against the desktop golden file. Tesseract only registers a parameter if the
    /// translation unit that declares it got compiled in at all, so an intentionally
    /// feature-reduced build is *missing* those parameters entirely, not just differently
    /// valued. Deliberately a named allow-list, not a blanket "tolerate any set difference":
    /// a real regression (a parameter that should exist silently disappearing, or a shared
    /// parameter's value changing) still fails, because only the specific, already-diagnosed,
    /// documented names below are excluded -- anything else missing or changed is a real,
    /// material difference and is reported as such.
    /// </remarks>
    /// </summary>
    public class UnorderedLinesTestDifferenceHandler : ITestDifferenceHandler
    {
        private readonly HashSet<string> ignorableVariableNames;

        public UnorderedLinesTestDifferenceHandler(IEnumerable<string>? ignorableVariableNames = null)
        {
            this.ignorableVariableNames = ignorableVariableNames != null
                ? new HashSet<string>(ignorableVariableNames, StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
        }

        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            if (!File.Exists(expectedResultFilename))
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
                return;
            }

            var actualLines = SortedLines(actualResultFilename);
            var expectedLines = SortedLines(expectedResultFilename);
            if (!actualLines.SequenceEqual(expectedLines, StringComparer.Ordinal))
            {
                throw new AssertionException(
                    $"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\" -- and not just by line order or a known/named difference (a material difference remains).");
            }
        }

        private List<string> SortedLines(string filename)
        {
            var text = TestUtils.NormaliseNewLine(File.ReadAllText(filename));
            return text.Split('\n')
                .Where(line => !string.IsNullOrEmpty(line))
                .Where(line => !ignorableVariableNames.Contains(VariableName(line)))
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToList();
        }

        private static string VariableName(string line)
        {
            var tabIndex = line.IndexOf('\t');
            return tabIndex >= 0 ? line.Substring(0, tabIndex) : line;
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
                        throw new AssertionException($"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\".");
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
