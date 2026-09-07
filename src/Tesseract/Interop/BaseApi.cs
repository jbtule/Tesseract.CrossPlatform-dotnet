using InteropDotNet;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Tesseract.Internal;

namespace Tesseract.Interop
{
    internal static class TessApi
    {
        static TessApi()
        {
            NativeLibraryResolver.Initialize();
        }

        //XHTML Begin Tag:
        public const string xhtmlBeginTag =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\n"
            + "    \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n"
            + "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" "
            + "lang=\"en\">\n <head>\n  <title></title>\n"
            + "<meta http-equiv=\"Content-Type\" content=\"text/html;"
            + "charset=utf-8\" />\n"
            + "  <meta name='ocr-system' content='tesseract' />\n"
            + "  <meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par"
            + " ocr_line ocrx_word"
            + "'/>\n"
            + "</head>\n<body>\n";

        //XHTML End Tag:
        public const string xhtmlEndTag = " </body>\n</html>\n";

        public const string htmlBeginTag =
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\""
            + " \"http://www.w3.org/TR/html4/loose.dtd\">\n"
            + "<html>\n<head>\n<title></title>\n"
            + "<meta http-equiv=\"Content-Type\" content=\"text/html;"
            + "charset=utf-8\" />\n<meta name='ocr-system' content='tesseract'/>\n"
            + "</head>\n<body>\n";

        public const string htmlEndTag = "</body>\n</html>\n";

        public static string BaseApiGetVersion()
        {
            IntPtr versionHandle = GetVersion();
            if (versionHandle != IntPtr.Zero)
            {
                var result = MarshalHelper.PtrToString(versionHandle, Encoding.UTF8);
                return result;
            }

            return null;
        }

