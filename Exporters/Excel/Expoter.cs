using System.IO;
using Adglopez.ServiceDocumenter.Core.Metadata;
using Adglopez.ServiceDocumenter.Core.Model;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public class Expoter : IExporter
    {
        public void Export(Service service, string connectionString)
        {
            if (File.Exists(connectionString))
            {
                File.Delete(connectionString);
            }


            using (var workBook = new ClosedXML.Excel.XLWorkbook())
            {
                foreach (var operation in service.Operations)
                {
                    
                }

                workBook.SaveAs(connectionString);
            }            
        }
    }
}
