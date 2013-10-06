namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public static class StringExtensions
    {
        public static string PrependTabs(this string input, int times)
        {
            const string tab = "\t";
            var output = string.Empty;
            for (int i = 0; i < times; i++)
            {
                output = tab + output;
            }
            return output + input;
        }
    }
}
