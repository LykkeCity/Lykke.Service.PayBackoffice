using System;
using System.IO;

namespace BackOffice.Services
{
    //TODO: fix Excel
    public class ExcelWorksheet
    {
    }

    public class ExcelSheetWriter
    {
        internal ExcelWorksheet ExcelWorksheet;

        public ExcelSheetWriter(ExcelWorksheet excelWorksheet)
        {
            ExcelWorksheet = excelWorksheet;
        }

        public void WriteHeader(params string[] data)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(params string[] data)
        {
            throw new NotImplementedException();
        }

    }

    public class ExcelReportBuilder
    {
        public ExcelSheetWriter GetWorkSheet(string name, params string[] header)
        {
            throw new NotImplementedException();
        }

        public Stream GetResult()
        {
            throw new NotImplementedException();
        }
    }
    //public class ExcelSheetWriter
    //{
    //    internal ExcelWorksheet ExcelWorksheet;
    //    private int _currentRow=1;

    //    public ExcelSheetWriter(ExcelWorksheet excelWorksheet)
    //    {
    //        ExcelWorksheet = excelWorksheet;
    //    }

    //    public void WriteHeader(params string[] data)
    //    {
    //        var col = 1;
    //        foreach (var s in data)
    //            ExcelWorksheet.Cells[_currentRow, col++].Value = s;


    //        ExcelWorksheet.Cells[_currentRow, 1, _currentRow, data.Length].Style.Font.Bold = true;

    //        _currentRow++;
    //    }

    //    public void WriteLine(params string[] data)
    //    {
    //        var col = 1;
    //        foreach (var s in data)
    //            ExcelWorksheet.Cells[_currentRow, col++].Value = s;

    //        _currentRow++;
    //    }

    //}

    //public class ExcelReportBuilder
    //{
    //    private readonly ExcelPackage _excelPackage = new ExcelPackage();


    //    private readonly Dictionary<string, ExcelSheetWriter> _sheetWriters = new Dictionary<string, ExcelSheetWriter>();

    //    public ExcelSheetWriter GetWorkSheet(string name, params string[] header)
    //    {

    //        if (_sheetWriters.ContainsKey(name))
    //            return _sheetWriters[name];

    //        var newSheet = new ExcelSheetWriter(_excelPackage.Workbook.Worksheets.Add(name));
    //        _sheetWriters.Add(name, newSheet);
    //        newSheet.WriteHeader(header);
    //        return newSheet;
    //    }

    //    public Stream GetResult()
    //    {

    //        foreach (var value in _sheetWriters.Values)
    //        {
    //            value.ExcelWorksheet.Cells.Style.Font.Size = 9;
    //            value.ExcelWorksheet.Cells.AutoFitColumns();
    //        }
    //        var result = new MemoryStream();
    //        _excelPackage.Save();

    //        _excelPackage.Stream.Position = 0;

    //        _excelPackage.Stream.CopyTo(result);

    //        result.Position = 0;

    //        return result;
    //    }
    //}
}