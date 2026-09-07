//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT
using System;
using System.Runtime.InteropServices;

namespace InteropDotNet
{
    static class SystemManager
    {
        public static string GetPlatformName()
        {
            // IntPtr.Size alone can't tell x64 from arm64 (both are 8 bytes),
            // so use the real process architecture instead.
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    return "x86";
                case Architecture.X64:
                    return "x64";
                case Architecture.Arm:
                    return "arm";
                case Architecture.Arm64:
                    return "arm64";
                default:
                    return IntPtr.Size == sizeof(int) ? "x86" : "x64";
            }
        }

        /// <summary>
        /// Computes a NuGet-style Runtime Identifier ("win-x64", "linux-arm64", "osx-arm64",
        /// etc.) from the actual OS + process architecture, for LibraryLoader's automatic
        /// "runtimes/&lt;rid&gt;/native" fallback location. Returns null where this isn't
        /// knowable (classic .NET Framework, or an OS/arch combination not recognized here) --
        /// callers should treat that as "skip this check", not fail.
        /// </summary>
        public static string GetRuntimeIdentifier()
        {
            string os;
            switch (GetOperatingSystem())
            {
                case OperatingSystem.Windows: os = "win"; break;
                case OperatingSystem.Unix: os = "linux"; break;
                case OperatingSystem.MacOSX: os = "osx"; break;
                default: return null;
            }

            string arch;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86: arch = "x86"; break;
                case Architecture.X64: arch = "x64"; break;
                case Architecture.Arm: arch = "arm"; break;
                case Architecture.Arm64: arch = "arm64"; break;
                default: return null;
            }

            return os + "-" + arch;
        }

        public static OperatingSystem GetOperatingSystem()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OperatingSystem.Windows;
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OperatingSystem.Unix;
            if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OperatingSystem.MacOSX;

            return OperatingSystem.Unknown;
        }
    }

    enum OperatingSystem
    {
        Windows,
        Unix,
        MacOSX,
        Unknown
    }
}