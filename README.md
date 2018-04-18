# Serilog.Sinks.MSSqlServer [![Build status](https://ci.appveyor.com/api/projects/status/3btbux1hbgyugind/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-mssqlserver/branch/master) [![NuGet](https://img.shields.io/nuget/v/Serilog.Sinks.MSSqlServer.svg)](https://nuget.org/packages/Serilog.Sinks.MSSqlServer)

A Serilog sink that writes events to Microsoft SQL Server. While a NoSql store allows for more flexibility to store the different kinds of properties, it sometimes is easier to use an already existing MS SQL server. This sink will write the logevent data to a table and can optionally also store the properties inside an Xml column so they can be queried.

**Package** - [Serilog.Sinks.MSSqlServer](http://nuget.org/packages/serilog.sinks.mssqlserver)
| **Platforms** - .NET Framework 4.5 and .NET Standard 2.0

## Configuration

At minimum a connection string and table name are required. 

To use a connection string from the `connectionStrings` section of your application config, specify its name as the value of the connection string.

#### Code (.NET Framework)

```csharp
var connectionString = @"Server=...";  // or the name of a connection string in the app config
var tableName = "Logs";
var columnOptions = new ColumnOptions();  // optional

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString, tableName, columnOptions: columnOptions)
    .CreateLogger();
```

#### Code (.NET Standard / .NET Core)

The application configuration parameter is optional for .NET Standard libraries or .NET Core applications:

```csharp
var appSettings = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build(); // more likely you will inject an IConfiguration reference

var connectionString = @"Server=...";  // or the name of a connection string in the app config
var tableName = "Logs";
var columnOptions = new ColumnOptions();  // optional

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString, tableName, appConfiguration: appSettings, columnOptions: columnOptions)
    .CreateLogger();
```

#### AppSettings package (.NET Framework)

.NET Framework libraries or applications can call `ReadFrom.AppSettings()` to configure Serilog using the [Serilog.Settings.AppSettings](https://github.com/serilog/serilog-settings-appsettings) package. This will apply configuration parameters from the `app.config` or `web.config` file:

```xml
<add key="serilog:using:MSSqlServer" value="Serilog.Sinks.MSSqlServer" />
<add key="serilog:write-to:MSSqlServer.connectionString" value="Server=..."/>
<add key="serilog:write-to:MSSqlServer.tableName" value="Logs"/>
<add key="serilog:write-to:MSSqlServer.autoCreateSqlTable" value="true"/>
```

#### Configuration package (.NET Standard / .NET Core)

.NET Standard libraries and .NET Core applications can call `ReadFrom.Configuration(IConfiguration)` to configure Serilog using the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package. This will apply configuration parameters from the application configuration (not only `appsettings.json` as shown here, but any other valid `IConfiguration` source):

```json
{
  "Serilog": {
    "Using":  ["Serilog.Sinks.MSSqlServer"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "MSSqlServer", 
        "Args": { 
            "connectionString": "Server...",
            "tableName": "Logs"
        } 
      }
    ]
  }
}
```

## Table definition

You'll need to create a table like this in your database:

```
CREATE TABLE [Logs] (

   [Id] int IDENTITY(1,1) NOT NULL,
   [Message] nvarchar(max) NULL,
   [MessageTemplate] nvarchar(max) NULL,
   [Level] nvarchar(128) NULL,
   [TimeStamp] datetimeoffset(7) NOT NULL,  -- use datetime for SQL Server pre-2008
   [Exception] nvarchar(max) NULL,
   [Properties] xml NULL,
   [LogEvent] nvarchar(max) NULL

   CONSTRAINT [PK_Logs] 
     PRIMARY KEY CLUSTERED ([Id] ASC) 
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
	       ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
     ON [PRIMARY]

) ON [PRIMARY];
```

**Remember to grant the necessary permissions for the sink to be able to write to the log table.**

If you don't plan on using one or more columns, you can specify which columns to include in the *columnOptions.Store* parameter (see below). 

The Level column should be defined as a TinyInt if the *columnOptions.Level.StoreAsEnum* is set to true.


### Automatic table creation

If you set the `autoCreateSqlTable` option to `true`, the sink will create a table for you in the database specified in the connection string.  Make sure that the user associated with this connection string has enough rights to make schema changes.


## Standard columns

The "standard columns" used by this sink (apart from obvious required columns like Id) are described by the StandardColumn enumeration and controlled by `columnOptions.Store`.

By default (and consistent with the SQL command to create a table, above) these columns are included:
 - StandardColumn.Message
 - StandardColumn.MessageTemplate
 - StandardColumn.Level
 - StandardColumn.TimeStamp
 - StandardColumn.Exception
 - StandardColumn.Properties

You can change this list, as long as the table definition is consistent:

```csharp
// Don't include the Properties XML column.
columnOptions.Store.Remove(StandardColumn.Properties);

// Do include the log event data as JSON.
columnOptions.Store.Add(StandardColumn.LogEvent);
```

You can also store your own log event properties as additional columns; see below.


### Saving properties in additional columns

By default any log event properties you include in your log statements will be saved to the Properties column (and/or LogEvent column, per columnOption.Store).  But they can also be stored in their own columns via the AdditionalDataColumns setting.

```csharp
var columnOptions = new ColumnOptions
{
    AdditionalDataColumns = new Collection<DataColumn>
    {
        new DataColumn {DataType = typeof (string), ColumnName = "User"},
        new DataColumn {DataType = typeof (string), ColumnName = "Other"},
    }
};

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=.\SQLEXPRESS;Database=LogEvents;Trusted_Connection=True;", "Logs", columnOptions: columnOptions)
    .CreateLogger();
```

The log event properties `User` and `Other` will now be placed in the corresponding column upon logging. The property name must match a column name in your table. Be sure to include them in the table definition.


#### Excluding redundant items from the Properties column

By default, additional properties will still be included in the XML data saved to the Properties column (assuming that is not disabled via the `columnOptions.Store` parameter). This is consistent with the idea behind structured logging, and makes it easier to convert the log data to another (e.g. NoSQL) storage platform later if desired. 

However, if necessary, then the properties being saved in their own columns can be excluded from the XML.  Use the `columnOptions.Properties.ExcludeAdditionalProperties` parameter in the sink configuration to exclude the redundant properties from the XML. 


### Columns defined by configuration

Columns can be defined with the name and data type of the column in SQL Server. Columns specified must match database table exactly. DataType is case sensitive, based on SQL type (excluding precision/length). 

#### .NET Framework

This section will be processed automatically if it exists in the application's `web.config` or `app.config` file.

```xml
  <configSections>
    <section name="MSSqlServerSettingsSection"
             type="Serilog.Configuration.MSSqlServerConfigurationSection, Serilog.Sinks.MSSqlServer"/>
  </configSections>
  <MSSqlServerSettingsSection>
    <Columns>
      <add ColumnName="EventType" DataType="int"/>
      <add ColumnName="Release" DataType="varchar"/>
    </Columns>
  </MSSqlServerSettingsSection>      
```

#### .NET Standard / .NET Core

To add custom columns, add a list of column names and data types for the `customColumns` argument:

```json
{
  "Serilog": {
    "Using":  ["Serilog.Sinks.MSSqlServer"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "MSSqlServer", 
        "Args": { 
            "connectionString": "Server...",
            "tableName": "Logs"
            "customColumns": [
                {
                "ColumnName" : "EventType",
                "DataType" : "int"
                },
                {
                "ColumnName" : "Release",
                "DataType" : "varchar"
                }
            ]
        } 
      }
    ]
  }
}
```

### Options for serialization of the log event data

#### JSON (LogEvent column)

The log event JSON can be stored to the LogEvent column. This can be enabled by adding the LogEvent column to the `columnOptions.Store` collection. Use the `columnOptions.LogEvent.ExcludeAdditionalProperties` parameter to exclude redundant properties from the JSON. This is analogue to excluding redundant items from XML in the Properties column.

#### XML (Properties column)

To take advantage of SQL Server's XML support, the default storage of the log event properties is in the Properties XML column.

The serialization of the properties can be controlled by setting values in the in the `columnOptions.Properties` parameter.

Names of elements can be controlled by the `RootElementName`, `PropertyElementName`, `ItemElementName`, `DictionaryElementName`, `SequenceElementName`, `StructureElementName` and `UsePropertyKeyAsElementName` options.

The `UsePropertyKeyAsElementName` option, if set to `true`, will use the property key as the element name instead of "property" for the name with the key as an attribute.

If `OmitDictionaryContainerElement`, `OmitSequenceContainerElement` or `OmitStructureContainerElement` are set then the "dictionary", "sequence" or "structure" container elements will be omitted and only child elements are included.

If `OmitElementIfEmpty` is set then if a property is empty, it will not be serialized.

##### Querying the properties XML

Extracting and querying the properties data directly can be helpful when looking for specific log sequences.

Given the following XML property collection:

```xml
<properties>
  <property key="Action">GetUsers</property>
  <property key="Controller">UserController</property>
</properties>
```

The following query will extract the `Action` property and restrict the query based on the `Controller` property using SQL Servers built-in XQuery support.

```sql
SELECT 	[Message]
  , [TimeStamp]
  , [Exception]
  , [Properties].value('(//property[@key="Action"]/node())[1]', 'nvarchar(max)') as Action
FROM [Logs]
WHERE [Properties].value('(//property[@key="Controller"]/node())[1]', 'nvarchar(max)') = 'UserController'
```
