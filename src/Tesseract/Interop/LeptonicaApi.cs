using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using InteropDotNet;

namespace Tesseract.Interop
{
    internal unsafe static partial class LeptonicaApi
    {
        // Leptonica's own diagnostic messages (fprintf(stderr, "Error in ...")/
        // "Warning in ..."), e.g. from the font-loading path a fresh
        // TesseractEngine touches during init: harmless everywhere else, but
        // under browser-wasm any stderr write at all -- not just an actual
        // fatal error -- unconditionally triggers Blazor's own
        // #blazor-error-ui "An unhandled error has occurred" banner (confirmed
        // directly in a real build's shipped blazor.webassembly.js: its err:
        // module callback does `console.error(e), yt()` with no severity
        // gating at all). L_SEVERITY_NONE = 6, from leptonica's own
        // environ.h -- suppresses every leptonica message regardless of
        // severity. Desktop RIDs are unaffected: this only runs under wasm.
        private const int L_SEVERITY_NONE = 6;
        private static int msgSeverityInitialized;

        static LeptonicaApi()
        {
            NativeLibraryResolver.Initialize();
            SuppressConsoleOutputUnderWasm();
        }

        // Callable from both this class's own static constructor and
        // TessApi's (see that class's matching call) -- a real bug, not
        // theorized: a consumer whose own code constructs TesseractEngine
        // before ever touching LeptonicaApi/Pix (the natural, common
        // pattern -- build the engine once, reuse it per scan) never runs
        // *this* class's static constructor before TessBaseAPIInit4 fires,
        // which internally calls into leptonica for the same font-loading
        // path this whole fix targets -- so the suppression call arrived
        // too late to matter. Reproduced directly (a one-line construction-
        // order swap in smoketest-wasm) before fixing. Idempotent via the
        // same Interlocked.Exchange-guard pattern NativeLibraryResolver.Initialize
        // already uses for the identical "whichever class gets touched
        // first" problem, so it's safe to call from both places.
        internal static void SuppressConsoleOutputUnderWasm()
        {
            if (!System.OperatingSystem.IsBrowser())
                return;
            if (Interlocked.Exchange(ref msgSeverityInitialized, 1) != 0)
                return;
            setMsgSeverity(L_SEVERITY_NONE);
        }

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "setMsgSeverity")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial int setMsgSeverity(int newSeverity);

