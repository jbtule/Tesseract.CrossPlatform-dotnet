using System;
using System.Reflection;
using AnyUnit;
using AnyUnit.Style.Nunit;

namespace Tesseract.Tests
{
    /// <summary>
    /// Marks a test (or a whole fixture, if every test in it needs this) as depending on
    /// image codec support (TIFF/PNG/JPEG/GIF decode via Leptonica) -- skipped, not failed,
    /// under browser-wasm.
    ///
    /// <remarks>
    /// The browser-wasm native build doesn't compile in any image codec library at all (see
    /// the WASM backlog plan's codec-drop decision): no libtiff port exists for Emscripten at
    /// all, and while libpng/libjpeg/giflib prebuilt ports do exist and link cleanly, actually
    /// linking them in was found, empirically, to introduce a real, currently-unexplained
    /// runtime hang unrelated to codec use itself (reproduced twice, deterministically, with
    /// no exception or error surfaced) -- not something to casually accept as a v1 tradeoff.
    /// Every one of these tests loads a real TIFF/PNG fixture file via Pix.LoadFromFile (or
    /// TesseractEngine.Process on one), so all of them are expected to fail under wasm today,
    /// for this specific, understood, already-diagnosed reason -- not a real regression.
    ///
    /// This is a placeholder until Phase 3 of the WASM backlog plan (Pix.FromRawPixelData,
    /// accepting decoder-agnostic raw pixel bytes from whatever the consumer already uses --
    /// SkiaSharp, ImageSharp, the browser's own decoder, anything) actually ships; once
    /// wasm-side test fixtures can be constructed that way instead of by file load, the
    /// equivalent coverage can run for real there too, and this attribute should come off
    /// whichever tests get rewritten to use it.
    ///
    /// Ported from real NUnit's ITestAction (NUnit.Framework.Interfaces.ITest,
    /// ActionTargets.Test/Suite) to AnyUnit.Style.Nunit's own, narrower ITestAction:
    /// MethodInfo instead of ITest, no ActionTargets (AnyUnit only ever runs this
    /// around individual tests, never once per fixture - see that interface's own
    /// remarks). Assert.Ignore is real NUnit's static entry point for throwing
    /// NUnit's own ignore signal from anywhere; AnyUnit has no static Assert facade
    /// (Assert is an instance property a fixture gets via AssertionHelper - this
    /// attribute isn't one), so it throws AnyUnit's IgnoreException directly instead,
    /// which is all Assert.Ignore does under the hood anyway.
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequiresImageCodecsAttribute : Attribute, ITestAction
    {
        public void BeforeTest(MethodInfo method)
        {
            if (OperatingSystem.IsBrowser())
            {
                throw new IgnoreException(
                    "Requires image codec support (TIFF/PNG/JPEG/GIF via Leptonica), not " +
                    "available in the browser-wasm native build -- see RequiresImageCodecsAttribute's own remarks.");
            }
        }

        public void AfterTest(MethodInfo method)
        {
        }
    }
}
