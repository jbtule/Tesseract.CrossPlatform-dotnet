using InteropDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tesseract
{
    public static class TesseractEnviornment
    {
        /// <summary>
        /// Gets or sets a search path that will be checked first when attempting to load the Tesseract and Leptonica dlls.
        /// </summary>
        /// <remarks>
        /// The library is looked for directly in this folder first (i.e. "&lt;CustomSearchPath&gt;/libtesseract.so").
        /// If it isn't found there, a platform component ("x86", "x64", "arm", or "arm64" -- reflecting the
        /// actual process architecture) is appended and checked as a fallback, for compatibility with the
        /// legacy convention of nesting binaries by platform underneath this path.
        /// </remarks>
        public static string CustomSearchPath
        {
            get { return LibraryLoader.CustomSearchPath; }
            set { LibraryLoader.CustomSearchPath = value; }
        }
    }
}
