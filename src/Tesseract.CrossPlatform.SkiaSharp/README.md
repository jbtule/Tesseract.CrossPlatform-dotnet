# Tesseract.CrossPlatform.SkiaSharp

SkiaSharp-based `Pix`/`SKBitmap` interop for [Tesseract.CrossPlatform](https://www.nuget.org/packages/Tesseract.CrossPlatform) — a cross-platform (`browser-wasm` included) alternative to `Tesseract.Drawing`'s Windows-only `System.Drawing.Common`-based converters.

Install this alongside `Tesseract.CrossPlatform` when you need to decode image files (PNG, JPEG, etc.) rather than already-decoded pixels — most useful under `browser-wasm`, where [Tesseract.Native.browser-wasm](https://www.nuget.org/packages/Tesseract.Native.browser-wasm) ships with no image codecs of its own.

See [the repo](https://github.com/jbtule/tesseract-nuget-platforms) for usage.
