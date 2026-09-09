using System;
using System.Runtime.InteropServices;

namespace Tesseract.Interop
{
    /// <summary>
    /// Wraps a native pointer for P/Invoke marshaling, replacing the previous
    /// <see cref="HandleRef"/>-based approach.
    /// </summary>
    /// <remarks>
    /// <see cref="HandleRef"/> is not supported by source-generated interop
    /// (<c>[LibraryImport]</c>) at all -- <c>SYSLIB1051: The type
    /// 'System.Runtime.InteropServices.HandleRef' is not supported by
    /// source-generated P/Invokes</c> -- which <c>[LibraryImport]</c> itself
    /// is required for under Blazor WebAssembly: Mono's wasm runtime can only
    /// call P/Invoke methods the AOT pinvoke-table generator could
    /// pre-analyze at build time; anything it can't (confirmed via a real
    /// repro -- every <c>[DllImport]</c> method in this assembly triggered a
    /// build-time "unsupported parameter type" warning) falls back to a
    /// dynamic runtime trampoline path that is broken/aborts under Blazor
    /// wasm specifically. <c>[LibraryImport]</c> generates the marshaling as
    /// plain compiled C# instead of relying on that runtime path, sidestepping
    /// the problem entirely -- confirmed directly: a trivial
    /// <c>[LibraryImport]</c> method placed alongside 88 broken
    /// <c>[DllImport]</c> ones on the same class still worked.
    ///
    /// This type constructs with <c>ownsHandle: false</c>, so
    /// <see cref="ReleaseHandle"/> is never invoked by the base class -- it
    /// exists purely so the P/Invoke marshaler has a real, source-generator-
    /// supported handle type to marshal (mirroring <see cref="HandleRef"/>'s
    /// actual role here: keeping the call's argument alive for the duration
    /// of the native call, nothing more). Actual native lifetime management
    /// (calling the matching <c>*Delete</c>/<c>*Destroy</c> function) remains
    /// each wrapper class's own explicit responsibility via its existing
    /// <c>Dispose</c> pattern, unchanged by this type.
    /// </remarks>
    public sealed class NativeHandle : SafeHandle
    {
        public NativeHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: false)
        {
            SetHandle(handle);
        }

        // Mirrors HandleRef.Handle -- SafeHandle only exposes DangerousGetHandle()
        // publicly, and this codebase reads .Handle pervasively (a plain property
        // read is safe here: ownsHandle is always false, so there's no reference
        // count to bypass and no actual "danger" the Dangerous* naming implies).
        public IntPtr Handle => DangerousGetHandle();

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle() => true;
    }
}
