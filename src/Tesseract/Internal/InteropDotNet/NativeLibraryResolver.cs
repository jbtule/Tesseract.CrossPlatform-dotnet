using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace InteropDotNet
{
    /// <summary>
    /// Registers <see cref="LibraryLoader.Resolve"/> as this assembly's
    /// <see cref="DllImportResolver"/>. Called from a static constructor on both
    /// <c>Tesseract.Interop.TessApi</c> and <c>Tesseract.Interop.LeptonicaApi</c> -- the two
    /// classes whose <c>[DllImport]</c> methods rely on it -- so registration is guaranteed to
    /// run before either class's first P/Invoke, regardless of which one is touched first (C#
    /// guarantees a type's static constructor runs before any of its static members are first
    /// accessed). Guarded so it's safe to call from both places: registering two different
    /// delegate instances for the same assembly throws, but calling this method more than once
    /// only ever registers the same <see cref="LibraryLoader.Resolve"/> method group once.
    /// </summary>
    internal static class NativeLibraryResolver
    {
        private static int initialized;

        public static void Initialize()
        {
            if (Interlocked.Exchange(ref initialized, 1) != 0)
                return;

            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, LibraryLoader.Resolve);
        }
    }
}
