using DbcParserLib.Parsers;
using NUnit.Framework;

namespace DbcParserLib.Tests
{
    public class ExcelParserTests
    {
        [Test]
        public void SimpleExcelParserTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelParser excelParser = new ExcelParser();
            excelParser.ParseFromPath(path, out Dbc dbcOutput);
            Assert.That(dbcOutput, Is.Not.Null);
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
