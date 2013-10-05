using System;
using Adglopez.ServiceDocumenter.Core.Metadata;
using Adglopez.ServiceDocumenter.Core.Model;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public class Expoter : IExporter
    {
        public void Export(Service service)
        {
            Console.WriteLine(service.Namespace + "." + service.Namespace);
        }
    }
}
