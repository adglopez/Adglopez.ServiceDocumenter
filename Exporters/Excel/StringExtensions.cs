namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public static class StringExtensions
    {
        public static string PrependDeepLevelIndicator(this string input, int times)
        {
            const string tab = " -";
            var output = string.Empty;
            for (int i = 0; i < times; i++)
            {
                output = tab + output;
            }
            return output + input;
        }
    }
}
