namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public static class StringExtensions
    {
        public static string FromLast(this string input, string value)
        {
            var idx = input.LastIndexOf(value, System.StringComparison.Ordinal);
            var result = idx < 0 ? input : input.Substring(idx);
            return result;
        }
    }
}
