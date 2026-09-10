A .NET wrapper for [tesseract-ocr] 5.

## About this fork

This is [jbtule](https://github.com/jbtule)'s fork of
[charlesw/tesseract](https://github.com/charlesw/tesseract), owned
independently going forward rather than staged as patches for an eventual
upstream PR -- upstream hasn't been updated in about 2 years, and changes
here go beyond what's realistic to get merged upstream anyway. Retargeted
from `netstandard2.0;net47;net48` to `net8.0;net9.0;net10.0` only. The
Reflection.Emit-based interop layer (`RuntimeDllImportAttribute` /
`InteropRuntimeImplementer`) was first replaced with plain `[DllImport]` +
`NativeLibrary.SetDllImportResolver`, then further converted to
source-generated `[LibraryImport]` + `SafeHandle`-derived native handles
(needed for real browser-wasm support: `[DllImport]`'s dynamic marshaling
path crashes Mono's wasm interpreter, confirmed the hard way); see
[jbtule/tesseract-nuget-platforms](https://github.com/jbtule/tesseract-nuget-platforms)'s
README ("Our fork of charlesw/tesseract") for the full writeup.

It's built and packaged, alongside prebuilt cross-platform native
tesseract/leptonica binaries, by
[jbtule/tesseract-nuget-platforms](https://github.com/jbtule/tesseract-nuget-platforms)
into the `Tesseract.CrossPlatform` and `Tesseract.Native` NuGet packages --
see that repo for usage, package details, and the list of fixes made here
(arm64 platform detection, native library search order, generic native
library names). If you're consuming this via NuGet, use those packages
instead of building from this repo directly.

Upstream's original README is preserved unchanged in
[ReadMe.orig.md](ReadMe.orig.md) for reference -- it may not reflect this
fork's actual setup requirements (e.g. this fork doesn't require a
separately-installed Visual Studio runtime; native binaries are
self-contained and resolved via `Tesseract.Native`). It also still carries
the original Apache-2.0/InteropDotNet license notices and contributor
attribution, both still accurate and worth keeping.
