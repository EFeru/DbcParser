using DbcParserLib.Generators;
using DbcParserLib.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

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
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void SimpleWriteToExcelXlsxTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xlsx";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void WriteToExcelXlsPathTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFile\tesla_can.xls";//ErrorPath
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath),Is.True );
        }
        [Test]
        public void ColumnIndexConfictionCheckTest()
        {
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.UpdateColumnConfig(DictionaryColumnKey.MessageName, 1);
            Assert.That(excelGenerator.CheckColumnIndexConfiction(out List<int> confictionIndexList), Is.True);
        }
    }
}
