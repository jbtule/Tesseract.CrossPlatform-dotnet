//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Tesseract;
using Tesseract.Internal;

namespace InteropDotNet
{
    /// <summary>
    /// Resolves and loads the native tesseract/leptonica libraries, and serves as the
    /// <see cref="DllImportResolver"/> registered for this assembly via
    /// <see cref="NativeLibrary.SetDllImportResolver"/> -- see <see cref="NativeLibraryResolver"/>
    /// for registration. Replaces the previous OS-specific dlopen/LoadLibrary P/Invoke
    /// wrappers (<c>ILibraryLoaderLogic</c> and friends) with <see cref="NativeLibrary"/>, which
    /// already abstracts that cross-platform since .NET Core 3.0.
    /// </summary>
    public static class LibraryLoader
    {
        private static readonly object syncLock = new object();
        private static readonly Dictionary<string, IntPtr> loadedAssemblies = new Dictionary<string, IntPtr>();
        private static string customSearchPath;

        public static string CustomSearchPath
        {
            get { return customSearchPath; }
            set { customSearchPath = value; }
        }

        /// <summary>
        /// The <see cref="DllImportResolver"/> callback registered for this assembly. Only
        /// resolves the tesseract/leptonica library names this package ships; returns
        /// <see cref="IntPtr.Zero"/> for anything else so the runtime's own default resolution
        /// still applies to it.
        /// </summary>
        internal static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != Tesseract.Interop.Constants.TesseractDllName && libraryName != Tesseract.Interop.Constants.LeptonicaDllName)
                return IntPtr.Zero;

            // Confirmed via a real spike (see tesseract-nuget-platforms' Blazor WASM backlog
            // plan): the resolver *is* invoked under browser-wasm even for a statically-linked
            // (NativeFileReference) native module -- there's no filesystem to probe there, and
            // LoadLibrary's search order would otherwise throw. Returning IntPtr.Zero lets the
            // runtime's own default resolution find the statically-linked symbols directly,
            // which it does successfully once given the chance.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
                return IntPtr.Zero;

