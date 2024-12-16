using DbcParserLib.Generators;
using NUnit.Framework;
using System.Collections.Generic;
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

        private const string J1939DbcFilePath = @"..\..\..\..\DbcFiles\J1939-2023-10_v1.1.dbc";
        [Test]
        public void J1939MessageWriteToFileTest()
        {
            var dbc = Parser.ParseFromPath(J1939DbcFilePath);
            var outputFilePath = @"..\..\..\..\DbcFiles\j1939_output.dbc";
            DbcGenerator.WriteToFile(dbc, outputFilePath);
            Assert.That(File.Exists(outputFilePath), Is.True);
        }
        [Test]
        public void MergeDbcFileTest()
        {
            string path1 = @"..\..\..\..\DbcFiles\CANopen.dbc";
            string path2 = @"..\..\..\..\DbcFiles\ASWT_CM_S_Inst2.dbc";
            var mergedPath = @"..\..\..\..\DbcFiles\merged_output.dbc";
            Dbc dbc1 = Parser.ParseFromPath(path1);
            Dbc dbc2 = Parser.ParseFromPath(path2);
            List<Dbc> dbcs = new List<Dbc>();
            dbcs.Add(dbc1);
            dbcs.Add(dbc2);
            DbcGenerator.MergeDbc(dbcs, out Dbc dbcOutput);
            DbcGenerator.WriteToFile(dbcOutput, mergedPath);
            Assert.That(File.Exists(mergedPath), Is.True);
        }
    }
}
