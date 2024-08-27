using BenchmarkDotNet.Running;
using DbcParserLib.Tests;

var summaryPack = BenchmarkRunner.Run<PackerBenchmark>();

/* Summary => This is pc dependend. The relevant information is the time difference

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2


| Method                                            | Mean        | Error     | StdDev    |
|-------------------------------------------------- |------------:| ----------:| ----------:|
| Pack_8Byte_BigEndian_Uint64                       | 40.5778 ns | 0.3100 ns | 0.2420 ns |
| Pack_8Byte_BigEndian_ByteArray                    | 126.9214 ns | 2.5612 ns | 2.3958 ns |
| Pack_8Byte_LittleEndian_Uint64                    | 12.1883 ns | 0.1173 ns | 0.0980 ns |
| Pack_8Byte_LittleEndian_ByteArray                 | 100.1948 ns | 0.6864 ns | 0.5359 ns |
| Pack_1Signal_Unsigned_NoScale_StatePack           | 0.8825 ns | 0.0081 ns | 0.0068 ns |
| Pack_1Signal_Unsigned_NoScale_SignalPack          | 3.0999 ns | 0.0711 ns | 0.0665 ns |
| Pack_1Signal_Unsigned_NoScale_SignalPackByteArray | 49.3012 ns | 0.8965 ns | 0.8386 ns |
*/

var summaryUnpack = BenchmarkRunner.Run<UnpackBenchmark>();

/* Summary => This is pc dependend. The relevant information is the time difference

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.32 (6.0.3224.31407), X64 RyuJIT AVX2


| Method                                          | Mean       | Error     | StdDev    |
|------------------------------------------------ |-----------:| ----------:| ----------:|
| Unpack_8Byte_BigEndian_Uint64                   | 107.287 ns | 2.1000 ns | 2.0625 ns |
| Unpack_8Byte_BigEndian_ByteArray                | 201.915 ns | 4.0086 ns | 4.2892 ns |
| Unpack_8Byte_LittleEndian_Uint64                | 81.062 ns | 0.2072 ns | 0.1617 ns |
| Unpack_8Byte_LittleEndian_ByteArray             | 171.328 ns | 3.2070 ns | 3.1497 ns |
| Unpack_1Signal_Unsigned_NoScale_State           | 1.077 ns | 0.0037 ns | 0.0029 ns |
| Unpack_1Signal_Unsigned_NoScale_Signal          | 20.269 ns | 0.1389 ns | 0.1232 ns |
| Unpack_1Signal_Unsigned_NoScale_SignalByteArray | 62.919 ns | 0.1156 ns | 0.0902 ns |
*/