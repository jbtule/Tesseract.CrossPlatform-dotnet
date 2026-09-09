using InteropDotNet;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Tesseract.Internal;

namespace Tesseract.Interop
{
    internal static partial class TessApi
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

        public static string BaseAPIGetHOCRText(NativeHandle handle, int pageNum)
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
        public static string BaseAPIGetHOCRText2(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetAltoText(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetTsvText(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetBoxText(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetLSTMBoxText(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetWordStrBoxText(NativeHandle handle, int pageNum)
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

        public static string BaseAPIGetUNLVText(NativeHandle handle)
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

        public static string BaseApiGetStringVariable(NativeHandle handle, string name)
        {
            var resultHandle = BaseApiGetStringVariableInternal(handle, name);
            if (resultHandle != IntPtr.Zero) {
                return MarshalHelper.PtrToString(resultHandle, Encoding.UTF8);
            } else {
                return null;
            }
        }

        public static string BaseAPIGetUTF8Text(NativeHandle handle)
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

        public static int BaseApiInit(NativeHandle handle, string datapath, string language, int mode, IEnumerable<string> configFiles, IDictionary<string, object> initialValues, bool setOnlyNonDebugParams)
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

        public static int BaseApiSetDebugVariable(NativeHandle handle, string name, string value)
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

        public static int BaseApiSetVariable(NativeHandle handle, string name, string value)
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

        public static string ResultIteratorWordRecognitionLanguage(NativeHandle handle)
        {
            // per docs (ltrresultiterator.h:118 as of 4897796 in github:tesseract-ocr/tesseract)
            // this return value should *NOT* be deleted.
            IntPtr txtHandle =
                ResultIteratorWordRecognitionLanguageInternal(handle);

            return txtHandle != IntPtr.Zero
                ? MarshalHelper.PtrToString(txtHandle, Encoding.UTF8)
                : null;
        }

        public static string ResultIteratorGetUTF8Text(NativeHandle handle, PageIteratorLevel level)
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
        internal static string ChoiceIteratorGetUTF8Text(NativeHandle choiceIteratorHandle)
        {
            Guard.Require("choiceIteratorHandle", choiceIteratorHandle.Handle != IntPtr.Zero, "ChoiceIterator Handle cannot be a null IntPtr and is required");
            IntPtr txtChoiceHandle = ChoiceIteratorGetUTF8TextInternal(choiceIteratorHandle);
            return MarshalHelper.PtrToString(txtChoiceHandle, Encoding.UTF8);
        }

        // hOCR Extension

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetComponentImages")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseAPIGetComponentImages(NativeHandle handle, PageIteratorLevel level, int text_only, IntPtr pixa, IntPtr blockids);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIAnalyseLayout")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseAPIAnalyseLayout(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIClear")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial void BaseAPIClear(NativeHandle handle);

        /// <summary>
        /// Creates a new BaseAPI instance
        /// </summary>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPICreate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr BaseApiCreate();

        // Base API
        /// <summary>
        /// Deletes a base api instance.
        /// </summary>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIDelete")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseApiDelete(NativeHandle ptr);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIDetectOrientationScript")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int TessBaseAPIDetectOrientationScript(NativeHandle handle, out int orient_deg, out float orient_conf, out IntPtr script_name, out float script_conf);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetBoolVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseApiGetBoolVariable(NativeHandle handle, string name, out int value);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetDoubleVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseApiGetDoubleVariable(NativeHandle handle, string name, out double value);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetHOCRText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetHOCRTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetAltoText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetAltoTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetTsvText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetTsvTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetBoxText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetBoxTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetLSTMBoxText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetLSTMBoxTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetWordStrBoxText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetWordStrBoxTextInternal(NativeHandle handle, int pageNum);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetUNLVText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetUNLVTextInternal(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetIntVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseApiGetIntVariable(NativeHandle handle, string name, out int value);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetIterator")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr BaseApiGetIterator(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetPageSegMode")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial PageSegMode BaseAPIGetPageSegMode(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetStringVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseApiGetStringVariableInternal(NativeHandle handle, string name);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetThresholdedImage")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr BaseAPIGetThresholdedImage(NativeHandle handle);

        // The following were causing issues on Linux/MacOsX when used in .net core
        //[DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIProcessPages")]
        //internal static extern int BaseAPIProcessPages(NativeHandle handle, string filename, string retry_config, int timeout_millisec, NativeHandle renderer);

        //[DllImport(Constants.TesseractDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TessBaseAPIProcessPage")]
        //internal static extern int BaseAPIProcessPage(NativeHandle handle, Pix pix, int page_index, string filename, string retry_config, int timeout_millisec, NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetInputName", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseAPISetInputName(NativeHandle handle, string name);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetDatapath", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial string BaseAPIGetDatapath(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetOutputName", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseAPISetOutputName(NativeHandle handle, string name);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIGetUTF8Text")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr BaseAPIGetUTF8TextInternal(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIInit4", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial int BaseApiInit(NativeHandle handle, string datapath, string language, int mode,
                                      string[] configs, int configs_size,
                                      string[] vars_vec, string[] vars_values, UIntPtr vars_vec_size,
                                      [MarshalAs(UnmanagedType.Bool)] bool set_only_non_debug_params);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIMeanTextConf")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseAPIMeanTextConf(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIRecognize")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseApiRecognize(NativeHandle handle, NativeHandle monitor);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetDebugVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial int BaseApiSetDebugVariable(NativeHandle handle, string name, IntPtr valPtr);

        // image analysis
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetImage2")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseApiSetImage(NativeHandle handle, NativeHandle pixHandle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetInputName", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseApiSetInputName(NativeHandle handle, string value);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetPageSegMode")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseAPISetPageSegMode(NativeHandle handle, PageSegMode mode);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetRectangle")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void BaseApiSetRectangle(NativeHandle handle, int left, int top, int width, int height);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPISetVariable", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial int BaseApiSetVariable(NativeHandle handle, string name, IntPtr valPtr);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessDeleteIntArray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void DeleteIntArray(IntPtr arr);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessDeleteText")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void DeleteText(IntPtr textPtr);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessDeleteTextArray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void DeleteTextArray(IntPtr arr);

        // Helper functions
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessVersion")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr GetVersion();

        // result iterator

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorBaseline")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int PageIteratorBaseline(NativeHandle handle, PageIteratorLevel level, out int x1, out int y1, out int x2, out int y2);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorBegin")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void PageIteratorBegin(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorBlockType")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial PolyBlockType PageIteratorBlockType(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorBoundingBox")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int PageIteratorBoundingBox(NativeHandle handle, PageIteratorLevel level, out int left, out int top, out int right, out int bottom);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorCopy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr PageIteratorCopy(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorDelete")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void PageIteratorDelete(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorGetBinaryImage")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr PageIteratorGetBinaryImage(NativeHandle handle, PageIteratorLevel level);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorGetImage")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr PageIteratorGetImage(NativeHandle handle, PageIteratorLevel level, int padding, NativeHandle originalImage, out int left, out int top);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorIsAtBeginningOf")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int PageIteratorIsAtBeginningOf(NativeHandle handle, PageIteratorLevel level);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorIsAtFinalElement")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int PageIteratorIsAtFinalElement(NativeHandle handle, PageIteratorLevel level, PageIteratorLevel element);

        // page iterator
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorNext")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int PageIteratorNext(NativeHandle handle, PageIteratorLevel level);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPageIteratorOrientation")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void PageIteratorOrientation(NativeHandle handle, out Orientation orientation, out WritingDirection writing_direction, out TextLineOrder textLineOrder, out float deskew_angle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorCopy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultIteratorCopy(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorDelete")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void ResultIteratorDelete(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorConfidence")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial float ResultIteratorGetConfidence(NativeHandle handle, PageIteratorLevel level);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorWordFontAttributes")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultIteratorWordFontAttributes(NativeHandle handle,
            [MarshalAs(UnmanagedType.Bool)] out bool isBold,
            [MarshalAs(UnmanagedType.Bool)] out bool isItalic,
            [MarshalAs(UnmanagedType.Bool)] out bool isUnderlined,
            [MarshalAs(UnmanagedType.Bool)] out bool isMonospace,
            [MarshalAs(UnmanagedType.Bool)] out bool isSerif,
            [MarshalAs(UnmanagedType.Bool)] out bool isSmallCaps,
            out int pointSize, out int fontId);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorWordIsFromDictionary")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ResultIteratorWordIsFromDictionary(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorWordIsNumeric")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ResultIteratorWordIsNumeric(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorWordRecognitionLanguage")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr ResultIteratorWordRecognitionLanguageInternal(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorSymbolIsSuperscript")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ResultIteratorSymbolIsSuperscript(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorSymbolIsSubscript")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ResultIteratorSymbolIsSubscript(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorSymbolIsDropcap")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ResultIteratorSymbolIsDropcap(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorGetPageIterator")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultIteratorGetPageIterator(NativeHandle handle);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorGetUTF8Text")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr ResultIteratorGetUTF8TextInternal(NativeHandle handle, PageIteratorLevel level);

        #region Choice Iterator

        /// <summary>
        /// Native API call to TessResultIteratorGetChoiceIterator
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultIteratorGetChoiceIterator")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultIteratorGetChoiceIterator(NativeHandle handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorDelete
        /// </summary>
        /// <param name="handle"></param>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessChoiceIteratorDelete")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void ChoiceIteratorDelete(NativeHandle handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorNext
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessChoiceIteratorNext")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int ChoiceIteratorNext(NativeHandle handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorGetUTF8Text
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessChoiceIteratorGetUTF8Text")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr ChoiceIteratorGetUTF8TextInternal(NativeHandle handle);

        /// <summary>
        /// Native API call to TessChoiceIteratorConfidence
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessChoiceIteratorConfidence")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial float ChoiceIteratorGetConfidence(NativeHandle handle);

        #endregion Choice Iterator

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBaseAPIPrintVariablesToFile", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int BaseApiPrintVariablesToFile(NativeHandle handle, string filename);

        #region Renderer API

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessTextRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr TextRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessAltoRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr AltoRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessTsvRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr TsvRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessHOcrRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr HOcrRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessHOcrRendererCreate2", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr HOcrRendererCreate2(string outputbase, int font_info);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessPDFRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr PDFRendererCreate(string outputbase, IntPtr datadir, int textonly);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessUnlvRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr UnlvRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessBoxTextRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr BoxTextRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessLSTMBoxRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr LSTMBoxRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessWordStrBoxRendererCreate", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr WordStrBoxRendererCreate(string outputbase);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessDeleteResultRenderer")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void DeleteResultRenderer(NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererInsert")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void ResultRendererInsert(NativeHandle renderer, NativeHandle next);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererNext")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultRendererNext(NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererBeginDocument")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int ResultRendererBeginDocument(NativeHandle renderer, IntPtr titlePtr);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererAddImage")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int ResultRendererAddImage(NativeHandle renderer, NativeHandle api);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererEndDocument")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int ResultRendererEndDocument(NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererExtention")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultRendererExtention(NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererTitle")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr ResultRendererTitle(NativeHandle renderer);

        [LibraryImport(Constants.TesseractDllName, EntryPoint = "TessResultRendererImageNum")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int ResultRendererImageNum(NativeHandle renderer);

        #endregion Renderer API
    }
}
