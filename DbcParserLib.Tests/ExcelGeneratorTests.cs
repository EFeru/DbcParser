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
        public void WriteToExcelXlsTest()
        {
            //string path = @"..\..\..\..\DbcFiles\J1939-2023-10_v1.1.dbc";
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";        
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator(path);
            excelGenerator.WriteToFile(dbc, outputPath);                
        }
        [Test]
        public void WriteToExcelXlsxTest()
        {

        }
    }
}
