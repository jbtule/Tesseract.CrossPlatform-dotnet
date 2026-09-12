using System.IO;

internal static class Program
{
    private static int Main(string[] args)
    {
        var jsonOutputPath = args.Length > 0 ? args[0] : null;
        using (var jsonOutputStream = jsonOutputPath != null ? File.Create(jsonOutputPath) : null)
        {
            return AnyUnit.Runner.Bootstrap.Runner.Run("net10", jsonOutputStream: jsonOutputStream);
        }
    }
}
