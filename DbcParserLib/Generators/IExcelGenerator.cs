using System;
using System.Collections.Generic;
using System.Text;

namespace DbcParserLib.Generators
{
    
    public interface IExcelGenerator
    {
        void WriteToFile(Dbc dbc, string path);
    }
}
