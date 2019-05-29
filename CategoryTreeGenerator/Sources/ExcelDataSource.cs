using System.Collections.Generic;
using System.IO;
using CategoryTreeGenerator.Models;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

namespace CategoryTreeGenerator.Sources
{
    public class ExcelDataSource : IDataSource
    {
        public IEnumerable<Tag> Tags { get; private set; }

        public IEnumerable<Models.Type> Types { get; private set; }

        public IEnumerable<Location> Locations { get; set; }

        public ExcelDataSource(IConfigurationRoot configuration)
        {
            Tags = new List<Tag>();
            Types = new List<Models.Type>();
            Locations = new List<Location>();

            var filePath = configuration.GetSection("DataSource:FilePath").Value;
            var file = new FileInfo(filePath);

            using (var excelBook = new ExcelPackage(file))
            {
                FillTags(excelBook.Workbook.Worksheets[1]);
                FillTypes(excelBook.Workbook.Worksheets[2]);
                FillLocations(excelBook.Workbook.Worksheets[3]);
            }
        }

        private void FillLocations(ExcelWorksheet sheet)
        {
            // todo realization
        }

        private void FillTypes(ExcelWorksheet sheet)
        {
            var rowsCount = sheet.Dimension.Rows;
            for (var i = 2; i <= rowsCount; i++)
            {
                var slug = sheet.Cells[i, 1].Value?.ToString();
                var title = sheet.Cells[i, 2].Value?.ToString();
                var alias = sheet.Cells[i, 3].Value?.ToString();
                var association = sheet.Cells[i, 4].Value?.ToString();

                ((List<Models.Type>)Types).Add(new Models.Type(slug, title, alias, association));
            }
        }

        private void FillTags(ExcelWorksheet sheet)
        {
            var rowsCount = sheet.Dimension.Rows;
            for (var i = 2; i <= rowsCount; i++)
            {
                var slug = sheet.Cells[i, 1].Value?.ToString();
                var title = sheet.Cells[i, 2].Value?.ToString();
                var alias = sheet.Cells[i, 3].Value?.ToString();

                ((List<Tag>)Tags).Add(new Tag(slug, title, alias));
            }
        }
    }
}