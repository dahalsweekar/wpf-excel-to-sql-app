using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OfficeOpenXml;

namespace ExcelToSQLTable
{
    class Excel
    {
        private string _excelFileName = string.Empty;

        public Excel(string filename)
        {
            ExcelPackage.License.SetNonCommercialPersonal("sisuser");
            _excelFileName = filename;
        }

        public (List<string> ,List<List<string>>) LoadExcelData()
        {
            FileInfo fileInfo = new FileInfo(_excelFileName);

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var headerRow = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    headerRow.Add(worksheet.Cells[1, col].Text);
                }

                var dataRows = new List<List<string>>();
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var rowData = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowData.Add(worksheet.Cells[row, col].Text);
                    }
                    dataRows.Add(rowData);
                }

                return (headerRow, dataRows);
            }
        }
    }
}
