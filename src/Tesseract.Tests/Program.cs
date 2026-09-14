using System.IO;

internal static class Program
{
    private static int Main(string[] args)
    {
        var jsonOutputPath = args.Length > 0 ? args[0] : null;
        using (var jsonOutputStream = jsonOutputPath != null ? File.Create(jsonOutputPath) : null)
        {
            // AnyUnit.Util.PlatformId.Current (added in AnyUnit 1.1), not a hardcoded
            // "net10": this project runs the same way across 5 different RIDs in CI
            // (linux-x64/linux-arm64/osx-arm64/win-x64/win-arm64) - a shared literal
            // means every RID's own result reports the identical Platform value, which
            // silently collides once results.json files from more than one RID get
            // merged (AnyUnit.Report's ResultsFile.Add dedups a test's Results by exact
            // Platform equality - a second RID's "net10" is just dropped, not merged).
            // PlatformId.Current instead yields "net10-linux-x64", "net10-win-arm64",
            // etc. - real per-RID identity, matching what every other AnyUnit runner
            // (anyunit-runner, the MTP adapter, the browser-wasm runner) already does.
            return AnyUnit.Runner.Bootstrap.Runner.Run(AnyUnit.Util.PlatformId.Current, jsonOutputStream: jsonOutputStream);
        }
    }
}
