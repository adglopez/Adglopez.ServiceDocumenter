using System;
using System.IO;
using Adglopez.ServiceDocumenter.Core.Metadata;
using Adglopez.ServiceDocumenter.Core.Model;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public class Expoter : IExporter
    {
        public void Export(Service service, string outputLocation)
        {
            if (File.Exists(outputLocation))
            {
                File.Delete(outputLocation);
            }

            using (var workBook = new ClosedXML.Excel.XLWorkbook(outputLocation))
            {
                Console.WriteLine(workBook.Worksheets.Count);
            }
        }
    }
}
