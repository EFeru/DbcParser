using DbcParserLib.Generators;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcParserLib.Tests
{
    public class ExcelGeneratorTests
    {
        [Test]
        public void SimpleWriteToExcelXlsTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";        
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator(path);
            excelGenerator.WriteToFile(dbc, outputPath);                
        }
        [Test]
        public void SimpleWriteToExcelXlsxTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xlsx";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator(path);
            excelGenerator.WriteToFile(dbc, outputPath);
        }
    }
}
