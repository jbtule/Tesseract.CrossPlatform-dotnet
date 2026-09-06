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
        /// This search path should not include the platform component (e.g. "x86", "x64", "arm64") as this will
        /// automatically be appended to the string based on the detected platform. On .NET Core/.NET 5+ the
        /// platform component reflects the actual process architecture (so an Apple Silicon or Arm64 Linux
        /// process gets "arm64", not "x64"); on classic .NET Framework it is always "x86" or "x64".
        /// </remarks>
        public static string CustomSearchPath
        {
            get { return LibraryLoader.Instance.CustomSearchPath; }
            set { LibraryLoader.Instance.CustomSearchPath = value; }
        }
    }
}
