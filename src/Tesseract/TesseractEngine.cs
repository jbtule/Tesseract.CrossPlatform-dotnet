using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using Tesseract.Internal;
using Tesseract.Interop;

namespace Tesseract
{
    /// <summary>
    /// The tesseract OCR engine.
    /// </summary>
    public class TesseractEngine : DisposableBase
    {
        private static readonly TraceSource trace = new TraceSource("Tesseract");

        private NativeHandle handle;

        private int processCount = 0;

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> using the <see cref="EngineMode.Default"/> mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        public TesseractEngine(string datapath, string language)
            : this(datapath, language, EngineMode.Default, new string[0], new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="configFile"/>
        /// using the <see cref="EngineMode.Default">Default Engine Mode</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// <para>
        /// Note: That the config files MUST be encoded without the BOM using unix end of line characters.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="configFile">
        /// An optional tesseract configuration file that is encoded using UTF8 without BOM
        /// with Unix end of line characters you can use an advanced text editor such as Notepad++ to accomplish this.
        /// </param>
        public TesseractEngine(string datapath, string language, string configFile)
            : this(datapath, language, EngineMode.Default, configFile != null ? new[] { configFile } : new string[0], new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="configFiles"/>
        /// using the <see cref="EngineMode.Default">Default Engine Mode</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="configFiles">
        /// An optional sequence of tesseract configuration files to load, encoded using UTF8 without BOM
        /// with Unix end of line characters you can use an advanced text editor such as Notepad++ to accomplish this.
        /// </param>
        public TesseractEngine(string datapath, string language, IEnumerable<string> configFiles)
            : this(datapath, language, EngineMode.Default, configFiles, new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="engineMode"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="engineMode">The <see cref="EngineMode"/> value to use when initialising the tesseract engine.</param>
        public TesseractEngine(string datapath, string language, EngineMode engineMode)
            : this(datapath, language, engineMode, new string[0], new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="engineMode"/> and <paramref name="configFile"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// <para>
        /// Note: That the config files MUST be encoded without the BOM using unix end of line characters.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="engineMode">The <see cref="EngineMode"/> value to use when initialising the tesseract engine.</param>
        /// <param name="configFile">
        /// An optional tesseract configuration file that is encoded using UTF8 without BOM
        /// with Unix end of line characters you can use an advanced text editor such as Notepad++ to accomplish this.
        /// </param>
        public TesseractEngine(string datapath, string language, EngineMode engineMode, string configFile)
            : this(datapath, language, engineMode, configFile != null ? new[] { configFile } : new string[0], new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="engineMode"/> and <paramref name="configFiles"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="engineMode">The <see cref="EngineMode"/> value to use when initialising the tesseract engine.</param>
        /// <param name="configFiles">
        /// An optional sequence of tesseract configuration files to load, encoded using UTF8 without BOM
        /// with Unix end of line characters you can use an advanced text editor such as Notepad++ to accomplish this.
        /// </param>
        public TesseractEngine(string datapath, string language, EngineMode engineMode, IEnumerable<string> configFiles)
            : this(datapath, language, engineMode, configFiles, new Dictionary<string, object>(), false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TesseractEngine"/> with the specified <paramref name="engineMode"/> and <paramref name="configFiles"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="datapath"/> parameter should point to the directory that contains the 'tessdata' folder
        /// for example if your tesseract language data is installed in <c>C:\Tesseract\tessdata</c> the value of datapath should
        /// be <c>C:\Tesseract</c>. Note that tesseract will use the value of the <c>TESSDATA_PREFIX</c> environment variable if defined,
        /// effectively ignoring the value of <paramref name="datapath"/> parameter.
        /// </para>
        /// </remarks>
        /// <param name="datapath">The path to the parent directory that contains the 'tessdata' directory, ignored if the <c>TESSDATA_PREFIX</c> environment variable is defined.</param>
        /// <param name="language">The language to load, for example 'eng' for English.</param>
        /// <param name="engineMode">The <see cref="EngineMode"/> value to use when initialising the tesseract engine.</param>
        /// <param name="configFiles">
        /// An optional sequence of tesseract configuration files to load, encoded using UTF8 without BOM
        /// with Unix end of line characters you can use an advanced text editor such as Notepad++ to accomplish this.
        /// </param>
        /// <param name="initialOptions">An optional set of variable name/value pairs to set as part of engine initialisation.</param>
        /// <param name="setOnlyNonDebugVariables">If <c>true</c>, only non-debug variables may be set using <paramref name="initialOptions"/> during initialisation.</param>
        public TesseractEngine(string datapath, string language, EngineMode engineMode, IEnumerable<string> configFiles, IDictionary<string, object> initialOptions, bool setOnlyNonDebugVariables)
        {
            Guard.RequireNotNullOrEmpty("language", language);

            DefaultPageSegMode = PageSegMode.Auto;
            handle = new NativeHandle(Interop.TessApi.BaseApiCreate());

            Initialise(datapath, language, engineMode, configFiles, initialOptions, setOnlyNonDebugVariables);
        }

        public string Version
        {
            get
            {
                // Get version doesn't work for x64, might be compilation related for now just
                // return constant so we don't crash.
                return TessApi.BaseApiGetVersion();
            }
        }

        internal NativeHandle Handle
        {
            get { return handle; }
        }

        /// <summary>
        /// Processes the specific image.
        /// </summary>
        /// <remarks>
        /// You can only have one result iterator open at any one time.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="pageSegMode">The page layout analyasis method to use.</param>
        public Page Process(Pix image, PageSegMode? pageSegMode = null)
        {
            return Process(image, null, new Rect(0, 0, image.Width, image.Height), pageSegMode);
        }

        /// <summary>
        /// Processes a specified region in the image using the specified page layout analysis mode.
        /// </summary>
        /// <remarks>
        /// You can only have one result iterator open at any one time.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="region">The image region to process.</param>
        /// <param name="pageSegMode">The page layout analyasis method to use.</param>
        /// <returns>A result iterator</returns>
        public Page Process(Pix image, Rect region, PageSegMode? pageSegMode = null)
        {
            return Process(image, null, region, pageSegMode);
        }

        /// <summary>
        /// Processes the specific image.
        /// </summary>
        /// <remarks>
        /// You can only have one result iterator open at any one time.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="inputName">Sets the input file's name, only needed for training or loading a uzn file.</param>
        /// <param name="pageSegMode">The page layout analyasis method to use.</param>
        public Page Process(Pix image, string inputName, PageSegMode? pageSegMode = null)
        {
            return Process(image, inputName, new Rect(0, 0, image.Width, image.Height), pageSegMode);
        }

        /// <summary>
        /// Processes a specified region in the image using the specified page layout analysis mode.
        /// </summary>
        /// <remarks>
        /// You can only have one result iterator open at any one time.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="inputName">Sets the input file's name, only needed for training or loading a uzn file.</param>
        /// <param name="region">The image region to process.</param>
        /// <param name="pageSegMode">The page layout analyasis method to use.</param>
        /// <returns>A result iterator</returns>
        public Page Process(Pix image, string inputName, Rect region, PageSegMode? pageSegMode = null)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (region.X1 < 0 || region.Y1 < 0 || region.X2 > image.Width || region.Y2 > image.Height)
                throw new ArgumentException("The image region to be processed must be within the image bounds.", "region");
            if (processCount > 0) throw new InvalidOperationException("Only one image can be processed at once. Please make sure you dispose of the page once your finished with it.");

            processCount++;

            var actualPageSegmentMode = pageSegMode.HasValue ? pageSegMode.Value : DefaultPageSegMode;
            Interop.TessApi.BaseAPISetPageSegMode(handle, actualPageSegmentMode);
            Interop.TessApi.BaseApiSetImage(handle, image.Handle);
            if (!String.IsNullOrEmpty(inputName))
            {
                Interop.TessApi.BaseApiSetInputName(handle, inputName);
            }
            var page = new Page(this, image, inputName, region, actualPageSegmentMode);
            page.Disposed += OnIteratorDisposed;
            return page;
        }
        
        protected override void Dispose(bool disposing)
        {
            // handle can be null here: unlike the HandleRef struct it replaces,
            // NativeHandle is a reference type, and an object is registered for
            // finalization at allocation time -- before its constructor body runs.
            // If the constructor throws before (or without) assigning handle (e.g.
            // Guard.RequireNotNullOrEmpty failing, or Initialise failing), the
            // finalizer still runs against a partially-constructed instance.
            if (handle != null && handle.Handle != IntPtr.Zero)
            {
                Interop.TessApi.BaseApiDelete(handle);
                handle = new NativeHandle(IntPtr.Zero);
            }
        }

        private string GetTessDataPrefix()
        {
            try
            {
                return Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
            }
            catch (SecurityException e)
            {
                trace.TraceEvent(TraceEventType.Error, 0, "Failed to detect if the environment variable 'TESSDATA_PREFIX' is set: {0}", e.Message);
                return null;
            }
        }

        private void Initialise(string datapath, string language, EngineMode engineMode, IEnumerable<string> configFiles, IDictionary<string, object> initialValues, bool setOnlyNonDebugVariables)
        {
            Guard.RequireNotNullOrEmpty("language", language);

            // do some minor processing on datapath to fix some common errors (this basically mirrors what tesseract does as of 3.04)
            if (!String.IsNullOrEmpty(datapath))
            {
                // remove any excess whitespace
                datapath = datapath.Trim();

                // remove any trialing '\' or '/' characters
                if (datapath.EndsWith("\\", StringComparison.Ordinal) || datapath.EndsWith("/", StringComparison.Ordinal))
                {
                    datapath = datapath.Substring(0, datapath.Length - 1);
                }
            }

            // Tesseract's own tprintf() (distinct from -- and not reached by --
            // LeptonicaApi's setMsgSeverity(L_SEVERITY_NONE) call: two
            // completely separate logging mechanisms) writes to stderr by
            // default, e.g. a routine "Estimating resolution as N" whenever a
            // Pix's resolution wasn't set, which real OCR calls hit often.
            // Same browser-wasm/Blazor concern as LeptonicaApi's own comment:
            // any stderr write there triggers Blazor's global error banner
            // regardless of severity. tprintf's own debug_file config
            // variable (tesseract/ccutil/tprintf.cpp) redirects it away from
            // stderr; it's passed here as an ordinary init variable rather
            // than a later SetVariable call so it's in effect from this
            // engine's very first Init4 call, before anything can tprintf.
            // Only set if the caller didn't already choose their own
            // debug_file (respects an explicit choice); "/dev/null" is a
            // standard-issue Emscripten virtual device, always present and
            // writable in the wasm runtime's default filesystem setup. Copies
            // into a new dictionary rather than mutating the caller's own
            // instance in place -- initialValues is caller-supplied and may
            // be reused elsewhere.
            if (OperatingSystem.IsBrowser() && (initialValues == null || !initialValues.ContainsKey("debug_file")))
            {
                var merged = initialValues != null ? new Dictionary<string, object>(initialValues) : new Dictionary<string, object>();
                merged["debug_file"] = "/dev/null";
                initialValues = merged;
            }

            if (Interop.TessApi.BaseApiInit(handle, datapath, language, (int)engineMode, configFiles ?? new List<string>(), initialValues ?? new Dictionary<string, object>(), setOnlyNonDebugVariables) != 0)
            {
                // Special case logic to handle cleaning up as init has already released the handle if it fails.
                handle = new NativeHandle(IntPtr.Zero);
                GC.SuppressFinalize(this);

                throw new TesseractException(ErrorMessage.Format(1, "Failed to initialise tesseract engine."));
            }
        }

        /// <summary>
        /// Ties the specified pix to the lifecycle of a page.
        /// </summary>
        public class PageDisposalHandle
        {
            private readonly Page page;
            private readonly Pix pix;

            public PageDisposalHandle(Page page, Pix pix)
            {
                this.page = page;
                this.pix = pix;
                page.Disposed += OnPageDisposed;
            }

            private void OnPageDisposed(object sender, System.EventArgs e)
            {
                page.Disposed -= OnPageDisposed;
                // dispose the pix when the page is disposed.
                pix.Dispose();
            }
        }

        #region Config

        /// <summary>
        /// Gets or sets default <see cref="PageSegMode" /> mode used by <see cref="Tesseract.TesseractEngine.Process(Pix, Rect, PageSegMode?)" />.
        /// </summary>
        public PageSegMode DefaultPageSegMode
        {
            get;
            set;
        }

        public bool SetDebugVariable(string name, string value)
        {
            return Interop.TessApi.BaseApiSetDebugVariable(handle, name, value) != 0;
        }

        /// <summary>
        /// Sets the value of a string variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool SetVariable(string name, string value)
        {
            return Interop.TessApi.BaseApiSetVariable(handle, name, value) != 0;
        }

        /// <summary>
        /// Sets the value of a boolean variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool SetVariable(string name, bool value)
        {
            var strEncodedValue = value ? "TRUE" : "FALSE";
            return Interop.TessApi.BaseApiSetVariable(handle, name, strEncodedValue) != 0;
        }

        /// <summary>
        /// Sets the value of a integer variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool SetVariable(string name, int value)
        {
            var strEncodedValue = value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);
            return Interop.TessApi.BaseApiSetVariable(handle, name, strEncodedValue) != 0;
        }

        /// <summary>
        /// Sets the value of a double variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool SetVariable(string name, double value)
        {
            var strEncodedValue = value.ToString("R", CultureInfo.InvariantCulture.NumberFormat);
            return Interop.TessApi.BaseApiSetVariable(handle, name, strEncodedValue) != 0;
        }

        /// <summary>
        /// Attempts to retrieve the value for a boolean variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The current value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool TryGetBoolVariable(string name, out bool value)
        {
            int val;
            if (Interop.TessApi.BaseApiGetBoolVariable(handle, name, out val) != 0)
            {
                value = (val != 0);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        /// <summary>
        /// Attempts to retrieve the value for a double variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The current value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool TryGetDoubleVariable(string name, out double value)
        {
            return Interop.TessApi.BaseApiGetDoubleVariable(handle, name, out value) != 0;
        }

        /// <summary>
        /// Attempts to retrieve the value for an integer variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The current value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool TryGetIntVariable(string name, out int value)
        {
            return Interop.TessApi.BaseApiGetIntVariable(handle, name, out value) != 0;
        }

        /// <summary>
        /// Attempts to retrieve the value for a string variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The current value of the variable.</param>
        /// <returns>Returns <c>True</c> if successful; otherwise <c>False</c>.</returns>
        public bool TryGetStringVariable(string name, out string value)
        {
            value = Interop.TessApi.BaseApiGetStringVariable(handle, name);
            return value != null;
        }

        /// <summary>
        /// Attempts to print the variables to the file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool TryPrintVariablesToFile(string filename)
        {
            return Interop.TessApi.BaseApiPrintVariablesToFile(handle, filename) != 0;
        }

        #endregion Config

        #region Event Handlers

        private void OnIteratorDisposed(object sender, EventArgs e)
        {
            processCount--;
        }

        #endregion Event Handlers
    }
}
