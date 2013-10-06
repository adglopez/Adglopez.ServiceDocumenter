using Adglopez.ServiceDocumenter.Core.Model;
using Adglopez.ServiceDocumenter.Core.Metadata;
using ClosedXML.Excel;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public class Expoter : IExporter
    {
        private const string TemplatePath = @"..\..\..\Assets\Template.xlsx";

        public void Export(Service service, string connectionString)
        {
            using (var workBook = new ClosedXML.Excel.XLWorkbook(TemplatePath))
            {
                var summaryWorksheet = workBook.Worksheet("Summary");

                summaryWorksheet.Cell(1, 2).Value = service.Name = service.Name;
                summaryWorksheet.Cell(2, 2).Value = service.Name = service.Namespace;
                summaryWorksheet.Cell(3, 2).Value = service.Name = service.Url;
                summaryWorksheet.Cell(4, 2).Value = service.Name = service.Contract;

                for (int idx = 0; idx < service.Endpoints.Count; idx++)
                {
                    summaryWorksheet.Cell(6 + idx, 2).Value = service.Endpoints[idx].Name;
                    summaryWorksheet.Cell(6 + idx, 3).Value = string.Format("{0} ({1})", service.Endpoints[idx].Binding.Name, service.Endpoints[idx].Binding.Type);
                    summaryWorksheet.Cell(6 + idx, 4).Value = service.Endpoints[idx].Address;

                    summaryWorksheet.Column(2).AdjustToContents();
                    summaryWorksheet.Column(3).AdjustToContents();
                    summaryWorksheet.Column(4).AdjustToContents();
                }

                foreach (var operation in service.Operations)
                {
                    // Write Opeation (general info)
                    var opeartionWorksheetName = workBook.Worksheets.EnsureUniqueName(operation.Name);
                    var operationWorksheet = workBook.Worksheet("Operation").CopyTo(opeartionWorksheetName);

                    operationWorksheet.Cell(1, 1).Value = "Operation";
                    operationWorksheet.Cell(1, 1).Style.Font.Bold = true;
                    operationWorksheet.Cell(1, 2).Value = operation.Name;

                    // Operation (detailed info)
                    const int initialRow = 4;
                    int currentRow = initialRow;
                    operationWorksheet.Cell(currentRow, 1).Value = "INPUTS";
                    operationWorksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    operationWorksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Gray;
                    currentRow++;

                    // Write Input Messages
                    if (operation.Input.Count == 0)
                    {
                        operationWorksheet.Cell(currentRow, 2).Value = "No inputs are received";
                        currentRow += 3;
                    }
                    else
                    {
                        foreach (ParameterType type in operation.Input)
                        {
                            currentRow = WriteMessage(type, operationWorksheet, currentRow, true);
                        }
                    }

                    // Write Ouptus Messages
                    operationWorksheet.Cell(currentRow, 1).Value = "OUTPUTS";
                    operationWorksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    operationWorksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Gray;
                    currentRow++;

                    if (operation.Output.Count == 0)
                    {
                        operationWorksheet.Cell(currentRow, 2).Value = "No outputs are returned";
                        currentRow += 3;
                    }
                    else
                    {
                        foreach (ParameterType type in operation.Output)
                        {
                            currentRow = WriteMessage(type, operationWorksheet, currentRow, false);
                        }
                    }

                    operationWorksheet.Cell(currentRow + 1, 1).Value = string.Empty;
                    operationWorksheet.Column(2).AdjustToContents();
                }

                workBook.Worksheet("Operation").Delete();

                workBook.Save(connectionString);
            }
        }

        private int WriteMessage(ParameterType message, IXLWorksheet worksheet, int currentRow, bool isInput)
        {
            if (message == null)
            {
                return currentRow;
            }

            worksheet.Cell(currentRow, 1).Value = "Message";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = isInput ? message.Name : "response";

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Type";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = message.TypeName;

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Mandatory";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = message.IsOptional ? "No" : "Yes";

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Properties";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            foreach (var property in message.Properties)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = property.Key.PrependDeepLevelIndicator(1);
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = property.Value;
            }

            if (message.IsComplex)
            {
                foreach (var child in message.Childs)
                {
                    currentRow = WriteComplexType(child, worksheet, currentRow, 1);
                }
            }

            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = string.Empty;

            return currentRow;
        }

        private int WriteComplexType(System.Collections.Generic.KeyValuePair<string, ParameterType> child, IXLWorksheet worksheet, int currentRow, int deep)
        {
            currentRow++;
            worksheet.Cell(currentRow, 1).Value = child.Key.PrependDeepLevelIndicator(deep);
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = child.Value.TypeName;
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            foreach (var property in child.Value.Properties)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = property.Key.PrependDeepLevelIndicator(deep + 1);
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = property.Value;
            }
            foreach (var nextChild in child.Value.Childs)
            {
                currentRow = WriteComplexType(nextChild, worksheet, currentRow, deep + 1);
            }
            return currentRow;
        }
    }
}
