using DbcParserLib.Generators;
using NUnit.Framework;
using System.IO;

namespace DbcParserLib.Tests
{
    public class DbcGeneratorTests
    {
        private const string MainDbcFilePath = @"..\..\..\..\DbcFiles\tesla_can.dbc";
        [Test]
        public void NormalMessageWriteToFileTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);
            var outputFilePath = @"..\..\..\..\DbcFiles\tesla_can_output.dbc";
            DbcGenerator.WriteToFile(dbc, outputFilePath);
            Assert.That(File.Exists(outputFilePath), Is.True);
        }

        private const string J1939DbcFilePath = @"..\..\..\..\DbcFiles\j1939.dbc";
        [Test]
        public void J1939MessageWriteToFileTest()
        {
            var dbc = Parser.ParseFromPath(J1939DbcFilePath);
            var outputFilePath = @"..\..\..\..\DbcFiles\j1939_output.dbc";
            DbcGenerator.WriteToFile(dbc, outputFilePath);
            Assert.That(File.Exists(outputFilePath), Is.True);
        }
    }
}
