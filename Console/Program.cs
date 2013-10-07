namespace Console
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var url = args.Length > 0 ? args[0] : "http://localhost:56137/Employees.svc";
            var document = args.Length > 1 ? args[1] : "Employee Service.xlsx";

            var metadataReader = new Adglopez.ServiceDocumenter.Core.Metadata.MetadataReader();
            var exporter = new Adglopez.ServiceDocumenter.Exporters.Excel.Expoter();

            var serviceInfo = metadataReader.ParseMetadata(url);

            exporter.Export(serviceInfo, document);

            System.Console.WriteLine("Exporting {0} to {1} completed...", url, document);

            System.Diagnostics.Process.Start("Excel.exe",string.Format("\"{0}\"", document));
        }
    }
}
