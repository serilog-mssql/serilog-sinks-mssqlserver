# Serilog.Sinks.MSSqlServer [![Build status](https://ci.appveyor.com/api/projects/status/3btbux1hbgyugind/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-mssqlserver/branch/master) [![NuGet](https://img.shields.io/nuget/v/Serilog.Sinks.MSSqlServer.svg)](https://nuget.org/packages/Serilog.Sinks.MSSqlServer)

A Serilog sink that writes events to Microsoft SQL Server. This sink will write the log event data to a table and can optionally also store the properties inside an XML or JSON column so they can be queried. Important properties can also be written to their own separate columns.

**Package** - [Serilog.Sinks.MSSqlServer](http://nuget.org/packages/serilog.sinks.mssqlserver)
| **Minimum Platforms** - .NET Framework 4.5.2, .NET Core 2.0, .NET Standard 2.0

#### Topics

* [Quick Start](#quick-start)
* [Sink Configuration](#sink-configuration)
* [Audit Sink Configuration](#audit-sink-configuration)
* [Table Definition](#table-definition)
* [MSSqlServerSinkOptions Object](#mssqlserversinkoptions-object)
* [ColumnOptions Object](#columnoptions-object)
* [SqlColumn Objects](#sqlcolumn-objects)
* [Standard Columns](#standard-columns)
* [Custom Property Columns](#custom-property-columns)
* [External Configuration Syntax](#external-configuration-syntax)
* [Troubleshooting](#troubleshooting)
* [Querying Property Data](#querying-property-data)
* [Deprecated Features](#deprecated-features)

## Quick Start

The most basic minimalistic sink initialization is done like this.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo
    .MSSqlServer(
        connectionString: "Server=localhost;Database=LogDb;Integrated Security=SSPI;",
        sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents" })
    .CreateLogger();
```

### Sample Programs

There is a set of small and simple sample programs provided with the source code in the `sample` directory. They demonstrate different ways to initialize the sink by code and configuration for different target frameworks.

## Sink Configuration

The sink can be configured completely through code, by using configuration files (or other types of configuration providers), a combination of both, or by using the various Serilog configuration packages. There are two configuration considerations: configuring the sink itself, and configuring the table used by the sink. The sink is configured with a typical Serilog `WriteTo` configuration method (or `AuditTo`, or similar variations). Settings for the sink are configured using a `MSSqlServerSinkOptions` object passed to the configuration method. The table is configured with an optional `ColumnOptions` object passed to the configuration method.

All sink configuration methods accept the following arguments, though not necessarily in this order. Use of named arguments is strongly recommended. Some platform targets have additional arguments.

* `connectionString`
* `sinkOptions`
* `columnOptions`
* `restrictedToMinimumLevel`
* `formatProvider`
* `logEventFormatter`

### Basic Arguments

At minimum, `connectionString` and `MSSqlServerSinkOptions.TableName` are required. If you are using an external configuration source such as an XML file or JSON file, you can use a named connection string instead of providing the full "raw" connection string.

All properties in the `MSSqlServerSinkOptions` object are discussed in the [MSSqlServerSinkOptions Object](#mssqlserversinkoptions-object) topic.

Table configuration with the optional `ColumnOptions` object is a lengthy subject discussed in the [ColumnOptions Object](#columnoptions-object) topic and other related topics.

Like other sinks, `restrictedToMinimumLevel` controls the `LogEventLevel` messages that are processed by this sink.

This is a "periodic batching sink." The sink will queue a certain number of log events before they're actually written to SQL Server as a bulk insert operation. There is also a timeout period so that the batch is always written even if it has not been filled. By default, the batch size is 50 rows and the timeout is 5 seconds. You can change these through by setting the `MSSqlServerSinkOptions.BatchPostingLimit` and `MSSqlServerSinkOptions.BatchPeriod` arguments.

Consider increasing the batch size in high-volume logging environments. In one test of a loop writing a single log entry, the default batch size averaged about 14,000 rows per second. Increasing the batch size to 1000 rows increased average write speed to nearly 43,000 rows per second. However, you should also consider the risk-factor. If the client or server crashes, or if the connection goes down, you may lose an entire batch of log entries. You can mitigate this by reducing the timeout. Run performance tests to find the optimal batch size for your production log table definition and log event content, network setup, and server configuration.

Refer to the Serilog Wiki's explanation of [Format Providers](https://github.com/serilog/serilog/wiki/Formatting-Output#format-providers) for details about the `formatProvider` arguments.

The parameter `logEventFormatter` can be used to specify a custom renderer implementing `ITextFormatter` which will be used to generate the contents of the `LogEvent`column. If the parameter is omitted or set to null, the default internal JSON formatter will be used. For more information about custom text formatters refer to the Serilog documentation [Custom text formatters](https://github.com/serilog/serilog/wiki/Formatting-Output#custom-text-formatters).

### Platform-Specific Arguments

These additional arguments are accepted when the sink is configured from a library or application that supports the .NET Standard-style _Microsoft.Extensions.Configuration_ packages. They are optional.

* `appConfiguration`
* `sinkOptionsSection`
* `columnOptionsSection`

The full configuration root provided to the `appConfiguration` argument is only required if you are using a named connection string. The sink needs access to the entire configuration object so that it can locate and read the `ConnectionStrings` section.

If you define the sink options or the log event table through external configuration, you must provide a reference to the `sinkOptionsSection` and/or `columnOptionsSection` via the argument by the same name.

### External Configuration and Framework Targets

Because of the way external configuration has been implemented in various .NET frameworks, you should be aware of how your target framework impacts which external configuration options are available. _System.Configuration_ refers to the use of XML-based `app.config` or `web.config` files, and _Microsoft.Extensions.Configuration_ (_M.E.C_) collectively refers to all of the extensions packages that were created as part of .NET Standard and the various compliant frameworks. _M.E.C_ is commonly referred to as "JSON configuration" although the packages support many other configuration sources including environment variables, command lines, Azure Key Vault, XML, and more.

| Your Framework | TFM | Project Types | External Configuration |
| --- | --- | --- |  --- |
| .NET Framework 4.5.2 | `net452` | app or library | _System.Configuration_ |
| .NET Framework 4.6.1+ | `net461` | app or library | _System.Configuration_ |
| .NET Framework 4.6.1+ | `net461` | app or library | _Microsoft.Extensions.Configuration_ |
| .NET Standard 2.0 | `netstandard2.0` | library only | _Microsoft.Extensions.Configuration_ |
| .NET Core 2.0+ | `netcoreapp2.0` | app or library | _System.Configuration_ |
| .NET Core 2.0+ | `netcoreapp2.0` | app or library | _Microsoft.Extensions.Configuration_ |

Support for .NET Framework 4.5.2 is tied to the Windows 8.1 lifecycle with support scheduled to end in January 2023.

Although it's possible to use both XML and _M.E.C_ configuration with certain frameworks, this is not supported, unintended consequences are possible, and a warning will be emitted to `SelfLog`. If you actually require multiple configuration sources, the _M.E.C_ builder-pattern is designed to support this, and your syntax will be consistent across configuration sources.

### Code-Only (any .NET target)

All sink features are configurable from code. Here is a typical example that works the same way for any .NET target. This example configures the sink itself as well as table features.

```csharp
var logDB = @"Server=...";
var sinkOpts = new MSSqlServerSinkOptions();
sinkOpts.TableName = "Logs";
var columnOpts = new ColumnOptions();
columnOpts.Store.Remove(StandardColumn.Properties);
columnOpts.Store.Add(StandardColumn.LogEvent);
columnOpts.LogEvent.DataLength = 2048;
columnOpts.PrimaryKey = options.TimeStamp;
columnOpts.TimeStamp.NonClusteredIndex = true;

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(
        connectionString: logDB,
        sinkOptions: sinkOpts,
        columnOptions: columnOpts
    ).CreateLogger();

```

### Code + _Microsoft.Extensions.Configuration_

Projects can build (or inject) a configuration object using _Microsoft.Extensions.Configuration_ and pass it to the sink's configuration method. If provided, the settings of `MSSqlServerSinkOptions` and `ColumnOptions` objects created in code are treated as a baseline which is then updated from the external configuration data. See the [External Configuration Syntax](#external-configuration-syntax) topic for details.

```csharp
var appSettings = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var logDB = @"Server=...";
var sinkOpts = new MSSqlServerSinkOptions { TableName = "Logs" };
var columnOpts = new ColumnOptions();

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(
        connectionString: logDB,
        sinkOptions: sinkOpts,
        columnOptions: columnOpts,
        appConfiguration: appSettings
    ).CreateLogger();
```

### Code + _System.Configuration_

Projects can load `MSSqlServerSinkOptions` and `ColumnOptions` objects from an XML configuration file such as `app.config` or `web.config`. The sink configuration method automatically checks `ConfigurationManager`, so there is no code to show, and no additional packages are required. See the [External Configuration Syntax](#external-configuration-syntax) topic for details. 

### External using _Serilog.Settings.Configuration_

_Requires configuration package version [**3.0.0**](https://www.nuget.org/packages/Serilog.Settings.Configuration/3.0.0) or newer._

.NET Standard projects can call `ReadFrom.Configuration()` to configure Serilog using the [_Serilog.Settings.Configuration_](https://github.com/serilog/serilog-settings-configuration) package. This will apply configuration arguments from all application configuration sources (not only `appsettings.json` as shown here, but any other valid `IConfiguration` source). This package can configure the sink itself with `MSSqlServerSinkOptions` as well as `ColumnOptions` table features. See the [External Configuration Syntax](#external-configuration-syntax) topic for details.

### External using _Serilog.Settings.AppSettings_

Projects can configure the sink from XML configuration by calling `ReadFrom.AppSettings()` using the [_Serilog.Settings.AppSettings_](https://github.com/serilog/serilog-settings-appsettings) package. This will apply configuration arguments from the project's `app.config` or `web.config` file. This is independent of configuring `MSSqlServerSinkOptions` or `ColumnOptions` from external XML files. See the [External Configuration Syntax](#external-configuration-syntax) topic for details.

## Audit Sink Configuration

A Serilog audit sink writes log events which are of such importance that they must succeed, and that verification of a successful write is more important than write performance. Unlike the regular sink, an audit sink _does not_ fail silently -- it can throw exceptions. You should wrap audit logging output in a `try/catch` block. The usual example is bank account withdrawal events -- a bank would certainly not want to allow a failure to record those transactions to fail silently.

The constructor accepts most of the same arguments, and like other Serilog audit sinks, you configure one by using `AuditTo` instead of `WriteTo`.

* `connectionString`
* `sinkOptions`
* `columnOptions`
* `formatProvider`
* `logEventFormatter`

The `restrictedToMinimumLevel` parameter is not available because all events written to an audit sink are required to succeed.

The `MSSqlServerSinkOptions.BatchPostingLimit` and `MSSqlServerSinkOptions.BatchPeriod` parameters are ignored because the audit sink writes log events immediately.

For _M.E.C_-compatible projects, `appConfiguration`, `sinkOptionsSection` and `columnOptionsSection` arguments are also provided, just as they are with the non-audit configuration extensions.

## Table Definition

If you don't use the auto-table-creation feature, you'll need to create a log event table in your database. In particular, give careful consideration to whether you need the `Id` column (options and performance impacts are discussed in the [Standard Columns](#standard-columns) topic). The table definition shown below reflects the default configuration using auto-table-creation without changing any sink options. Many other variations are possible. Refer to the [ColumnOptions Object](#columnoptions-object) topic to understand how the various configuration features relate to the table definition.

**IMPORTANT:** If you create your log event table ahead of time, the sink configuration must _exactly_ match that table, or errors are likely to occur.

```
CREATE TABLE [Logs] (

   [Id] int IDENTITY(1,1) NOT NULL,
   [Message] nvarchar(max) NULL,
   [MessageTemplate] nvarchar(max) NULL,
   [Level] nvarchar(128) NULL,
   [TimeStamp] datetime NOT NULL,
   [Exception] nvarchar(max) NULL,
   [Properties] nvarchar(max) NULL

   CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC) 
);
```

### Permissions

At a minimum, writing log entries requires SELECT and INSERT permissions for the log table. (SELECT is required because the sink's batching behavior uses bulk inserts which reads the schema before the write operations begin).

SQL permissions are a very complex subject. Here is an example of one possible solution (valid for SQL 2012 or later):

```
CREATE ROLE [SerilogAutoCreate];
GRANT SELECT ON sys.tables TO [SerilogAutoCreate];
GRANT SELECT ON sys.schemas TO [SerilogAutoCreate];
GRANT ALTER ON SCHEMA::[dbo] TO [SerilogAutoCreate]
GRANT CREATE TABLE ON DATABASE::[SerilogTest] TO [SerilogAutoCreate];

CREATE ROLE [SerilogWriter];
GRANT SELECT TO [SerilogWriter];
GRANT INSERT TO [SerilogWriter];

CREATE LOGIN [Serilog] WITH PASSWORD = 'password';

CREATE USER [Serilog] FOR LOGIN [Serilog] WITH DEFAULT_SCHEMA = dbo;
GRANT CONNECT TO [Serilog];

ALTER ROLE [SerilogAutoCreate] ADD MEMBER [Serilog];
ALTER ROLE [SerilogWriter] ADD MEMBER [Serilog];
```

This creates a SQL login named `Serilog`, a database user named `Serilog`, and assigned to that user are the roles `SerilogAutoCreate` and `SerilogWriter`. As the name suggests, the `SerilogAutoCreate` role is not needed if you create the database ahead of time, which is the recommended course of action if you're concerned about security at this level.

Ideally the `SerilogWriter` role would be restricted to the log table only, and that table has to already exist to use table-specific `GRANT` statements, so that's another reason that you probably don't want to use auto-create if you're concerned about log security. Table-level restrictions would look like this (assuming you name your log table `SecuredLog`, of course):

```
GRANT SELECT ON [dbo].[SecuredLog] TO [SerilogWriter];
GRANT INSERT ON [dbo].[SecuredLog] TO [SerilogWriter];
```

There are many possible variations. For example, you could also create a logging-specific schema and restrict access that way.

## MSSqlServerSinkOptions Object

Basic settings of the sink are configured using the properties in a `MSSqlServerSinkOptions` object:

* `TableName`
* `SchemaName`
* `AutoCreateSqlTable`
* `BatchPostingLimit`
* `BatchPeriod`
* `EagerlyEmitFirstEvent`
* `UseAzureManagedIdentity`
* `AzureServiceTokenProviderResource`

### TableName

A required parameter specifying the name of the table used to write the log events.

### SchemaName

An optional parameter specifiying the database schema where the log events table is located. It defaults to `"dbo"`.

### AutoCreateSqlTable

A flag specifiying if the log events table should be created if it does not exist. It defaults to `false`.

### BatchPostingLimit

Specifies a maximum number of log events that the non-audit sink writes per batch. The default is 50.  
This setting is not used by the audit sink as it writes each event immediately and not in a batched manner.

### BatchPeriod

Specifies the interval in which the non-audit sink writes a batch of log events to the database. It defaults to 5 seconds.  
This setting is not used by the audit sink as it writes each event immediately and not in a batched manner.

### EagerlyEmitFirstEvent

A Flag to eagerly write a batch to the database containing the first received event regardless of `BatchPostingLimit` or `BatchPeriod`. It defaults to `true`.  
This setting is not used by the audit sink as it writes each event immediately and not in a batched manner.

### UseAzureManagedIdentity

A flag specifiying to use Azure Managed Identities for authenticating with an Azure SQL server. It defaults to `false`. If enabled the property `AzureServiceTokenProviderResource` must be set as well.

**IMPORTANT:** Azure Managed Identities is only supported for the target frameworks .NET Framework 4.7.2+ and .NET Core 2.2+. Setting this to `true` when targeting a different framework results in an exception.

See [Azure AD-managed identities for Azure resources documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/) for details on how to configure and use Azure Managed Identitites.

### AzureServiceTokenProviderResource

Specifies the token provider resource to be used for aquiring an authentication token when using Azure Managed Identities for authenticating with an Azure SQL server. This setting is only used if `UseAzureManagedIdentity` is set to `true`.

## ColumnOptions Object

Features of the log table are defined by changing properties on a `ColumnOptions` object:

* `Store`
* `PrimaryKey`
* `ClusteredColumnstoreIndex`
* `DisableTriggers`
* `AdditionalColumns`

### Store

This is a list of columns that have special handling when a log event is being written. These are explained in the [Standard Columns](#standard-columns) topic. Only the Standard Columns which are in the log table should be present in the `Store` collection. This is a `List<>` of `StandardColumn` enumeration members, so you can simply `Add` or `Remove` columns to change the list. The order of appearance does not matter. The `ColumnOptions` object also has a property for each individual Standard Column providing access to column-specific settings. The properties match the Standard Column names (`Id`, `Message`, etc.) These are discussed in the documentation for each Standard Column.

### PrimaryKey

By default, the `Id` Standard Column is the table's primary key. You can set this property to any other column (either Standard Columns or custom columns you define; see the [Custom Property Columns](#custom-property-columns) topic). SQL Server requires primary key indexes to always be `NOT NULL` so the column-level `AllowNull` property will be overridden if set to `true`.

The primary key is optional; set this property to `null` to create a heap table with no primary key.

_NOTE:_ If you do not set the `NonClusteredIndex` property on the primary key column to `true`, the primary key constraint will be created as a clustered index. Clustered indexing is the default for backwards-compatibility reasons, but generally speaking this is not the best option for logging purposes (applications rarely emit fully unique properties, and using the unique auto-incrementing `Id` column as a primary key isn't particularly useful for query purposes).

### ClusteredColumnstoreIndex

Setting this to `true` changes the table to the clustered columnstore index (CCI) format. A complete discussion of CCI is beyond the scope of this documentation, but generally it uses high compression to dramatically improve search speeds. It is not compatible with a table primary key or a non-columnstore clustered index, and supporting `(max)` length character-data columns requires SQL 2017 or newer.

### DisableTriggers

Disabling triggers can significantly improve batch-write performance.

### AdditionalColumns

This is a `Collection<>` of `SqlColumn` objects that you create to define custom columns in the log event table. Refer to the [Custom Property Columns](#custom-property-columns) topic for more information.

## SqlColumn Objects

Each Standard Column in the `ColumnOptions.Store` list and any custom columns you add to the `AdditionalColumns` collection are `SqlColumn` objects with the following properties:

* `ColumnName`
* `PropertyName`
* `DataType`
* `AllowNull`
* `DataLength`
* `NonClusteredIndex`

### ColumnName

Any valid SQL column name can be used. Standard Columns have default names assigned but these can be changed without affecting their special handling.

### PropertyName

The optional name of a Serilog property to use as the value for a custom column. If not provided, the property used is the one that has the same name as the specified ColumnName. It applies only to custom columns defined in `AdditionalColumns` and is ignored for standard columns.

### DataType

This property can be set to nearly any value in the `System.Data.SqlDbType` enumeration. Unlike previous versions of this sink, SQL column types are fully supported end-to-end, including auto-table-creation. Earlier limitations imposed by the use of the .NET `DataColumn` object no longer apply. Most of the Standard Columns only support a limited subset of the SQL column types (and often just one type). Some of the special-case SQL column types are excluded such as `timestamp` and `udt`, and deprecated types like `text` and `image` are excluded. These are the supported SQL column data types:

* `bigint`
* `bit`
* `char`
* `date`
* `datetime`
* `datetime2`
* `datetimeoffset`
* `decimal`
* `float`
* `int`
* `money`
* `nchar`
* `nvarchar`
* `real`
* `smalldatetime`
* `smallint`
* `smallmoney`
* `time`
* `tinyint`
* `uniqueidentifier`
* `varchar`
* `xml`

Numeric types use the default precision and scale. For numeric types, you are responsible for ensuring the values you write do not exceed the min/max values of the underlying SQL column data types. For example, the SQL `decimal` type defaults to 18-digit precision (and scale 0) meaning the maximum value is 10<sup>18</sup>-1, or 999,999,999,999,999,999, whereas the .NET `decimal` type has a much higher maximum value of 79,228,162,514,264,337,593,543,950,335.

### AllowNull

Determines whether or not the column can store SQL `NULL` values. Some of the other features like `PrimaryKey` have related restrictions, and some of the Standard Columns impose restrictions (for example, the `Id` column never allows nulls).

### DataLength

For character-data and binary columns, this defines the column size (or maximum size if variable-length). The value -1 indicates `(max)` length and is the property's default. If the column data type doesn't support this, the setting is ignored. Note that clustered columnstore indexing is incompatible with `(max)` length columns prior to SQL 2017.

Supported SQL column data types that use this property:

* `char`
* `nchar`
* `nvarchar`
* `varchar`

### NonClusteredIndex

Any individual column can be defined as a non-clustered index, including the table primary key. Use this with caution, indexing carries a relatively high write-throughput penalty. One way to mitigate this is to keep non-clustered indexes offline and use batch reindexing on a scheduled basis.

## Standard Columns

By default (and consistent with the SQL DDL to create a table shown earlier) these columns are included in a new `ColumnOptions.Store` list:

 - `StandardColumn.Id`
 - `StandardColumn.Message`
 - `StandardColumn.MessageTemplate`
 - `StandardColumn.Level`
 - `StandardColumn.TimeStamp`
 - `StandardColumn.Exception`
 - `StandardColumn.Properties`

There is one additional Standard Column which is not included by default (for backwards-compatibility reasons):

- `StandardColumn.LogEvent`

You can change this list as long as the underlying table definition is consistent:

```csharp
// we don't need XML data
columnOptions.Store.Remove(StandardColumn.Properties);

// we do want JSON data
columnOptions.Store.Add(StandardColumn.LogEvent);
```

In addition to any special properties described below, each Standard Column also has the usual column properties like `ColumnName` as described in the topic [SqlColumn Objects](#sqlcolumn-objects).

### Id

The `Id` column is an optional table identity column. It defaults to the `int` data type but can also be configured as `bigint`. The `AllowNull` property is always `false`. If it is included in the table, it must be an auto-incrementing unique identity column and is automatically configured and auto-created as such.

Previous versions of this sink assumed the `Id` column was _always_ present as an `int` identity primary key with a clustered index. Other configurations are possible and probably preferable, however this is still the default for backwards-compatibility reasons. Carefully consider your anticipated logging volume and query requirements. The default setting is not ideal in real-world scenarios since a clustered index is primarily of use when the key is used for sorting or range searches. This is rarely the case for the `Id` column.

_No Id column:_ If you eliminate the column completely, the log table is stored as an unorded heap (as long as you don't define a different clustered primary key, which is not recommended). This is the ideal write-speed scenario for logging, however any non-clustered indexes you add will slightly degrade write performance.

_Non-clustered primary key:_ You can also retain the column as an identity primary key, but using a non-clustered index. The log is still stored as an unordered heap, but writing a non-clustered index is slightly faster. Non-clustered indexes on other columns will reference the Id primary key. However, read performance will be slightly degraded since it requires two reads (searching the non-clustered index, then dereferencing the heap row from the Id).

_BigInt data type:_ For very large log tables, if you absolutely require an identity column, you may wish to define the `Id` as the SQL `bigint` datatype. This 8-byte integer (equivalent to a c# `long` integer) will permit a maximum identity value of 9,223,372,036,854,775,807. This will slightly degrade both read and write performance.

### Message

This column stores the formatted output (property placeholders are replaced with property values). It defaults to `nvarchar(max)`. The `DataType` property can only be set to character-storage types.

### MessageTemplate

This column stores the log event message with the property placeholders. It defaults to `nvarchar(max)`. The `DataType` property can only be set to character-storage types.

### Level

This column stores the event level (Error, Information, etc.). For backwards-compatibility reasons it defaults to a length of 128 characters, but 12 characters is recommended. Alternately, the `StoreAsEnum` property can be set to `true` which causes the underlying level enum integer value to be stored as a SQL `tinyint` column. The `DataType` property can only be set to `nvarchar` or `tinyint`. Setting the `DataType` to `tinyint` is identical to setting `StoreAsEnum` to `true`.

### TimeStamp

This column stores the time the log event was sent to Serilog as a SQL `datetime` (default), `datetime2` or `datetimeoffset` type. If `datetime2` or `datetimeoffset` should be used, this can be configured as follows.

```csharp
var columnOptions = new ColumnOptions();
columnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
```

```csharp
var columnOptions = new ColumnOptions();
columnOptions.TimeStamp.DataType = SqlDbType.DateTime2;
```

Please be aware that you have to configure the sink for `datetimeoffset` if the used logging database table has a `TimeStamp` column of type `datetimeoffset`. If the underlying database uses `datetime2` for the `TimeStamp` column, the sink must be configured to use `datetime2`. On the other hand you must not configure for `datetimeoffset` if the `TimeStamp` column is of type `datetime` or `datetime2`. Failing to configure the data type accordingly can result in log table entries with wrong timezone offsets or no log entries being created at all due to exceptions during logging.

While TimeStamp may appear to be a good candidate as a clustered primary key, even relatively low-volume logging can emit identical timestamps forcing SQL Server to add a "uniqueifier" value behind the scenes (effectively an auto-incrementing identity-like integer). For frequent timestamp range-searching and sorting, a non-clustered index is better.

When the `ConvertToUtc` property is set to `true`, the time stamp is adjusted to the UTC standard. Normally the time stamp value reflects the local time of the machine issuing the log event, including the current timezone information. For example, if the event is written at 07:00 Eastern time, the Eastern timezone is +4:00 relative to UTC, so after UTC conversion the time stamp will be 11:00. Offset is stored as +0:00 but this is _not_ the GMT time zone because UTC does not use offsets (by definition). To state this another way, the timezone is discarded and unrecoverable. UTC is a representation of the date and time _exclusive_ of timezone information. This makes it easy to reference time stamps written from different or changing timezones.

### Exception

When an exception is logged as part of the log event, the exception message is stored here automatically. The `DataType` must be `nvarchar`.

### Properties

This column stores log event property values as XML. Typically you will use either this column or the JSON-based `LogEvent` column, but not both.

The `DataType` defaults to `nvarchar` and it is strongly recommended that this not be changed, but the SQL `xml` type is also supported. Using the `xml` type causes SQL server to convert the string data to a storage-efficent representation which can be searched much more quickly, but there is a measurable CPU-overhead cost. Test carefully with realistic workloads before committing to the `xml` data type.

The `ExcludeAdditionalProperties` setting is described in the [Custom Property Columns](#custom-property-columns) topic.

Names of elements can be controlled by the `RootElementName`, `PropertyElementName`, `ItemElementName`, `DictionaryElementName`, `SequenceElementName`, `StructureElementName` and `UsePropertyKeyAsElementName` options.

The `UsePropertyKeyAsElementName` option, if set to `true`, will use the property key as the element name instead of "property" for the name with the key as an attribute.

If `OmitDictionaryContainerElement`, `OmitSequenceContainerElement` or `OmitStructureContainerElement` are set then the "dictionary", "sequence" or "structure" container elements will be omitted and only child elements are included.

If `OmitElementIfEmpty` is set then if a property is empty, it will not be serialized.

### LogEvent

This column stores log event property values as JSON. Typically you will use either this column or the XML-based `Properties` column, but not both. This column's `DataType` must always be `nvarchar`.

The `ExcludeAddtionalProperties` and `ExcludeStandardColumns` properties are described in the [Custom Property Columns](#custom-property-columns) topic.

The content of this column is rendered as JSON by default or with a custom ITextFormatter passed by the caller as parameter `logEventFormatter`. Details can be found in [Sink Configuration](#sink-configuration).

## Custom Property Columns

By default, any log event properties you include in your log statements will be saved to the XML `Properties` column or the JSON `LogEvent` column. But they can also be stored in their own individual columns via the `AdditionalColumns` collection. This adds overhead to write operations but is very useful for frequently-queried properties. Only `ColumnName` is required; the default configuration is `varchar(max)`.

```csharp
var columnOptions = new ColumnOptions
{
    AdditionalColumns = new Collection<SqlColumn>
    {
        new SqlColumn
            {ColumnName = "EnvironmentUserName", PropertyName = "UserName", DataType = SqlDbType.NVarChar, DataLength = 64},

        new SqlColumn
            {ColumnName = "UserId", DataType = SqlDbType.BigInt, NonClusteredIndex = true},

        new SqlColumn
            {ColumnName = "RequestUri", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = false},
    }
};

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=...",
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs" },
        columnOptions: columnOptions)
    .CreateLogger();
```

In this example, when a log event contains any of the properties `UserName`, `UserId`, and `RequestUri`, the property values would be written to the corresponding columns. The property names must match exactly (case-insensitive).  In the case of `UserName`, that value would be written to the column named `EnvironmentUserName`.

Unlike previous versions of the sink, Standard Column names are not reserved. If you remove the `Id` Standard Column from the `ColumnOptions.Store` list, you are free to create a new custom column called `Id` which the sink will treat like any other custom column fully under your control.

Note the use of the `SqlDbType` enumerations for specifying `DataType`. Unlike previous versions of the sink, .NET `System` data types and `DataColumn` objects are no longer used for custom column definition. 

### Excluding redundant data

By default, properties matching a custom column will still be included in the data saved to the XML `Properties` or JSON `LogEvent` column. This is consistent with the idea behind structured logging, and makes it easier to convert the log data to another document-data storage platform later, if desired. 

However, the properties being saved in their own columns can be excluded from these catch-all columns.  Use the `columnOptions.Properties.ExcludeAdditionalProperties` parameter to exclude the redundant properties from the `Properties` XML column, or `columnOptions.LogEvent.ExcludeAdditionalProperties` if you're using the JSON `LogEvent` column. 

Standard Columns are always excluded from the XML `Properties` column  but Standard Columns are included in the JSON data for backwards-compatibility reasons. They can be excluded from the JSON `LogEvent` column with `columnOptions.LogEvent.ExcludeStandardColumns`.

## External Configuration Syntax

Projects targeting frameworks which are compatible with _System.Configuration_ automatically have support for XML-based configuration (either `app.config` or `web.config`) of a `MSSqlServerSinkOptions` parameters and a `ColumnOptions` table definition, and the _Serilog.Settings.AppSettings_ package adds XML-based configuration of other direct sink arguments (like `customFormatter` or `restrictedToMinimumLevel`).

Projects targeting frameworks which are compatible with _Microsoft.Extensions.Configuration_ can apply configuration-driven sink setup and `MSSqlServerSinkOptions` or `ColumnOptions` settings using the _Serilog.Settings.Configuration_ package or by supplying the appropriate arguments through code. 

All properties of the `MSSqlServerSinkOptions` class are configurable and almost all of the `ColumnOptions` class except the `Properties.PropertyFilter` predicate expression, and all elements and lists shown are optional. In most cases configuration key names match the class property names, but there are some exceptions. For example, because `PrimaryKey` is a `SqlColumn` object reference when configured through code, external configuration uses a `primaryKeyColumnName` setting to identify the primary key by name.

Custom columns and the stand-alone Standard Column entries all support the same general column properties (`ColumnName`, `DataType`, etc) listed in the [SqlColumn Objects](#sqlcolumn-objects) topic. The following sections documenting configuration syntax omit many of these properties for brevity.

If you combine external configuration with configuration through code, external configuration changes will be applied in addition to `MSSqlServerSinkOptions` and `ColumnOptions` objects you provide through code (external configuration "overwrites" properties defined in configuration, but properties only defined through code are preserved).

**IMPORTANT:** Some of the following examples do not reflect real-world configurations that can be copy-pasted as-is. Some settings or properties shown are mutually exclusive and are listed below for documentation purposes only.

### JSON (_Microsoft.Extensions.Configuration_)

Keys and values are not case-sensitive. This is an example of configuring the sink arguments.

```json
{
  "Serilog": {
    "Using":  ["Serilog.Sinks.MSSqlServer"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "MSSqlServer", 
        "Args": { 
            "connectionString": "NamedConnectionString",
            "sinkOptionsSection": {
                "tableName": "Logs",
                "schemaName": "EventLogging",
                "autoCreateSqlTable": true,
                "batchPostingLimit": 1000,
                "period": "0.00:00:30"
            },
            "restrictedToMinimumLevel": "Warning",
            "columnOptionsSection": { . . . }
        } 
      }
    ]
  }
}
```

As the name suggests, `columnOptionSection` is an entire configuration section in its own right. The `AdditionalColumns` collection can also be populated from a key named `customColumns` (not shown here) for backwards-compatibility reasons.

```json
"columnOptionsSection": {
    "disableTriggers": true,
    "clusteredColumnstoreIndex": false,
    "primaryKeyColumnName": "Id",
    "addStandardColumns": [ "LogEvent" ],
    "removeStandardColumns": [ "MessageTemplate", "Properties" ],
    "additionalColumns": [
        { "ColumnName": "EventType", "DataType": "int", "AllowNull": false },
        { "ColumnName": "Release", "DataType": "varchar", "DataLength": 32 },
        { "ColumnName": "EnvironmentUserName", "PropertyName": "UserName", "DataType": "varchar", "DataLength": 50 },
        { "ColumnName": "All_SqlColumn_Defaults",
            "DataType": "varchar",
            "AllowNull": true,
            "DataLength": -1,
            "NonClusteredIndex": false
        }
    ],
    "id": { "nonClusteredIndex": true },
    "level": { "columnName": "Severity", "storeAsEnum": false },
    "properties": { 
        "columnName": "Properties",
        "excludeAdditionalProperties": true, 
        "dictionaryElementName": "dict",
        "itemElementName": "item",
        "omitDictionaryContainerElement": false, 
        "omitSequenceContainerElement": false, 
        "omitStructureContainerElement": false, 
        "omitElementIfEmpty": true, 
        "propertyElementName": "prop",
        "rootElementName": "root",
        "sequenceElementName": "seq",
        "structureElementName": "struct",
        "usePropertyKeyAsElementName": false
    },
    "timeStamp": { "columnName": "Timestamp", "convertToUtc": true },
    "logEvent": {
        "excludeAdditionalProperties": true,
        "excludeStandardColumns": true
    },
    "message": { "columnName": "Msg" },
    "exception": { "columnName": "Ex" },
    "messageTemplate": { "columnName": "Template" }
}
```

### XML ColumnOptions (_System.Configuration_)

Keys and values are case-sensitive. Case must match **_exactly_** as shown below.

```xml
  <configSections>
    <section name="MSSqlServerSettingsSection"
             type="Serilog.Configuration.MSSqlServerConfigurationSection, Serilog.Sinks.MSSqlServer"/>
  </configSections>
  <MSSqlServerSettingsSection DisableTriggers="false"
                       ClusteredColumnstoreIndex="false"
                       PrimaryKeyColumnName="Id">

    <!-- SinkOptions parameters -->
    <TableName Value="Logs"/>
    <SchemaName Value="EventLogging"/>
    <AutoCreateSqlTable Value="true"/>
    <BatchPostingLimit Value="150"/>
    <BatchPeriod Value="00:00:15"/>

    <!-- ColumnOptions parameters -->
    <AddStandardColumns>
        <add Name="LogEvent"/>
    </AddStandardColumns>
    <RemoveStandardColumns>
        <remove Name="Properties"/>
    </RemoveStandardColumns>
    <Columns>
      <add ColumnName="EventType" DataType="int"/>
      <add ColumnName="EnvironmentUserName" 
           PropertyName="UserName" 
           DataType="varchar" 
           DataLength="50" />
      <add ColumnName="Release"
           DataType="varchar"
           DataLength="64"
           AllowNull="true"
           NonClusteredIndex="false"/>
    </Columns>
    <Exception ColumnName="Ex" DataLength="512"/>
    <Id NonClusteredIndex="true"/>
    <Level ColumnName="Severity" StoreAsEnum="true"/>
    <LogEvent ExcludeAdditionalProperties="true"
              ExcludeStandardColumns="true"/>
    <Message DataLength="1024"/>
    <MessageTemplate DataLength="1536"/>
    <Properties DataType="xml"
                ExcludeAdditionalProperties="true"
                DictionaryElementName="dict"
                ItemElementName="item"
                OmitDictionaryContainerElement="false"
                OmitSequenceContainerElement="false"
                OmitStructureContainerElement="false"
                OmitElementIfEmpty="true"
                PropertyElementName="prop"
                RootElementName="root"
                SequenceElementName="seq"
                StructureElementName="struct"
                UsePropertyKeyAsElementName="false"/>
    <TimeStamp ConvertToUtc="true"/>
  </MSSqlServerSettingsSection>      
```

### XML Sink (_Serilog.Settings.AppSettings_)

Refer to the _Serilog.Settings.AppSettings_ package documentation for complete details about sink configuration. This is an example of setting some of the configuration parameters for this sink.

```xml
<add key="serilog:using:MSSqlServer" value="Serilog.Sinks.MSSqlServer" />
<add key="serilog:write-to:MSSqlServer.connectionString" value="EventLogDB"/>
<add key="serilog:write-to:MSSqlServer.tableName" value="Logs"/>
<add key="serilog:write-to:MSSqlServer.autoCreateSqlTable" value="true"/>
```

## Troubleshooting

This is a relatively complex sink, and there are certain common problems which you should investigate before opening a new issue to ask for help. If you do open a new issue, please be sure to tell us all of the Serilog packages you are using and which versions, show us your _real_ configuration code and any external configuration sources, and a _simple_ example of code which reproduces the problem. If you're getting an error message, please include the exact message.

### Always check SelfLog first

After configuration is complete, this sink runs through a number of checks to ensure consistency. Some configuration issues result in an exception, but others may only generate warnings through Serilog's `SelfLog` feature. At runtime, exceptions are silently reported through `SelfLog`. Refer to [Debugging and Diagnostics](https://github.com/serilog/serilog/wiki/Debugging-and-Diagnostics#selflog) in the main Serilog documentation to enable `SelfLog` output.

### Always call Log.CloseAndFlush

Any Serilog application should _always_ call `Log.CloseAndFlush` before shutting down. This is especially important in sinks like this one. It is a "periodic batching sink" which means log event records are written in batches for performance reasons. Calling `Log.CloseAndFlush` should guarantee any batch in memory will be written to the database (but read the Visual Studio note below). You may wish to put the `Log.CloseAndFlush` call in a `finally` block in console-driven apps where a `Main` loop controls the overall startup and shutdown process. Refer to the _Serilog.AspNetCore_ sample code for an example. More exotic scenarios like dependency injection may warrant hooking the `ProcessExit` event when the logger is registered as a singleton:

```csharp
AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
```

### Consider batched sink SqlBulkCopy behavior

If you initialize the sink with `WriteTo` then it uses a batched sink semantics. This means that it does not directly issue an SQL command to the database for each log call, but it collectes log events in a buffer and later asynchronously writes a bulk of them to the database using [SqlBulkCopy](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?view=dotnet-plat-ext-3.1). **If SqlBulkCopy fails to write a single row of the batch to the database, the whole batch will be lost.** Unfortunately it is not easily possible (and probably only with a significant performance impact) to find out what lines of the batch caused problems. Therefore the sink cannot easily retry the operation with the problem lines removed. Typical problems can be that data (like the log message) exceeds the field length in the database or fields which cannot be null are null in the log event. Keep this in mind when using the batched version of the sink and avoid log events to be created with data that is invalid according to your database schema. Use a wrapper class or Serilog Enrichers to validate and correct the log event data before it gets written to the database.

### Test outside of Visual Studio

When you exit an application running in debug mode under Visual Studio, normal shutdown processes may be interrupted. Visual Studio issues a nearly-instant process kill command when it decides you're done debugging. This is a particularly common problem with ASP.NET and ASP.NET Core applications, in which Visual Studio instantly terminates the application as soon as the browser is closed. Even `finally` blocks usually fail to execute. If you aren't seeing your last few events written, try testing your application outside of Visual Studio.

### Try a `dev` package

If you're reading about a feature that doesn't seem to work, check whether you're reading the docs for the `master` branch or the `dev` branch -- most Serilog repositories are configured to use the `dev` branch by default. If you see something interesting only described by the `dev` branch documentation, you'll have to reference a `dev`-versioned package. The repository automatically generates a new `dev` package whenever code-related changes are merged.

### Are you really using this sink?

Please check your NuGet references and confirm you are specifically referencing _Serilog.Sinks.MSSqlServer_. In the early days of .NET Core, there was a popular Core-specific fork of this sink, but the documentation and NuGet project URLs pointed here. Today the package is marked deprecated, but we continue to see some confusion around this.

### .NET Framework apps must reference Microsoft.Data.SqlClient

If you are using the sink in a .NET Framework app, make sure to add a nuget package reference to Microsoft.Data.SqlClient in your app project. This is necessary due to a bug in SqlClient which can lead to exceptions about missing Microsoft assemblies. Details can be found in [issue 283](https://github.com/serilog/serilog-sinks-mssqlserver/issues/283#issuecomment-664397489) and [issue 208](https://github.com/serilog/serilog-sinks-mssqlserver/issues/208#issuecomment-664503566).

## Querying Property Data

Extracting and querying the property column directly can be helpful when looking for specific log sequences. SQL Server has query syntax supporting columns that store either XML or JSON data.

### LogEvent JSON

This capability requires SQL 2016 or newer. Given the following JSON properties:

```json
{
  "Properties": {
    "Action": "GetUsers",
    "Controller": "UserController"
  }
}
```

The following query will extract the `Action` property and restrict the query based on the `Controller` property using SQL Server's built-in JSON path support.

```sql
SELECT
  [Message], [TimeStamp], [Exception],
  JSON_VALUE(LogEvent, '$.Properties.Action') AS Action
FROM [Logs]
WHERE
  JSON_VALUE(LogEvent, '$.Properties.Controller') = 'UserController'
```

### Properties XML

Given the following XML properties:

```xml
<properties>
  <property key="Action">GetUsers</property>
  <property key="Controller">UserController</property>
</properties>
```

The following query will extract the `Action` property and restrict the query based on the `Controller` property using SQL Server's built-in XQuery support.

```sql
SELECT
  [Message], [TimeStamp], [Exception],
  [Properties].value('(//property[@key="Action"]/node())[1]', 'nvarchar(max)') AS Action
FROM [Logs]
WHERE
  [Properties].value('(//property[@key="Controller"]/node())[1]', 'nvarchar(max)') = 'UserController'
```

## Deprecated Features

Feature | Notes
:--- | :---
`AdditionalDataColumns` | Use the `AdditionalColumns` collection instead. Configuring the sink no longer relies upon .NET `DataColumn` objects or .NET `System` types.
`Id.BigInt` | Use `Id.DataType = SqlDb.BigInt` instead. (The `BigInt` property was only available in dev packages).
`Binary` and `VarBinary` | Due to the way Serilog represents property data internally, it isn't possible for the sink to access property data as a byte array, so the sink can't write to these column types. 

Most deprecated features are still available, but they are marked with the `[Obsolete]` attribute (which results in a compiler warning in your project) and will be removed in a future release. You should switch to the replacement implementations as soon as possible. Where possible, internally these are converted to the replacement implementation so that they only exist at the configuration level.
