using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public static class ClosedXmlExtensions
    {
        public static string EnsureUniqueName(this IXLWorksheets collection, string name)
        {
            if (collection.Any(w => w.Name == name))
            {
                int counter = 0;

                while (true)
                {
                    counter++;
                    name = name + counter;

                    if (collection.All(w => w.Name != name))
                    {
                        return name;
                    }
                }
            }
            else
            {
                return name;
            }
        }

        public static void Save(this XLWorkbook workbook, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            workbook.SaveAs(filePath);
        }

        public static IXLWorksheet Clone(this IXLWorksheet worksheet, string name)
        {
            return worksheet.CopyTo(worksheet.Workbook, name);
        }
    }
}
