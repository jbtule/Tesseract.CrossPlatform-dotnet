using System.IO;

internal static class Program
{
    private static int Main(string[] args)
    {
        var jsonOutputPath = args.Length > 0 ? args[0] : null;
        using (var jsonOutputStream = jsonOutputPath != null ? File.Create(jsonOutputPath) : null)
        {
            // AnyUnit.Util.PlatformId.Current (added in AnyUnit 1.1) - see
            // Tesseract.Tests's own identical Program.cs for the full reasoning
            // (a hardcoded "net10" collides across this project's own 5-RID matrix
            // once results.json files get merged by AnyUnit.Report).
            return AnyUnit.Runner.Bootstrap.Runner.Run(AnyUnit.Util.PlatformId.Current, jsonOutputStream: jsonOutputStream);
        }
    }
}
