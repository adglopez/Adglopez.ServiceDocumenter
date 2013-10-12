using System.Collections.Generic;
using System.Linq;

using ClosedXML.Excel;
using Adglopez.ServiceDocumenter.Core.Model;
using Adglopez.ServiceDocumenter.Core.Metadata;

namespace Adglopez.ServiceDocumenter.Exporters.Excel
{
    public class Expoter : IExporter
    {
        public void Export(Service service, string connectionString)
        {
            using (var workBook = new ClosedXML.Excel.XLWorkbook())
            {
                // "Summary" worksheet.
                WriteSummary(service, workBook);

                // Creates a worksheet per Operation. This also renders messages schema
                foreach (var operation in service.Operations)
                {
                    WriteOperation(operation, workBook);
                }

                workBook.Save(connectionString);
            }
        }

        #region Render Information
        private void WriteSummary(Service service, XLWorkbook workBook)
        {
            var summaryWorksheet = workBook.Worksheets.Add("Summary");

            summaryWorksheet.Cell(1, 1).Value = "service.Name";
            summaryWorksheet.Cell(2, 1).Value = "Namespace";
            summaryWorksheet.Cell(3, 1).Value = "Url";
            summaryWorksheet.Cell(4, 1).Value = "Contract";
            summaryWorksheet.Cell(4, 1).Value = "Endpoints";
            summaryWorksheet.Cell(5, 2).Value = "Name";
            summaryWorksheet.Cell(5, 3).Value = "Binding";
            summaryWorksheet.Cell(5, 4).Value = "Address";

            summaryWorksheet.Cell(1, 2).Value = service.Name;
            summaryWorksheet.Cell(2, 2).Value = service.Namespace;
            summaryWorksheet.Cell(3, 2).Value = service.Url;
            summaryWorksheet.Cell(4, 2).Value = service.Contract;

            StyleToInfoLabel(summaryWorksheet.Cell(1, 1));
            StyleToInfoLabel(summaryWorksheet.Cell(2, 1));
            StyleToInfoLabel(summaryWorksheet.Cell(3, 1));
            StyleToInfoLabel(summaryWorksheet.Cell(4, 1));
            StyleToInfoLabel(summaryWorksheet.Cell(4, 1));
            StyleToInfoLabel(summaryWorksheet.Cell(5, 2));
            StyleToInfoLabel(summaryWorksheet.Cell(5, 3));
            StyleToInfoLabel(summaryWorksheet.Cell(5, 4));

            for (int idx = 0; idx < service.Endpoints.Count; idx++)
            {
                summaryWorksheet.Cell(6 + idx, 2).Value = service.Endpoints[idx].Name;
                summaryWorksheet.Cell(6 + idx, 3).Value = string.Format("{0} ({1})", service.Endpoints[idx].Binding.Name, service.Endpoints[idx].Binding.Type);
                summaryWorksheet.Cell(6 + idx, 4).Value = service.Endpoints[idx].Address;
            }

            summaryWorksheet.Column(1).AdjustToContents();
            summaryWorksheet.Column(2).AdjustToContents();
            summaryWorksheet.Column(3).AdjustToContents();
            summaryWorksheet.Column(4).AdjustToContents();
        }

