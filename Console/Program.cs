﻿using Adglopez.ServiceDocumenter.Core.Metadata;

namespace Adglopez.ServiceDocumenter.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            var url = args.Length > 0 ? args[0] : "http://localhost/Adglopez.Documenter.Samples/Employees.svc";
            var document = args.Length > 1 ? args[1] : "Employee.xlsx";

            var metadataReader = new MetadataReader();

            var exporter = new Exporters.Excel.Expoter();

            var serviceInfo = metadataReader.ParseMetadata(url);

            exporter.Export(serviceInfo, document);

            System.Console.WriteLine("Exporting {0} to {1} completed...", url, document);
        }
    }
}
