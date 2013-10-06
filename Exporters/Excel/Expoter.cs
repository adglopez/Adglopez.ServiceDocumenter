﻿using System.Linq;
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
                
                for(int idx = 0; idx < service.Endpoints.Count ; idx++)
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
                    var opeartionWorksheet = workBook.Worksheet("Operation").CopyTo(opeartionWorksheetName);

                    opeartionWorksheet.Cell(1, 1).Value = "Operation";
                    opeartionWorksheet.Cell(1, 1).Style.Font.Bold = true;
                    opeartionWorksheet.Cell(1, 2).Value = operation.Name;

                    // Operation (detailed info)
                    const int initialRow = 4;
                    int currentRow = initialRow;
                    // Write Input Messages
                    foreach (var message in operation.Input)
                    {
                        currentRow = WriteMessage(message, opeartionWorksheet, currentRow, true);
                    }
                    
                    // Write Return Message (if present)
                    WriteMessage(operation.Output.FirstOrDefault(), opeartionWorksheet, currentRow, false);
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
            worksheet.Cell(currentRow, 1).Value = "Fields";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = "List of fields goes here";

            if (message.IsComplex)
            {
                foreach (var property in message.Properties)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = property.Key;
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 2).Value = property.Value;
                }
            }

            
            currentRow+=2;
            worksheet.Cell(currentRow, 1).Value = string.Empty;
            
            return currentRow;
        }
    }
}
