using Common;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LkeServices.Export
{
    public class ExportService
    {
        private ConcurrentDictionary<string, byte[]> _savedData;

        public ExportService()
        {
            _savedData = new ConcurrentDictionary<string, byte[]>();
        }

        public string ExportToExcel<T>(IEnumerable<T> rows, string sheetName = null)
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columnTitles = properties.Select(prop => prop.Name).ToList();
            var dataRows = new List<List<string>>();

            foreach (var row in rows)
            {
                var dataRow = properties.Select(prop => prop.GetValue(row)?.ToString()).ToList();
                dataRows.Add(dataRow);
            }

            return ExportToExcel(dataRows, columnTitles, sheetName);
        }

        public string ExportToExcel(List<List<string>> dataRows, List<string> columnTitles = null, string sheetName = null)
        {
            #region Validation

            if (dataRows == null || dataRows.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(sheetName))
                sheetName = "Sheet1";

            if (columnTitles != null && columnTitles.Count != dataRows[0].Count)
            {
                columnTitles = null;
            }

            #endregion
            
            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

                    // Add a WorkbookPart to the document.
                    WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart.
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Add Sheets to the Workbook.
                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = sheetName
                    };
                    sheets.Append(sheet);



                    int firstRowPos = 1;
                    if (columnTitles != null)
                    {
                        for(var colIdx = 1; colIdx <= columnTitles.Count; colIdx++)
                            worksheetPart.InsertString(colIdx, firstRowPos, columnTitles[colIdx - 1]);

                        firstRowPos++;
                    }


                    for (var rowIdx = firstRowPos; rowIdx < dataRows.Count + firstRowPos; rowIdx++)
                    {
                        for (var colIdx = 1; colIdx <= dataRows[rowIdx - firstRowPos].Count; colIdx++)
                        {
                            worksheetPart.InsertString(colIdx, rowIdx, dataRows[rowIdx - firstRowPos][colIdx - 1]);
                        }
                    }
                    


                    workbookpart.Workbook.Save();
                    spreadsheetDocument.Close();

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var data = memoryStream.ToBytes();
                    var dataId = SaveData(data);

                    return dataId;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public byte[] PopSavedData(string dataId)
        {
            byte[] data;
            if (_savedData.TryRemove(dataId, out data))
            {
                return data;
            }

            return null;
        }

        public string SaveData(byte[] data)
        {
            if (data == null)
                return null;

            var dataId = Guid.NewGuid().ToString();
            _savedData.AddOrUpdate(dataId, data, (key, oldValue) => data);

            return dataId;
        }
    }
}
