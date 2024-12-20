using DbcParserLib.Generators;
using DbcParserLib.Model;
using DbcParserLib.Parsers;
using NUnit.Framework;
using System.Collections.Generic;
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
            excelParser.ParseFirstSheetFromPath(path, out Dbc dbcOutput);
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
        [Test]
        public void SpecificSimplParserWithColumnNameTest() 
        {
            // Path setting
            string fileName = "SpecificSimpleTest";
            string extension = ".xlsx";
            string outputFileName = fileName;
            string path = $@"..\..\..\..\DbcFiles\{fileName}{extension}";

            //Modified Specific Column Header and Column Index
            ExcelParser excelParser = new ExcelParser();
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.MessageName, "A");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.FrameFormat, "B");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.ID, "C");           
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.MessageSendType, "D");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.CycleTime, "E");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.DataLength, "F");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.SignalName, "g");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.Description, "h");        
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.ByteOrder, "I");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.StartBit, "J");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.BitLength, "L");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.DataType, "M");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.Factor, "N");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.Offset, "O");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.MinimumPhysical, "P");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.MaximumPhysical, "q");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.InitialValue, "T");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.Unit, "W");
            excelParser.UpdateColumnConfigWithName(DictionaryColumnKey.ValueTable, "x");
            excelParser.SetNodeRowIndex(0);
            excelParser.SetNodeStartIndex("Y");
            Assert.That(excelParser.CheckColumnIndexConfiction(out List<int> conflctList), Is.False);
            // Parsing and generation
            excelParser.ParseFirstSheetFromPath(path, out Dbc dbcOutput);
            var outputFilePath = $@"..\..\..\..\DbcFiles\{outputFileName}.dbc";
            DbcGenerator.WriteToFile(dbcOutput, outputFilePath);
            Assert.That(dbcOutput, Is.Not.Null);
            Assert.That(File.Exists(outputFilePath), Is.True);
        }
    }
}
