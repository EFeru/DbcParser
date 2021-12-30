
# DbcParser

[![](https://img.shields.io/nuget/dt/dbcparserlib?color=004880&label=downloads&logo=NuGet)](https://www.nuget.org/packages/DbcParserLib/)
[![](https://img.shields.io/nuget/vpre/dbcparserlib?color=%23004880&label=NuGet&logo=NuGet)](https://www.nuget.org/packages/DbcParserLib/)
[![GitHub](https://img.shields.io/github/license/eferu/dbcparser?color=%231281c0)](LICENSE)

Probably **the first .NET DBC file parser**. Includes packing and unpacking functionality for sending and receiving CAN signals.

Below is a quick preview of the extracted data using a [Tesla dbc file](https://github.com/commaai/opendbc/blob/master/tesla_can.dbc) taken from [commaai/opendbc](https://github.com/commaai/opendbc) project:

![Preview](https://raw.githubusercontent.com/EFeru/DbcParser/main/Docs/pics/dbcparser_preview.png)


## Quickstart

Install the library via [Nuget Packages](https://www.nuget.org/packages/DbcParserLib/) and add at the top of your file:
```cs
using DbcParserLib;
```

Then to parse a dbc file use:
```cs
DbcParser dbc = new DbcParser();
dbc.ReadFromFile("C:\\your_dbc_file.dbc");
```

Access the nodes, messages and signals using:
```cs
dbc.Nodes[i]
dbc.Messages[j]
dbc.Messages[j].Signals[k]
```

### Packing/Unpacking signals

Example for packing/unpacking a signal: `14 bits`, Min: `-61.92`, Max: `101.91`
```cs
Signal sig = new Signal();
sig.Length = 14;
sig.StartBit = 2;
sig.IsSigned = 1;
sig.ByteOrder = 1; // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
sig.Factor = 0.01;
sig.Offset = 20;
ulong TxMsg = dbc.TxSignalPack(-34.3, sig);
double val = dbc.RxSignalUnpack(TxMsg, sig);
```

Multiple signals can be packed before CAN transmission using:
```cs
ulong TxMsg = 0;
TxMsg |= dbc.TxSignalPack(value1, sig1);
TxMsg |= dbc.TxSignalPack(value2, sig2);
TxMsg |= dbc.TxSignalPack(value3, sig3);
// ...
// Send TxMsg on CAN
```
The user needs to make sure that the signals do not overlap with each other by properly specifying the `Length` and `StartBit`.

## Contributions

Contributions are appreciated! Feel free to create pull requests to improve this library.