        public static string BaseAPIGetHOCRText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetHOCRTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return htmlBeginTag + result + htmlEndTag;
            } else {
                return null;
            }
        }

        //Just Copied:
        public static string BaseAPIGetHOCRText2(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetHOCRTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return xhtmlBeginTag + result + xhtmlEndTag;
            } else {
                return null;
            }
        }

        public static string BaseAPIGetAltoText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetAltoTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            } else {
                return null;
            }
        }

        public static string BaseAPIGetTsvText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetTsvTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            } else {
                return null;
            }
        }

        public static string BaseAPIGetBoxText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetBoxTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero)
            {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string BaseAPIGetLSTMBoxText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetLSTMBoxTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero)
            {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string BaseAPIGetWordStrBoxText(HandleRef handle, int pageNum)
        {
            IntPtr txtHandle = BaseApiGetWordStrBoxTextInternal(handle, pageNum);
            if (txtHandle != IntPtr.Zero)
            {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string BaseAPIGetUNLVText(HandleRef handle)
        {
            IntPtr txtHandle = BaseApiGetUNLVTextInternal(handle);
            if (txtHandle != IntPtr.Zero)
            {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string BaseApiGetStringVariable(HandleRef handle, string name)
        {
            var resultHandle = BaseApiGetStringVariableInternal(handle, name);
            if (resultHandle != IntPtr.Zero) {
                return MarshalHelper.PtrToString(resultHandle, Encoding.UTF8);
            } else {
                return null;
            }
        }

        public static string BaseAPIGetUTF8Text(HandleRef handle)
        {
            IntPtr txtHandle = BaseAPIGetUTF8TextInternal(handle);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            } else {
                return null;
            }
        }

        public static int BaseApiInit(HandleRef handle, string datapath, string language, int mode, IEnumerable<string> configFiles, IDictionary<string, object> initialValues, bool setOnlyNonDebugParams)
        {
            Guard.Require("handle", handle.Handle != IntPtr.Zero, "Handle for BaseApi, created through BaseApiCreate is required.");
            Guard.RequireNotNullOrEmpty("language", language);
            Guard.RequireNotNull("configFiles", configFiles);
            Guard.RequireNotNull("initialValues", initialValues);

            string[] configFilesArray = new List<string>(configFiles).ToArray();

            string[] varNames = new string[initialValues.Count];
            string[] varValues = new string[initialValues.Count];
            int i = 0;
            foreach (var pair in initialValues) {
                Guard.Require("initialValues", !String.IsNullOrEmpty(pair.Key), "Variable must have a name.");

                Guard.Require("initialValues", pair.Value != null, "Variable '{0}': The type '{1}' is not supported.", pair.Key, pair.Value.GetType());
                varNames[i] = pair.Key;
                string varValue;
                if (TessConvert.TryToString(pair.Value, out varValue)) {
                    varValues[i] = varValue;
                } else {
                    throw new ArgumentException(
                        String.Format("Variable '{0}': The type '{1}' is not supported.", pair.Key, pair.Value.GetType()),
                        "initialValues"
                    );
                }
                i++;
            }

            return BaseApiInit(handle, datapath, language, mode,
                configFilesArray, configFilesArray.Length,
                varNames, varValues, new UIntPtr((uint)varNames.Length), setOnlyNonDebugParams);
        }

        public static int BaseApiSetDebugVariable(HandleRef handle, string name, string value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try {
                valuePtr = MarshalHelper.StringToPtr(value, Encoding.UTF8);
                return BaseApiSetDebugVariable(handle, name, valuePtr);
            } finally {
                if (valuePtr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(valuePtr);
                }
            }
        }

        public static int BaseApiSetVariable(HandleRef handle, string name, string value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try {
                valuePtr = MarshalHelper.StringToPtr(value, Encoding.UTF8);
                return BaseApiSetVariable(handle, name, valuePtr);
            } finally {
                if (valuePtr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(valuePtr);
                }
            }
        }

        public static string ResultIteratorWordRecognitionLanguage(HandleRef handle)
        {
            // per docs (ltrresultiterator.h:118 as of 4897796 in github:tesseract-ocr/tesseract)
            // this return value should *NOT* be deleted.
            IntPtr txtHandle =
                ResultIteratorWordRecognitionLanguageInternal(handle);

            return txtHandle != IntPtr.Zero
                ? MarshalHelper.PtrToString(txtHandle, Encoding.UTF8)
                : null;
        }

        public static string ResultIteratorGetUTF8Text(HandleRef handle, PageIteratorLevel level)
        {
            IntPtr txtHandle = ResultIteratorGetUTF8TextInternal(handle, level);
            if (txtHandle != IntPtr.Zero) {
                var result = MarshalHelper.PtrToString(txtHandle, Encoding.UTF8);
                DeleteText(txtHandle);
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns the null terminated UTF-8 encoded text string for the current choice
        /// </summary>
        /// <remarks>
        /// NOTE: Unlike LTRResultIterator::GetUTF8Text, the return points to an
        /// internal structure and should NOT be delete[]ed to free after use.
        /// </remarks>
        /// <param name="choiceIteratorHandle"></param>
        /// <returns>string</returns>
        internal static string ChoiceIteratorGetUTF8Text(HandleRef choiceIteratorHandle)
        {
            Guard.Require("choiceIteratorHandle", choiceIteratorHandle.Handle != IntPtr.Zero, "ChoiceIterator Handle cannot be a null IntPtr and is required");
            IntPtr txtChoiceHandle = ChoiceIteratorGetUTF8TextInternal(choiceIteratorHandle);
            return MarshalHelper.PtrToString(txtChoiceHandle, Encoding.UTF8);
        }

        // hOCR Extension

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetComponentImages")]
        internal static extern IntPtr BaseAPIGetComponentImages(HandleRef handle, PageIteratorLevel level, int text_only, IntPtr pixa, IntPtr blockids);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIAnalyseLayout")]
        internal static extern IntPtr BaseAPIAnalyseLayout(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIClear")]
        internal static extern void BaseAPIClear(HandleRef handle);

        /// <summary>
        /// Creates a new BaseAPI instance
        /// </summary>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPICreate")]
        public static extern IntPtr BaseApiCreate();

        // Base API
        /// <summary>
        /// Deletes a base api instance.
        /// </summary>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIDelete")]
        public static extern void BaseApiDelete(HandleRef ptr);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIDetectOrientationScript")]
        public static extern int TessBaseAPIDetectOrientationScript(HandleRef handle, out int orient_deg, out float orient_conf, out IntPtr script_name, out float script_conf);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetBoolVariable")]
        public static extern int BaseApiGetBoolVariable(HandleRef handle, string name, out int value);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetDoubleVariable")]
        public static extern int BaseApiGetDoubleVariable(HandleRef handle, string name, out double value);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetHOCRText")]
        internal static extern IntPtr BaseApiGetHOCRTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetAltoText")]
        internal static extern IntPtr BaseApiGetAltoTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetTsvText")]
        internal static extern IntPtr BaseApiGetTsvTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetBoxText")]
        internal static extern IntPtr BaseApiGetBoxTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetLSTMBoxText")]
        internal static extern IntPtr BaseApiGetLSTMBoxTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetWordStrBoxText")]
        internal static extern IntPtr BaseApiGetWordStrBoxTextInternal(HandleRef handle, int pageNum);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetUNLVText")]
        internal static extern IntPtr BaseApiGetUNLVTextInternal(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetIntVariable")]
        public static extern int BaseApiGetIntVariable(HandleRef handle, string name, out int value);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetIterator")]
        public static extern IntPtr BaseApiGetIterator(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetPageSegMode")]
        public static extern PageSegMode BaseAPIGetPageSegMode(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetStringVariable")]
        internal static extern IntPtr BaseApiGetStringVariableInternal(HandleRef handle, string name);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetThresholdedImage")]
        public static extern IntPtr BaseAPIGetThresholdedImage(HandleRef handle);

        // The following were causing issues on Linux/MacOsX when used in .net core
        //[DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIProcessPages")]
        //internal static extern int BaseAPIProcessPages(HandleRef handle, string filename, string retry_config, int timeout_millisec, HandleRef renderer);

        //[DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIProcessPage")]
        //internal static extern int BaseAPIProcessPage(HandleRef handle, Pix pix, int page_index, string filename, string retry_config, int timeout_millisec, HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetInputName")]
        public static extern void BaseAPISetInputName(HandleRef handle, string name);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetDatapath")]
        public static extern string BaseAPIGetDatapath(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetOutputName")]
        public static extern void BaseAPISetOutputName(HandleRef handle, string name);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIGetUTF8Text")]
        internal static extern IntPtr BaseAPIGetUTF8TextInternal(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIInit4")]
        internal static extern int BaseApiInit(HandleRef handle, string datapath, string language, int mode,
                                      string[] configs, int configs_size,
                                      string[] vars_vec, string[] vars_values, UIntPtr vars_vec_size,
                                      bool set_only_non_debug_params);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIMeanTextConf")]
        public static extern int BaseAPIMeanTextConf(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIRecognize")]
        public static extern int BaseApiRecognize(HandleRef handle, HandleRef monitor);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetDebugVariable")]
        internal static extern int BaseApiSetDebugVariable(HandleRef handle, string name, IntPtr valPtr);

        // image analysis
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetImage2")]
        public static extern void BaseApiSetImage(HandleRef handle, HandleRef pixHandle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetInputName")]
        public static extern void BaseApiSetInputName(HandleRef handle, string value);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetPageSegMode")]
        public static extern void BaseAPISetPageSegMode(HandleRef handle, PageSegMode mode);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetRectangle")]
        public static extern void BaseApiSetRectangle(HandleRef handle, int left, int top, int width, int height);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPISetVariable")]
        internal static extern int BaseApiSetVariable(HandleRef handle, string name, IntPtr valPtr);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessDeleteIntArray")]
        public static extern void DeleteIntArray(IntPtr arr);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessDeleteText")]
        public static extern void DeleteText(IntPtr textPtr);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessDeleteTextArray")]
        public static extern void DeleteTextArray(IntPtr arr);

        // Helper functions
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessVersion")]
        public static extern IntPtr GetVersion();

        // result iterator

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorBaseline")]
        public static extern int PageIteratorBaseline(HandleRef handle, PageIteratorLevel level, out int x1, out int y1, out int x2, out int y2);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorBegin")]
        public static extern void PageIteratorBegin(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorBlockType")]
        public static extern PolyBlockType PageIteratorBlockType(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorBoundingBox")]
        public static extern int PageIteratorBoundingBox(HandleRef handle, PageIteratorLevel level, out int left, out int top, out int right, out int bottom);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorCopy")]
        public static extern IntPtr PageIteratorCopy(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorDelete")]
        public static extern void PageIteratorDelete(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorGetBinaryImage")]
        public static extern IntPtr PageIteratorGetBinaryImage(HandleRef handle, PageIteratorLevel level);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorGetImage")]
        public static extern IntPtr PageIteratorGetImage(HandleRef handle, PageIteratorLevel level, int padding, HandleRef originalImage, out int left, out int top);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorIsAtBeginningOf")]
        public static extern int PageIteratorIsAtBeginningOf(HandleRef handle, PageIteratorLevel level);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorIsAtFinalElement")]
        public static extern int PageIteratorIsAtFinalElement(HandleRef handle, PageIteratorLevel level, PageIteratorLevel element);

        // page iterator
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorNext")]
        public static extern int PageIteratorNext(HandleRef handle, PageIteratorLevel level);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPageIteratorOrientation")]
        public static extern void PageIteratorOrientation(HandleRef handle, out Orientation orientation, out WritingDirection writing_direction, out TextLineOrder textLineOrder, out float deskew_angle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorCopy")]
        public static extern IntPtr ResultIteratorCopy(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorDelete")]
        public static extern void ResultIteratorDelete(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorConfidence")]
        public static extern float ResultIteratorGetConfidence(HandleRef handle, PageIteratorLevel level);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorWordFontAttributes")]
        public static extern IntPtr ResultIteratorWordFontAttributes(HandleRef handle, out bool isBold, out bool isItalic, out bool isUnderlined, out bool isMonospace, out bool isSerif, out bool isSmallCaps, out int pointSize, out int fontId);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorWordIsFromDictionary")]
        public static extern bool ResultIteratorWordIsFromDictionary(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorWordIsNumeric")]
        public static extern bool ResultIteratorWordIsNumeric(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorWordRecognitionLanguage")]
        internal static extern IntPtr ResultIteratorWordRecognitionLanguageInternal(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorSymbolIsSuperscript")]
        public static extern bool ResultIteratorSymbolIsSuperscript(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorSymbolIsSubscript")]
        public static extern bool ResultIteratorSymbolIsSubscript(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorSymbolIsDropcap")]
        public static extern bool ResultIteratorSymbolIsDropcap(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorGetPageIterator")]
        public static extern IntPtr ResultIteratorGetPageIterator(HandleRef handle);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorGetUTF8Text")]
        internal static extern IntPtr ResultIteratorGetUTF8TextInternal(HandleRef handle, PageIteratorLevel level);

        #region Choice Iterator

        /// <summary>
        /// Native API call to TessResultIteratorGetChoiceIterator
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultIteratorGetChoiceIterator")]
        public static extern IntPtr ResultIteratorGetChoiceIterator(HandleRef handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorDelete
        /// </summary>
        /// <param name="handle"></param>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessChoiceIteratorDelete")]
        public static extern void ChoiceIteratorDelete(HandleRef handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorNext
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessChoiceIteratorNext")]
        public static extern int ChoiceIteratorNext(HandleRef handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorGetUTF8Text
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessChoiceIteratorGetUTF8Text")]
        internal static extern IntPtr ChoiceIteratorGetUTF8TextInternal(HandleRef handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorConfidence
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessChoiceIteratorConfidence")]
        public static extern float ChoiceIteratorGetConfidence(HandleRef handle);

        #endregion Choice Iterator

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIPrintVariablesToFile")]
        public static extern int BaseApiPrintVariablesToFile(HandleRef handle, string filename);

        #region Renderer API

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessTextRendererCreate")]
        public static extern IntPtr TextRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessAltoRendererCreate")]
        public static extern IntPtr AltoRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessTsvRendererCreate")]
        public static extern IntPtr TsvRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessHOcrRendererCreate")]
        public static extern IntPtr HOcrRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessHOcrRendererCreate2")]
        public static extern IntPtr HOcrRendererCreate2(string outputbase, int font_info);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessPDFRendererCreate")]
        public static extern IntPtr PDFRendererCreate(string outputbase, IntPtr datadir, int textonly);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessUnlvRendererCreate")]
        public static extern IntPtr UnlvRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBoxTextRendererCreate")]
        public static extern IntPtr BoxTextRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessLSTMBoxRendererCreate")]
        public static extern IntPtr LSTMBoxRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessWordStrBoxRendererCreate")]
        public static extern IntPtr WordStrBoxRendererCreate(string outputbase);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessDeleteResultRenderer")]
        public static extern void DeleteResultRenderer(HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererInsert")]
        public static extern void ResultRendererInsert(HandleRef renderer, HandleRef next);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererNext")]
        public static extern IntPtr ResultRendererNext(HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererBeginDocument")]
        public static extern int ResultRendererBeginDocument(HandleRef renderer, IntPtr titlePtr);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererAddImage")]
        public static extern int ResultRendererAddImage(HandleRef renderer, HandleRef api);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererEndDocument")]
        public static extern int ResultRendererEndDocument(HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererExtention")]
        public static extern IntPtr ResultRendererExtention(HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererTitle")]
        public static extern IntPtr ResultRendererTitle(HandleRef renderer);

        [DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessResultRendererImageNum")]
        public static extern int ResultRendererImageNum(HandleRef renderer);

        #endregion Renderer API
    }
}
