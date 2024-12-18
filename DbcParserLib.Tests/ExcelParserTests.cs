using DbcParserLib.Generators;
using DbcParserLib.Parsers;
using NUnit.Framework;
using System.IO;

namespace DbcParserLib.Tests
{
    public class ExcelParserTests
    {
        [Test]
        public void SimpleExcelParserTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.xls";
            ExcelParser excelParser = new ExcelParser();
            excelParser.ParseFromPath(path, out Dbc dbcOutput);
            var outputFilePath = @"..\..\..\..\DbcFiles\excelConverted_tesla_can.dbc";
            DbcGenerator.WriteToFile(dbcOutput, outputFilePath);
            Assert.That(dbcOutput, Is.Not.Null);
            Assert.That(File.Exists(outputFilePath), Is.True);
        }
        [Test]
        public void ExcelColumnNameParserTest()
        {
            ExcelParser excelParser = new ExcelParser();
            excelParser.SetNodeStartIndex("A");
            Assert.That(excelParser.GetNodeStartIndex(), Is.EqualTo(0));
            excelParser.SetNodeStartIndex("B");
            Assert.That(excelParser.GetNodeStartIndex(), Is.EqualTo(1));
            excelParser.SetNodeStartIndex("AA");
            Assert.That(excelParser.GetNodeStartIndex(), Is.EqualTo(26));

        }
    }
}