        #region PixA

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaReadMultipageTiff", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixaReadMultipageTiff(string filename);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaCreate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixaCreate(int n);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaAddPix")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixaAddPix(NativeHandle pixa, NativeHandle pix, PixArrayAccessType copyflag);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaGetPix")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixaGetPix(NativeHandle pixa, int index, PixArrayAccessType accesstype);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaRemovePix")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixaRemovePix(NativeHandle pixa, int index);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaClear")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixaClear(NativeHandle pixa);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaGetCount")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixaGetCount(NativeHandle pixa);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixaDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void pixaDestroy(ref IntPtr pix);

        #endregion

        #region Pix

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixCreate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe partial IntPtr pixCreate(int width, int height, int depth);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixClone")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe partial IntPtr pixClone(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void pixDestroy(ref IntPtr pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixEqual")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixEqual(NativeHandle pix1, NativeHandle pix2, out int same);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetWidth")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetWidth(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetHeight")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetHeight(NativeHandle pix);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetDepth")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetDepth(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetXRes")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetXRes(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetYRes")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetYRes(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetResolution")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetResolution(NativeHandle pix, out int xres, out int yres);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetWpl")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixGetWpl(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSetXRes")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSetXRes(NativeHandle pix, int xres);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSetYRes")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSetYRes(NativeHandle pix, int yres);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSetResolution")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSetResolution(NativeHandle pix, int xres, int yres);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixScaleResolution")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixScaleResolution(NativeHandle pix, float xscale, float yscale);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetData")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixGetData(NativeHandle pix);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetInputFormat")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ImageFormat pixGetInputFormat(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSetInputFormat")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSetInputFormat(NativeHandle pix, ImageFormat inputFormat);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixEndianByteSwap")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixEndianByteSwap(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixRead", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixRead(string filename);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixReadMem")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe partial IntPtr pixReadMem(byte* data, int length);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixReadMemTiff")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe partial IntPtr pixReadMemTiff(byte* data, int length, int page);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixReadFromMultipageTiff", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixReadFromMultipageTiff(string filename, ref int offset);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixWrite", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixWrite(string filename, NativeHandle handle, ImageFormat format);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixDisplayWrite")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixDisplayWrite(NativeHandle pixs, int reduction);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGetColormap")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixGetColormap(NativeHandle pix);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSetColormap")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSetColormap(NativeHandle pix, NativeHandle pixCmap);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixDestroyColormap")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixDestroyColormap(NativeHandle pix);

        // pixconv.h functions

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixConvertRGBToGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixConvertRGBToGray(NativeHandle pix, float rwt, float gwt, float bwt);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixConvertTo8")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixConvertTo8(NativeHandle pix, int cmapflag);


        // image analysis and manipulation functions

        // skew

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixDeskewGeneral")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixDeskewGeneral(NativeHandle pix, int redSweep, float sweepRange, float sweepDelta, int redSearch, int thresh, out float pAngle, out float pConf);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixFindSkew")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixFindSkew(NativeHandle pixs, out float pangle, out float pconf);

        // rotation

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixRotate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixRotate(NativeHandle pixs, float angle, RotationMethod type, RotationFill fillColor, int width, int heigh);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixRotateOrth")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixRotateOrth(NativeHandle pixs, int quads);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixRotateAMGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixRotateAMGray(NativeHandle pixs, float angle, byte grayval);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixRotate90")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixRotate90(NativeHandle pixs, int direction);

        // Grayscale

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixCloseGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixCloseGray(NativeHandle pixs, int hsize, int vsize);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixErodeGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixErodeGray(NativeHandle pixs, int hsize, int vsize);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixAddGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixAddGray(NativeHandle pixd, NativeHandle pixs1, NativeHandle pixs2);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixOpenGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixOpenGray(NativeHandle pixs, int hsize, int vsize);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixCombineMasked")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixCombineMasked(NativeHandle pixd, NativeHandle pixs, NativeHandle pixm);

        // Threshold

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixThresholdToValue")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixThresholdToValue(NativeHandle pixd, NativeHandle pixs, int threshval, int setval);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixThresholdToBinary")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixThresholdToBinary(NativeHandle pixs, int thresh);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixInvert")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixInvert(NativeHandle pixd, NativeHandle pixs);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixBackgroundNormFlex")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixBackgroundNormFlex(NativeHandle pixs, int sx, int sy, int smoothx, int smoothy, int delta);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixGammaTRCMasked")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixGammaTRCMasked(NativeHandle pixd, NativeHandle pixs, NativeHandle pixm, float gamma, int minval, int maxval);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixHMT")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixHMT(NativeHandle pixd, NativeHandle pixs, NativeHandle sel);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixDilate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixDilate(NativeHandle pixd, NativeHandle pixs, NativeHandle sel);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSubtract")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixSubtract(NativeHandle pixd, NativeHandle pixs1, NativeHandle pixs2);

        // Sel

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "selCreateFromString", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr selCreateFromString(string text, int h, int w, string name);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "selCreateBrick")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr selCreateBrick(int h, int w, int cy, int cx, SelType type);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "selDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void selDestroy(ref IntPtr psel);

        // Binarization - src/binarize.c

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixOtsuAdaptiveThreshold")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixOtsuAdaptiveThreshold(NativeHandle pix, int sx, int sy, int smoothx, int smoothy, float scorefract, out IntPtr ppixth, out IntPtr ppixd);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSauvolaBinarize")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSauvolaBinarize(NativeHandle pix, int whsize, float factor, int addborder, out IntPtr ppixm, out IntPtr ppixsd, out IntPtr ppixth, out IntPtr ppixd);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixSauvolaBinarizeTiled")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixSauvolaBinarizeTiled(NativeHandle pix, int whsize, float factor, int nx, int ny, out IntPtr ppixth, out IntPtr ppixd);

        // Scaling - src/scale.c

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixScale")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixScale(NativeHandle pixs, float scalex, float scaley);

        #endregion

        #region Color map

        // Color map creation and deletion

        /// <summary>
        /// Creates a new colormap with the specified <paramref name="depth"/>.
        /// </summary>
        /// <param name="depth">The depth of the pix in bpp, can be 2, 4, or 8</param>
        /// <returns>The pointer to the color map, or null on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapCreate")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapCreate(int depth);

        /// <summary>
        /// Creates a new colormap of the specified <paramref name="depth"/> with random colors where the first color can optionally be set to black, and the last optionally set to white.
        /// </summary>
        /// <param name="depth">The depth of the pix in bpp, can be 2, 4, or 8</param>
        /// <param name="hasBlack">If set to 1 the first color will be black.</param>
        /// <param name="hasWhite">If set to 1 the last color will be white.</param>
        /// <returns>The pointer to the color map, or null on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapCreateRandom")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapCreateRandom(int depth, int hasBlack, int hasWhite);

        /// <summary>
        /// Creates a new colormap of the specified <paramref name="depth"/> with equally spaced gray color values.
        /// </summary>
        /// <param name="depth">The depth of the pix in bpp, can be 2, 4, or 8</param>
        /// <param name="levels">The number of levels (must be between 2 and 2^<paramref name="depth"/></param>
        /// <returns>The pointer to the colormap, or null on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapCreateLinear")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapCreateLinear(int depth, int levels);

        /// <summary>
        /// Performs a deep copy of the color map.
        /// </summary>
        /// <param name="cmaps">The pointer to the colormap instance.</param>
        /// <returns>The pointer to the colormap, or null on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapCopy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapCopy(NativeHandle cmaps);

        /// <summary>
        /// Destorys and cleans up any memory used by the color map.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance, set to null on success.</param>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void pixcmapDestroy(ref IntPtr cmap);

        // colormap metadata (depth, count, etc)

        /// <summary>
        /// Gets the number of color entries in the color map.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <returns>Returns the number of color entries in the color map, or 0 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetCount")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetCount(NativeHandle cmap);

        /// <summary>
        /// Gets the number of free color entries in the color map.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <returns>Returns the number of free color entries in the color map, or 0 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetFreeCount")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetFreeCount(NativeHandle cmap);


        /// <returns>Returns color maps depth, or 0 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetDepth")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetDepth(NativeHandle cmap);

        /// <summary>
        /// Gets the minimum pix depth required to support the color map.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="minDepth">Returns the minimum depth to support the colormap</param>
        /// <returns>Returns 0 if OK, 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetMinDepth")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetMinDepth(NativeHandle cmap, out int minDepth);

        // colormap - color addition\clearing

        /// <summary>
        /// Removes all colors from the color map by setting the count to zero.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <returns>Returns 0 if OK, 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapClear")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapClear(NativeHandle cmap);

        /// <summary>
        /// Adds the color to the pix color map if their is room.
        /// </summary>
        /// <returns>Returns 0 if OK, 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapAddColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapAddColor(NativeHandle cmap, int redValue, int greenValue, int blueValue);

        /// <summary>
        /// Adds the specified color if it doesn't already exist, returning the colors index in the data array.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="redValue">The red value</param>
        /// <param name="greenValue">The green value</param>
        /// <param name="blueValue">The blue value</param>
        /// <param name="colorIndex">The index of the new color if it was added, or the existing color if it already existed.</param>
        /// <returns>Returns 0 for success, 1 for error, 2 for not enough space.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapAddNewColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapAddNewColor(NativeHandle cmap, int redValue, int greenValue, int blueValue, out int colorIndex);

        /// <summary>
        /// Adds the specified color if it doesn't already exist, returning the color's index in the data array.
        /// </summary>
        /// <remarks>
        /// If the color doesn't exist and there is not enough room to add a new color return the nearest color.
        /// </remarks>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="redValue">The red value</param>
        /// <param name="greenValue">The green value</param>
        /// <param name="blueValue">The blue value</param>
        /// <param name="colorIndex">The index of the new color if it was added, or the existing color if it already existed.</param>
        /// <returns>Returns 0 for success, 1 for error, 2 for not enough space.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapAddNearestColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapAddNearestColor(NativeHandle cmap, int redValue, int greenValue, int blueValue, out int colorIndex);

        /// <summary>
        /// Checks if the color already exists or if their is enough room to add it.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="redValue">The red value</param>
        /// <param name="greenValue">The green value</param>
        /// <param name="blueValue">The blue value</param>
        /// <param name="usable">Returns 1 if usable; 0 if not.</param>
        /// <returns>Returns 0 if OK, 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapUsableColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapUsableColor(NativeHandle cmap, int redValue, int greenValue, int blueValue, out int usable);

        /// <summary>
        /// Adds a color (black\white) if not already there returning it's index through <paramref name="index"/>.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="color">The color to add (0 for black; 1 for white)</param>
        /// <param name="index">The index of the color.</param>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapAddBlackOrWhite")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapAddBlackOrWhite(NativeHandle cmap, int color, out int index);

        /// <summary>
        /// Sets the darkest color in the colormap to black, if <paramref name="setBlack"/> is 1.
        /// Sets the lightest color in the colormap to white if <paramref name="setWhite"/> is 1.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="setBlack">0 for no operation; 1 to set darket color to black</param>
        /// <param name="setWhite">0 for no operation; 1 to set lightest color to white</param>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapSetBlackAndWhite")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapSetBlackAndWhite(NativeHandle cmap, int setBlack, int setWhite);

        // color access - color entry access

        /// <summary>
        /// Gets the color at the specified index.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="index">The index of the color entry.</param>
        /// <param name="redValue">The color entry's red value.</param>
        /// <param name="blueValue">The color entry's blue value.</param>
        /// <param name="greenValue">The color entry's green value.</param>
        /// <returns>Returns 0 if OK; 1 if not accessable (caller should check).</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetColor(NativeHandle cmap, int index, out int redValue, out int blueValue, out int greenValue);

        /// <summary>
        /// Gets the color at the specified index.
        /// </summary>
        /// <remarks>
        /// The alpha channel will always be zero as it is not used in Leptonica color maps.
        /// </remarks>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="index">The index of the color entry.</param>
        /// <param name="color">The color entry as 32 bit value</param>
        /// <returns>Returns 0 if OK; 1 if not accessable (caller should check).</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetColor32")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetColor32(NativeHandle cmap, int index, out int color);

        /// <summary>
        /// Sets a previously allocated color entry.
        /// </summary>
        /// <param name="cmap">The pointer to the colormap instance.</param>
        /// <param name="index">The index of the colormap entry</param>
        /// <param name="redValue"></param>
        /// <param name="blueValue"></param>
        /// <param name="greenValue"></param>
        /// <returns>Returns 0 if OK; 1 if not accessable (caller should check).</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapResetColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapResetColor(NativeHandle cmap, int index, int redValue, int blueValue, int greenValue);

        /// <summary>
        /// Gets the index of the color entry with the specified color, return 0 if found; 1 if not.
        /// </summary>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetIndex")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetIndex(NativeHandle cmap, int redValue, int blueValue, int greenValue, out int index);


        /// <summary>
        /// Returns 0 if the color exists in the color map; otherwise 1.
        /// </summary>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapHasColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapHasColor(NativeHandle cmap, int color);


        /// <summary>
        /// Returns the number of unique grey colors including black and white.
        /// </summary>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapCountGrayColors")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapCountGrayColors(NativeHandle cmap, out int ngray);

        /// <summary>
        /// Finds the index of the color entry with the rank intensity.
        /// </summary>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetRankIntensity")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetRankIntensity(NativeHandle cmap, float rankVal, out int index);


        /// <summary>
        /// Finds the index of the color entry closest to the specified color.
        /// </summary>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetNearestIndex")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetNearestIndex(NativeHandle cmap, int rVal, int bVal, int gVal, out int index);

        /// <summary>
        /// Finds the index of the color entry closest to the specified color.
        /// </summary>
        /// <remarks>
        /// Should only be used on gray colormaps.
        /// </remarks>
        /// <returns>Returns 0 if OK; 1 on error.</returns>
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGetNearestGrayIndex")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGetNearestGrayIndex(NativeHandle cmap, int val, out int index);

        // color map conversion

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGrayToColor")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapGrayToColor(int color);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapColorToGray")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapColorToGray(NativeHandle cmaps, float redWeight, float greenWeight, float blueWeight);

        // colormap serialization

        // NOTE: leptonica's real pixcmapToArrays(cmap, l_int32**, l_int32**, l_int32**, l_int32**)
        // takes 4 out arrays (r/g/b/a), not the 3 declared here -- pre-existing signature
        // mismatch, not fixed here since this method isn't called anywhere in this codebase
        // (internal class, unreachable from outside this assembly either); only the EntryPoint
        // -- previously wrongly aliased onto pixcmapColorToGray -- is fixed, since that one was
        // actively breaking a wasm build (two different C# signatures can't share one wasm
        // pinvoke-table entry, unlike on desktop where it silently pointed both callers at
        // pixcmapColorToGray's implementation instead of pixcmapToArrays' own).
        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapToArrays")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapToArrays(NativeHandle cmap, out IntPtr redMap, out IntPtr blueMap, out IntPtr greenMap);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapToRGBTable")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapToRGBTable(NativeHandle cmap, out IntPtr colorTable, out int colorCount);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapSerializeToMemory")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapSerializeToMemory(NativeHandle cmap, out int components, out int colorCount, out IntPtr colorData, out int colorDataLength);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapDeserializeFromMemory")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr pixcmapDeserializeFromMemory(NativeHandle colorData, int colorCount, int colorDataLength);

        // colormap transformations

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapGammaTRC")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapGammaTRC(NativeHandle cmap, float gamma, int minVal, int maxVal);


        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapContrastTRC")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapContrastTRC(NativeHandle cmap, float factor);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "pixcmapShiftIntensity")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int pixcmapShiftIntensity(NativeHandle cmap, float fraction);


        #endregion

        #region Box

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "boxaGetCount")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int boxaGetCount(NativeHandle boxa);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "boxaGetBox")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr boxaGetBox(NativeHandle boxa, int index, PixArrayAccessType accesstype);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "boxGetGeometry")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial int boxGetGeometry(NativeHandle box, out int px, out int py, out int pw, out int ph);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "boxDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void boxDestroy(ref IntPtr box);

        [LibraryImport(Constants.LeptonicaDllName, EntryPoint = "boxaDestroy")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void boxaDestroy(ref IntPtr box);

        #endregion
    }
}