            return LoadLibrary(libraryName);
        }

        public static IntPtr LoadLibrary(string fileName, string platformName = null)
        {
            fileName = FixUpLibraryName(fileName);
            lock (syncLock)
            {
                if (!loadedAssemblies.ContainsKey(fileName))
                {
                    if (platformName == null)
                        platformName = SystemManager.GetPlatformName();

                    Logger.TraceInformation("Current platform: " + platformName);

                    IntPtr dllHandle = CheckCustomSearchPath(fileName, platformName);
                    if (dllHandle == IntPtr.Zero)
                        dllHandle = CheckNuGetRuntimesFolder(fileName, platformName);
                    if (dllHandle == IntPtr.Zero)
                        dllHandle = CheckExecutingAssemblyDomain(fileName, platformName);
                    if (dllHandle == IntPtr.Zero)
                        dllHandle = CheckCurrentAppDomain(fileName, platformName);
                    if (dllHandle == IntPtr.Zero)
                        dllHandle = CheckCurrentAppDomainBin(fileName, platformName);
                    if (dllHandle == IntPtr.Zero)
                        dllHandle = CheckWorkingDirecotry(fileName, platformName);

                    if (dllHandle != IntPtr.Zero)
                        loadedAssemblies[fileName] = dllHandle;
                    else
                        // Thrown directly here, rather than returning IntPtr.Zero from Resolve()
                        // and letting NativeLibrary.SetDllImportResolver's caller fall through to
                        // the runtime's own default probing: that would replace this diagnostic
                        // message with a generic DllNotFoundException after redundant extra
                        // probing that's already been done above.
                        throw new DllNotFoundException(string.Format("Failed to find library \"{0}\" for platform {1}.", fileName, platformName));
                }

                return loadedAssemblies[fileName];
            }
        }

        private static IntPtr CheckCustomSearchPath(string fileName, string platformName)
        {
            var baseDirectory = CustomSearchPath;
            if (!String.IsNullOrEmpty(baseDirectory)) {
                Logger.TraceInformation("Checking custom search location '{0}' for '{1}' on platform {2}.", baseDirectory, fileName, platformName);
                // CustomSearchPath is set explicitly by the caller, so it should
                // mean exactly what it says: look here for the library. Check
                // the path directly first, rather than unconditionally forcing
                // a platform-name subfolder underneath it (that behavior is
                // still useful for the automatic fallback locations below,
                // which the caller doesn't control the layout of -- it's just
                // surprising for a path the caller picked on purpose).
                var directPath = Path.Combine(baseDirectory, fileName);
                if (File.Exists(directPath) && NativeLibrary.TryLoad(directPath, out var handle))
                    return handle;
                return InternalLoadLibrary(baseDirectory, platformName, fileName);
            } else {
                Logger.TraceInformation("Custom search path is not defined, skipping.");
                return IntPtr.Zero;
            }

        }

        /// <summary>
        /// Checks the NuGet RID-graph convention -- "&lt;app base dir&gt;/runtimes/&lt;rid&gt;/native/&lt;file&gt;"
        /// -- that `dotnet publish` (and single-RID build/run) populates automatically for any
        /// referenced runtime package (e.g. Tesseract.Native). This is what makes native NuGet
        /// runtime packages "just work" with no caller-side setup at all: the RID is computed
        /// lazily right here, on first actual LoadLibrary call, not eagerly at startup.
        /// </summary>
        private static IntPtr CheckNuGetRuntimesFolder(string fileName, string platformName)
        {
            var rid = SystemManager.GetRuntimeIdentifier();
            if (String.IsNullOrEmpty(rid))
            {
                Logger.TraceInformation("Could not determine a NuGet RID for this process, skipping.");
                return IntPtr.Zero;
            }

            var baseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            var fullPath = Path.Combine(baseDirectory, "runtimes", rid, "native", fileName);
            Logger.TraceInformation("Checking NuGet runtimes folder '{0}' for '{1}' on platform {2}.", fullPath, fileName, platformName);
            return File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out var handle) ? handle : IntPtr.Zero;
        }

        // Assembly.Location always returns "" for a single-file/NativeAOT publish (flagged by
        // the IL3000 trim analyzer) -- there's no on-disk assembly file to report a path for.
        // Suppressed rather than avoided: this method's whole purpose is covering the case where
        // this assembly's own file lives somewhere other than the app's base directory (e.g. a
        // shared lib folder), which legitimately needs .Location; the empty-Location guard below
        // just turns "nothing to report" into a clean no-op instead of a bogus relative path,
        // for the single-file/AOT case where CheckCurrentAppDomain (AppDomain.BaseDirectory)
        // already covers the app's own directory anyway.
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("SingleFile", "IL3000",
            Justification = "Guarded by the IsNullOrEmpty check immediately below; single-file/AOT publishes just skip this check.")]
        private static IntPtr CheckExecutingAssemblyDomain(string fileName, string platformName)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            if(executingAssembly == null) {
                // #591 Executing assembly may be null in some cases
                return IntPtr.Zero;
            }

            var location = executingAssembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                Logger.TraceInformation("Executing assembly has no on-disk location (single-file/AOT publish), skipping.");
                return IntPtr.Zero;
            }

            var baseDirectory = Path.GetDirectoryName(location);
            Logger.TraceInformation("Checking executing application domain location '{0}' for '{1}' on platform {2}.", baseDirectory, fileName, platformName);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        private static IntPtr CheckCurrentAppDomain(string fileName, string platformName)
        {
            var baseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            Logger.TraceInformation("Checking current application domain location '{0}' for '{1}' on platform {2}.", baseDirectory, fileName, platformName);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        /// <summary>
        /// Special test for web applications.
        /// </summary>
        /// <remarks>
        /// Note that this makes a couple of assumptions these being:
        ///
        /// <list type="bullet">
        ///     <item>That the current application domain's location for web applications corresponds to the web applications root directory.</item>
        ///     <item>That the tesseract\leptonica dlls reside in the corresponding x86 or x64 directories in the bin directory under the apps root directory.</item>
        /// </list>
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>
        private static IntPtr CheckCurrentAppDomainBin(string fileName, string platformName)
        {
            var baseDirectory = Path.Combine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "bin");
            if (Directory.Exists(baseDirectory)) {
                Logger.TraceInformation("Checking current application domain's bin location '{0}' for '{1}' on platform {2}.", baseDirectory, fileName, platformName);
                return InternalLoadLibrary(baseDirectory, platformName, fileName);
            } else {
                Logger.TraceInformation("No bin directory exists under the current application domain's location, skipping.");
                return IntPtr.Zero;
            }
        }

        private static IntPtr CheckWorkingDirecotry(string fileName, string platformName)
        {
            var baseDirectory = Path.GetFullPath(Environment.CurrentDirectory);
            Logger.TraceInformation("Checking working directory '{0}' for '{1}' on platform {2}.", baseDirectory, fileName, platformName);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        private static IntPtr InternalLoadLibrary(string baseDirectory, string platformName, string fileName)
        {
            // Try the flat path first: `dotnet publish -r <rid>` (the standard,
            // documented way to consume a RID-specific native NuGet package,
            // self-contained or not) copies runtime assets straight into the
            // output root alongside the app itself, NOT nested under a
            // platform-name subfolder -- confirmed empirically, not assumed.
            // Falls back to the legacy nested-by-platform-name layout for
            // anyone relying on that (this is what all four automatic
            // fallback locations use, so this one change covers all of them).
            var flatPath = Path.Combine(baseDirectory, fileName);
            if (File.Exists(flatPath) && NativeLibrary.TryLoad(flatPath, out var flatHandle))
                return flatHandle;

            var fullPath = Path.Combine(baseDirectory, Path.Combine(platformName, fileName));
            return File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out var nestedHandle) ? nestedHandle : IntPtr.Zero;
        }

        private static string FixUpLibraryName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            if (SystemManager.GetOperatingSystem() == OperatingSystem.Windows)
            {
                if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    fileName += ".dll";
                return fileName;
            }

            var extension = SystemManager.GetOperatingSystem() == OperatingSystem.MacOSX ? ".dylib" : ".so";
            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                fileName += extension;
            if (!fileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
                fileName = "lib" + fileName;
            return fileName;
        }
    }
}
