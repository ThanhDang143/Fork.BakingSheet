// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

namespace Cathei.BakingSheet.Raw
{
    /// <summary>
    /// Single page of a Spreadsheet workbook for exporting.
    /// </summary>
    public interface IRawSheetExporterPage
    {
        void SetCell(int col, int row, string data);
    }
}
