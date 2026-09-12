internal static class Program
{
    private static int Main(string[] args)
    {
        var jsonOutputPath = args.Length > 0 ? args[0] : null;
        return AnyUnit.Runner.Bootstrap.Runner.Run("net10", jsonOutputPath: jsonOutputPath);
    }
}
