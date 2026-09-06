using System;

namespace Tesseract.Interop
{
    /// <summary>
    /// Description of Constants.
    /// </summary>
    internal static class Constants
    {
        // Generic (unversioned) names: FixUpLibraryName appends the right
        // platform extension and "lib" prefix, so this resolves to
        // libleptonica.so / libleptonica.dylib / leptonica.dll -- names
        // that exist as unversioned symlinks/aliases regardless of the
        // exact Leptonica/Tesseract release in use, rather than pinning to
        // one specific bundled build's exact versioned filename
        // (previously "leptonica-1.82.0"/"tesseract50", tied to whatever
        // binaries happened to be bundled directly in this package -- a
        // real problem for anyone supplying their own build via
        // CustomSearchPath or a NuGet runtime package, since no search
        // path fixes a flatly wrong filename).
        public const string LeptonicaDllName = "leptonica";
        public const string TesseractDllName = "tesseract";
        
        // tesseract uses an int to represent true false values.
        public const int TRUE = 1;
        public const int FALSE = 0;
    }
}