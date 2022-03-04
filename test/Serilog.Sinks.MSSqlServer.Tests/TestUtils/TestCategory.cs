namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    public static class TestCategory
    {
        public const string TraitName = "Category";

        public const string Integration = nameof(Integration);
        public const string Isolated = nameof(Isolated);
        public const string Unit = nameof(Unit);
    }
}