        private void WriteOperation(Operation operation, XLWorkbook workBook)
        {
            var name = operation.Name;

            // This is due to a limit in the lenght of the spreadsheets names
            if (name.Length >= 31)
            {
                // TODO: This should be determined automatically.
                name = name.Substring(0, 25);
            }
            // Write Opeation (general info)
            var operationWorksheetName = workBook.Worksheets.EnsureUniqueName(name);
            var operationWorksheet = workBook.Worksheets.Add(operationWorksheetName);

            operationWorksheet.Cell(1, 1).Value = "Operation";
            StyleToInfoLabel(operationWorksheet.Cell(1, 1));
            operationWorksheet.Cell(1, 2).Value = operation.Name;

            // Operation (detailed info)
            const int initialRow = 4;
            int currentRow = initialRow;

            operationWorksheet.Cell(currentRow, 1).Value = "INPUTS";
            StyleToDirectionType(operationWorksheet.Cell(currentRow, 1));

            currentRow++;

            // Write Input Messages
            if (operation.Input.Count == 0)
            {
                operationWorksheet.Cell(currentRow, 2).Value = "No inputs are received";
                currentRow += 3;
            }
            else
            {
                currentRow = operation.Input.Aggregate(currentRow, (current, type) => WriteMessage(type, operationWorksheet, current, true));
            }

            // Write Ouptus Messages
            operationWorksheet.Cell(currentRow, 1).Value = "OUTPUTS";
            StyleToDirectionType(operationWorksheet.Cell(currentRow, 1));
            currentRow++;

            if (operation.Output.Count == 0)
            {
                operationWorksheet.Cell(currentRow, 2).Value = "No outputs are returned";
                currentRow += 3;
            }
            else
            {
                currentRow = operation.Output.Aggregate(currentRow, (current, type) => WriteMessage(type, operationWorksheet, current, false));
            }

            operationWorksheet.Cell(currentRow + 1, 1).Value = string.Empty;
            operationWorksheet.Column(1).AdjustToContents();
            operationWorksheet.Column(2).AdjustToContents();
            operationWorksheet.Column(3).AdjustToContents();
            operationWorksheet.Column(4).AdjustToContents();
        }

        private int WriteMessage(ParameterType message, IXLWorksheet worksheet, int currentRow, bool isInput)
        {
            if (message == null)
            {
                return currentRow;
            }

            worksheet.Cell(currentRow, 1).Value = "Message";
            StyleToBold(worksheet.Cell(currentRow, 1));

            worksheet.Cell(currentRow, 2).Value = isInput ? message.Name : "response";

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Type";
            StyleToBold(worksheet.Cell(currentRow, 1));

            worksheet.Cell(currentRow, 2).Value = message.TypeName;

            currentRow++;
            worksheet.Cell(currentRow, 1).Value = "Mandatory";
            StyleToBold(worksheet.Cell(currentRow, 1));

            worksheet.Cell(currentRow, 2).Value = message.IsOptional ? "No" : "Yes";

            if (!message.IsComplex)
            {
                currentRow++;
                return currentRow;
            }

            currentRow += 2;

            worksheet.Cell(currentRow, 1).Value = "Properties";
            StyleToBold(worksheet.Cell(currentRow, 1));

            foreach (var property in message.Properties)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = property.Key.PrependDeepLevelIndicator(1);
                StyleToBold(worksheet.Cell(currentRow, 1));
                worksheet.Cell(currentRow, 2).Value = property.Value;
            }

            currentRow = message.Childs.Aggregate(currentRow, (current, child) => WriteComplexType(child, worksheet, current, 1));

            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = string.Empty;

            return currentRow;
        }

        private int WriteComplexType(KeyValuePair<string, ParameterType> child, IXLWorksheet worksheet, int currentRow, int deep)
        {
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = child.Key.PrependDeepLevelIndicator(deep);
            StyleToBold(worksheet.Cell(currentRow, 1));

            worksheet.Cell(currentRow, 2).Value = child.Value.TypeName;
            StyleToBold(worksheet.Cell(currentRow, 2));

            foreach (var property in child.Value.Properties)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = property.Key.PrependDeepLevelIndicator(deep + 1);
                worksheet.Cell(currentRow, 2).Value = property.Value;
                StyleToBold(worksheet.Cell(currentRow, 1));
            }

            return child.Value.Childs.Aggregate(currentRow, (current, nextChild) => WriteComplexType(nextChild, worksheet, current, deep + 1));
        } 
        #endregion

        #region Styles
        private void StyleToInfoLabel(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.GoldenYellow;
        }

        private void StyleToBold(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
        }

        private void StyleToDirectionType(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        } 
        #endregion
    }
}
