
# DbcParser

[![Continuous Integration](https://github.com/EFeru/DbcParser/actions/workflows/ci.yml/badge.svg)](https://github.com/EFeru/DbcParser/actions/workflows/ci.yml)
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
using DbcParserLib.Model;
```
### Parsing
Then to parse a dbc file use the static class `Parser`, using one oth the parsing flavours:
```cs
Dbc dbc = Parser.ParseFromPath("C:\\your_dbc_file.dbc");
Dbc dbc = Parser.ParseFromStream(File.OpenRead("C:\\your_dbc_file.dbc")); // Or a stream from network
Dbc dbc = Parser.Parse("a dbc as string");
```

### Handling `Dbc` object
The ``Dbc`` object contains two collections, `Messages` and `Nodes`, both are `IEnumerable<T>` so can be accessed, iterated and queried using standard LINQ.

As an example, take all messages with id > 100 and more than 2 signals:
```cs
var filteredSelection = dbc
			.Messages
			.Where(m => m.ID > 100 && m.Signals.Count > 2)
			.ToArray();
```

### Parsing errors management
From **v1.4.0** parsing errors management has been introduced to inform users about syntax errors occurred during parsing procedure.
The `IParseFailureObserver` interface provides all methods to handle syntax errors, like:
- Generic syntax error (eg. `;`, `'`, `,` missing)
- Duplicated object definition (eg. messages with same ID; nodes, signals, custom properties with same name etc..)
- Object definition missing (eg. custom property is assigned before its declaration)
- Value consistency (eg. custom property value is out of bound with respect to min and max values defined in the property)

The library comes with two different implementations:
1. `SilentFailureObserver`: the default one. It silently swallow errors when parsing
2. `SimpleFailureObserver`: simple observer that logs any error. Errors list can be retrieve through `GetErrorList()` method, like in the example below

```cs
// Comment this two lines to remove errors parsing management (errors will be silent)
// You can provide your own IParseFailureObserver implementation to customize errors parsing management
var failureObserver = new SimpleFailureObserver();
Parser.SetParsingFailuresObserver(failureObserver);

var dbc = Parser.ParseFromPath(filePath);
var errors = failureObserver.GetErrorList();
```

The user is free to create its own implementation to customize error management.

## Packing/Unpacking signals

### Simple scenario
To pack and unpack signals you can use static class `Packer`
Example for packing/unpacking a signal: `14 bits`, Min: `-61.92`, Max: `101.91`
```cs
Signal sig = new Signal
{
  sig.Length = 14,
  sig.StartBit = 2,
  sig.IsSigned = 1,
  sig.ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
  sig.Factor = 0.01,
  sig.Offset = 20
};

// This packs the signal for sending
ulong TxMsg = Packer.TxSignalPack(-34.3, sig);

// This unpacks a received signal and calculates the corresponding engineering value
double val = Packer.RxSignalUnpack(TxMsg, sig);
```

Multiple signals can be packed before CAN transmission using:
```cs
ulong TxMsg = 0;
TxMsg |= Packer.TxSignalPack(value1, sig1);
TxMsg |= Packer.TxSignalPack(value2, sig2);
TxMsg |= Packer.TxSignalPack(value3, sig3);
// ...
// Send TxMsg on CAN
```
The user needs to make sure that the signals do not overlap with each other by properly specifying the `Length` and `StartBit`.

### Multiplexing
A message can contain multiplexed data, i.e. layout can change depending on a multiplexor value. The `Packer` class is unaware of multiplexing, so it's up to the user to check that the given message actually contains the signal.
As an example, consider the following dbc lines:
```
BO_ 568 UI_driverAssistRoadSign: 8 GTW
 SG_ UI_roadSign M : 0|8@1+ (1,0) [0|0] ""  DAS
 SG_ UI_dummyData m0 : 8|1@1+ (1,0) [0|0] "" Vector__XXX
 SG_ UI_stopSignStopLineDist m1 : 8|10@1+ (0.25,-8) [-8|247.5] "m" Vector__XXX
```
Signal `UI_dummyData` will only be available when `UI_roadSign` value is 0 while `UI_stopSignStopLineDist` will only be available when `UI_roadSign` value is 1. 
You can access multiplexing information calling
```cs
var multiplexingInfo = signal.MultiplexingInfo();
if(multiplexingInfo.Role == MultiplexingRole.Multiplexor)
{
	// This is a multiplexor!
}
else if(multiplexingInfo.Role == MultiplexingRole.Multiplexed)
{
	Console.WriteLine($"This signal is multiplexed and will be available when multiplexor value is {multiplexingInfo.Group}");
}
```
You can also check is a message does contain multiplexed signals by calling the extension method
```cs
if(message.IsMultiplexed())
{
	// ...
}
```

<br>

# Obsolete stuff

Below you can find a list of obsolete stuff that are going to be removed in the future releases.

| Class       | Property/Method     | Obsolete      | Removed               | Replaced by      | Comment       |
| --------    | -------             | -------       | -------               | -------          | -------       |
| Signal      | IsSigned            | v1.3.0        | planned in **1.4.3**  | `ValueType`      | Byte property replaced by `DbcValueType` property which provides <br> more informations about signal type |
| Signal      | ValueTable          | v1.3.0        | planned in **1.4.3**  | `ValueTableMap`  | String property replaced by a `IDictionary<int,string>` property |
| Signal      | ToPairs()           | v1.3.0        | **v1.4.2**            | -                | Extension method used to convert ValueTable into ValueTableMap |
| Message     | CycleTime           | v1.4.2        | planned in **1.4.4**  | `bool CycleTime(out int cycleTime)`    | CycleTime is no more a message property (replaced by an extension method) |

<br>

# Useful references
- [High level overview](https://docs.openvehicles.com/en/latest/components/vehicle_dbc/docs/dbc-primer.html)
- [Very well done overview, many resources on that site](https://github.com/stefanhoelzl/CANpy/blob/master/docs/DBC_Specification.md)
- [Good starting point for syntax](https://www.csselectronics.com/pages/can-dbc-file-database-intro)
- [Syntax specification. Not full but quite complete](http://mcu.so/Microcontroller/Automotive/dbc-file-format-documentation_compress.pdf)

# Contributions

Contributions are appreciated! Feel free to create pull requests to improve this library.

# Support

This project is supported by JetBrains for Open Source development

![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg)
