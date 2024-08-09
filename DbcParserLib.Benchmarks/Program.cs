using BenchmarkDotNet.Running;
using DbcParserLib.Tests;

var summary = BenchmarkRunner.Run<PackerBenchmark>();


/* Summary 

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2


| Method                         | Mean     | Error    | StdDev   |
|------------------------------- |---------:| --------:| --------:|
| Pack_8Byte_BigEndian_Uint64    | 40.24 ns | 0.555 ns | 0.519 ns |
| Pack_8Byte_BigEndian_ByteArray | 39.68 ns | 0.786 ns | 0.807 ns |

*/