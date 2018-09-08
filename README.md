# Serilog.Sinks.MSSqlServer [![Build status](https://ci.appveyor.com/api/projects/status/3btbux1hbgyugind/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-mssqlserver/branch/master) [![NuGet](https://img.shields.io/nuget/v/Serilog.Sinks.MSSqlServer.svg)](https://nuget.org/packages/Serilog.Sinks.MSSqlServer)

A Serilog sink that writes events to Microsoft SQL Server. While a NoSql store allows for more flexibility to store the different kinds of properties, it sometimes is easier to use an already existing MS SQL server. This sink will write the logevent data to a table and can optionally also store the properties inside an Xml column so they can be queried.

**Package** - [Serilog.Sinks.MSSqlServer](http://nuget.org/packages/serilog.sinks.mssqlserver)
| **Platforms** - .NET Framework 4.5 and .NET Standard 2.0

## Configuration

At minimum a connection string and table name are required. 

To use a connection string from the `connectionStrings` section of your application config, specify its name as the value of the connection string.


#### Code (.NET Framework)

Older .NET Framework applications can use the `ConfigurationManager` API shown below. Newer .NET Framework applications (using a Framework version that is .NET Standard compliant) should use the _Microsoft.Extensions.Configuration_ version in the next section.

```csharp
var connectionString = @"Server=...";  // or the name of a connection string in the app config
var tableName = "Logs";
var columnOptions = new ColumnOptions();  // optional

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString, tableName, columnOptions: columnOptions)
    .CreateLogger();
```


#### Code (.NET Standard / .NET Core)

The application configuration parameter is optional for .NET Standard libraries or .NET Core applications.

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


#### Serilog AppSettings package (.NET Framework)

.NET Framework libraries or applications can call `ReadFrom.AppSettings()` to configure Serilog using the [_Serilog.Settings.AppSettings_](https://github.com/serilog/serilog-settings-appsettings) package. This will apply configuration parameters from the `app.config` or `web.config` file:

```xml
<add key="serilog:using:MSSqlServer" value="Serilog.Sinks.MSSqlServer" />
<add key="serilog:write-to:MSSqlServer.connectionString" value="Server=..."/>
<add key="serilog:write-to:MSSqlServer.tableName" value="Logs"/>
<add key="serilog:write-to:MSSqlServer.autoCreateSqlTable" value="true"/>
```


#### Serilog Configuration package (.NET Standard / .NET Core)

.NET Standard libraries and .NET Core applications can call `ReadFrom.Configuration(IConfiguration)` to configure Serilog using the [_Serilog.Settings.Configuration_](https://github.com/serilog/serilog-settings-configuration) package (version [**3.0.0-dev-00111**](https://www.nuget.org/packages/Serilog.Settings.Configuration/3.0.0-dev-00111) or newer). This will apply configuration parameters from the application configuration (not only `appsettings.json` as shown here, but any other valid `IConfiguration` source):


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

You'll need to create a table like this in your database. Many other variations are possible. In particular, give careful consideration to whether you need the Id column (discussed in the next section). The table definition shown here is the default configuration.

```
CREATE TABLE [Logs] (

   [Id] int IDENTITY(1,1) NOT NULL,
   [Message] nvarchar(max) NULL,
   [MessageTemplate] nvarchar(max) NULL,
   [Level] nvarchar(128) NULL,
   [TimeStamp] datetimeoffset(7) NOT NULL,  -- use datetime for SQL Server pre-2008
   [Exception] nvarchar(max) NULL,
   [Properties] xml NULL

   CONSTRAINT [PK_Logs] 
     PRIMARY KEY CLUSTERED ([Id] ASC) 
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
	       ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
     ON [PRIMARY]

) ON [PRIMARY];
```

If you don't plan to use a column, you can specify which columns to exclude in the `columnOptions.Store` parameter (see below). 

The Level column should be defined as a `tinyint` when `columnOptions.Level.StoreAsEnum` is set to `true`.


### Automatic table creation

If you set the `autoCreateSqlTable` option to `true`, the sink will create a table for you in the database specified in the connection string.  Make sure that the user associated with this connection string has enough rights to make schema changes; see below.


### Permissions

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

This creates a SQL login named `Serilog`, a database user named `Serilog`, and assigned to that user are the roles `SerilogAutoCreate` and `SerilogWriter`. As the name suggests, the SerilogAutoCreate role is not needed if you create the database ahead of time, which is the recommended course of action if you're concerned about security at this level.

Also, ideally the SerilogWriter role would be restricted to the log table only, and that table has to already exist for table-specific GRANT statements to execute, so that's another reason that you probably don't want to use auto-create. Table-level restrictions would look like this (assuming you name your log table SecuredLog, of course):

```
GRANT SELECT ON [dbo].[SecuredLog] TO [SerilogWriter];
GRANT SELECT ON [dbo].[SecuredLog] TO [SerilogWriter];
```

There are many possible variations. For example, you could also create a new schema that was specific to the log(s) and restrict access that way.


## Id Column Options

Previous versions of this sink assumed the Id column is always present as an `int` `IDENTITY` primary key with a clustered index. Other configurations are available, however this is still the default strictly for backwards-compatibility reasons.

You should consider your anticipated logging volume and query requirements carefully. The default setting is not especially useful in real-world query scenarios since a clustered index is primarily of use when the key is used for sorting or range searches, which will rarely be the case for the Id column.

### No Id Column

If you eliminate the Id column completely, the log table is stored as an unindexed heap. This is the ideal write-speed scenario for logging, however any non-clustered indexes you add will degrade write performance. One way to mitigate this is to keep the non-clustered indexes offline and use batch reindexing on a scheduled basis. If you create your table ahead of time, simply omit the Id column and the constraint shown in the previous section.

### Unclustered Id Column

You can also retain the Id column as an `IDENTITY` primary key, but without a clustered index. The log is still stored as an unindexed heap, but writes with non-clustered indexes are slightly faster. The non-clustered indexes will reference the Id primary key. However, read performance will be slightly degraded since it requires two reads (the covering non-clustered index, then dereferencing the heap row from the Id). To create this type of table ahead of time, change the constraint in the previous section to `NONCLUSTERED` and leave out the `WITH` clause.

### Bigint Data Type

For very large log tables, you may wish to create the Id column with the `bigint` datatype. This 8-byte integer will permit a maximum identity value of 9,223,372,036,854,775,807. The only change to the table syntax in the previous section is the datatype where `[Id]` is defined. 


## Standard columns

The "standard columns" used by this sink are described by the `StandardColumn` enumeration and controlled through code by the `columnOptions.Store` collection. By default (and consistent with the SQL command to create a table, above) these columns are included:

 - `StandardColumn.Id`
 - `StandardColumn.Message`
 - `StandardColumn.MessageTemplate`
 - `StandardColumn.Level`
 - `StandardColumn.TimeStamp`
 - `StandardColumn.Exception`
 - `StandardColumn.Properties`

You can change this list, as long as the table definition is consistent:

```csharp
// Don't include the Properties XML column.
columnOptions.Store.Remove(StandardColumn.Properties);

// Do include the log event data as JSON.
columnOptions.Store.Add(StandardColumn.LogEvent);
```

You can also store your own log event properties in additional custom columns; see below.

### Saving properties in custom columns

By default any log event properties you include in your log statements will be saved to the Properties column (and/or LogEvent column, per columnOption.Store).  But they can also be stored in their own columns via the AdditionalDataColumns setting.

```csharp
var columnOptions = new ColumnOptions
{
    AdditionalDataColumns = new Collection<DataColumn>
    {
        new DataColumn {DataType = "nvarchar", ColumnName = "UserName", DataLength = 64},
        new DataColumn {DataType = "varchar", ColumnName = "RequestUri", DataLength = -1, AllowNull = false},
    }
};

var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=...", "Logs", columnOptions: columnOptions)
    .CreateLogger();
```

The log event properties `UserName` and `RequestUri` will be written to the corresponding columns whenever those values (with the exact same property name) occur in a log entry. Be sure to include them in the table definition if you create your table ahead of time.

Variable-length data types like `varchar` require a `DataLength` property. Use -1 to specify SQL's `MAX` length.

**Standard column names are reserved. Even if you exclude a standard column, never create a custom column by the same name.**


#### Excluding redundant Properties or LogEvent data

By default, additional properties will still be included in the data saved to the XML Properties or JSON LogEvent column (assuming one or both are enabled via the `columnOptions.Store` parameter). This is consistent with the idea behind structured logging, and makes it easier to convert the log data to another (e.g. NoSQL) storage platform later if desired. 

However, if necessary, the properties being saved in their own columns can be excluded from the data.  Use the `columnOptions.Properties.ExcludeAdditionalProperties` parameter in the sink configuration to exclude the redundant properties from the XML, or `columnOptions.LogEvent.ExcludeAdditionalProperties` if you've added the JSON LogEvent column. 

The standard columns are always excluded from the Properties and LogEvent columns.

### Columns defined by AppSettings (.NET Framework)

Custom columns can be defined with the name and data type of the column in SQL Server. Columns specified must match database table exactly. DataType is case sensitive, based on SQL type (excluding precision/length). This section will be processed automatically if it exists in the application's `web.config` or `app.config` file.

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

### ColumnOptions defined by Configuration (.NET Standard / .NET Core)

For projects using the Serilog Configuration package, most properties of the `ColumnOptions` object are configurable. (The only property not currently supported is the filter-predicate `columnOptions.Properties.PropertyFilter`).

The equivalent of adding custom columns as shown in the .NET Framework example above looks like this:

```json
{
  "Serilog": {
    "Using":  ["Serilog.Sinks.MSSqlServer"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "MSSqlServer", 
        "Args": { 
            "connectionString": "Server...",
            "tableName": "Logs",
            "columnOptionsSection": {
              "customColumns": [
                { "ColumnName": "EventType", "DataType": "int", "AllowNull": false },
                { "ColumnName": "Release", "DataType": "varchar", "DataLength": 32 }
              ]
            }
        } 
      }
    ]
  }
}
```

As the name suggests, `columnOptionSection` is an entire configuration section in its own right. All possible entries and some sample values are shown below. All properties and subsections are optional. It is not currently possible to specify a `PropertiesFilter` predicate in configuration.

```json
"columnOptionsSection": {
    "addStandardColumns": [ "LogEvent" ],
    "removeStandardColumns": [ "MessageTemplate", "Properties" ],
    "customColumns": [
        { "ColumnName": "EventType", "DataType": "int", "AllowNull": false },
        { "ColumnName": "Release", "DataType": "varchar", "DataLength": 32 }
    ],
    "disableTriggers": true,
    "id": { "columnName": "Id", "bigint": true, "clusteredIndex": true },
    "level": { "columnName": "Level", "storeAsEnum": false },
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
    "logEvent": { "columnName": "LogEvent", "excludeAdditionalProperties": true },
    "message": { "columnName": "Message" },
    "exception": { "columnName": "Exception" },
    "messageTemplate": { "columnName": "MessageTemplate" }
}
```


### Options for serialization of event data

Typically you will choose either XML or JSON serialization, but not both.

#### JSON (LogEvent column)

Event data items can be stored to the LogEvent column. This can be enabled by adding the LogEvent column to the `columnOptions.Store` collection. Use the `columnOptions.LogEvent.ExcludeAdditionalProperties` parameter to exclude redundant properties from the JSON. This is analogue to excluding redundant items from XML in the Properties column.

#### XML (Properties column)

To take advantage of SQL Server's XML support, the default storage of the log event properties is in the Properties XML column.

The serialization of the properties can be controlled by setting values in the in the `columnOptions.Properties` parameter.

Names of elements can be controlled by the `RootElementName`, `PropertyElementName`, `ItemElementName`, `DictionaryElementName`, `SequenceElementName`, `StructureElementName` and `UsePropertyKeyAsElementName` options.

The `UsePropertyKeyAsElementName` option, if set to `true`, will use the property key as the element name instead of "property" for the name with the key as an attribute.

If `OmitDictionaryContainerElement`, `OmitSequenceContainerElement` or `OmitStructureContainerElement` are set then the "dictionary", "sequence" or "structure" container elements will be omitted and only child elements are included.

If `OmitElementIfEmpty` is set then if a property is empty, it will not be serialized.

##### Querying the Properties XML data

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
