To get .net wrapper for Tesseract working on macOS tesseract and leptonica libraries need to installed and linked in project.

# Install libraries via Brew
```sh
brew install leptonica tesseract
```

# Add symlinks to output folder
Add the following Target to the csproj.

Apple silicon Mac

Note the `arm64` folder below (not `x64`) — on .NET Core/.NET 5+ the library
loader uses the real process architecture, and an Apple Silicon process
reports as `arm64`.
```xml
<Target Name="link_deps" AfterTargets="AfterBuild">
  <Exec Command="ln -sf /opt/homebrew/lib/libleptonica.dylib $(OutDir)arm64/libleptonica-1.82.0.dylib" />
  <Exec Command="ln -sf /opt/homebrew/lib/libtesseract.dylib $(OutDir)arm64/libtesseract50.dylib" />
</Target>
```

Intel Mac
```xml
<Target Name="link_deps" AfterTargets="AfterBuild">
  <Exec Command="ln -sf /usr/local/lib/liblept.dylib $(OutDir)x64/libleptonica-1.82.0.dylib"/>
  <Exec Command="ln -sf /usr/local/lib/libtesseract.dylib $(OutDir)x64/libtesseract50.dylib"/>
</Target>
```
