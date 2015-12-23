# Serilog.Sinks.MSSqlServer

[![Build status](https://ci.appveyor.com/api/projects/status/3btbux1hbgyugind/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-mssqlserver/branch/master)

A Serilog sink that writes events to Microsoft SQL Server. While a NoSql store allows for more flexibility to store the different kinds of properties, it sometimes is easier to use an already existing MS SQL server. This sink will write the logevent data to a table and can optionally also store the properties inside an Xml column so they can be queried.

**Package** - [Serilog.Sinks.MSSqlServer](http://nuget.org/packages/serilog.sinks.mssqlserver)
| **Platforms** - .NET 4.5

```csharp
var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString: @"Server=...", tableName: "Logs")
    .CreateLogger();
```

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
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
     ON [PRIMARY]

) ON [PRIMARY];
```

If you don't plan on using the Properties or LogEvent columns, you can disable their use with the *storeProperties* and *storeLogEvent* parameters.

NOTE Make sure to set up security in such a way that the sink can write to the log table. 

### XML configuration

If you are configuring Serilog with the `ReadFrom.AppSettings()` XML configuration support, you can use:

```xml
<add key="serilog:using:MSSqlSever" value="Serilog.Sinks.MSSqlServer" />
<add key="serilog:write-to:MSSqlServer.connectionString" value="Server=..."/>
<add key="serilog:write-to:MSSqlServer.tableName" value="Logs"/>
```

### Writing properties as columns

This feature will still use all of the default columns and provide additional columns for that can be logged to (be sure to create the extra columns via SQL script first). This gives the flexibility to use as many extra columns as needed.

```csharp
var dataColumns = new[]
    {
        new DataColumn { DataType = typeof(string), ColumnName = "User" },
        new DataColumn { DataType = typeof(string), ColumnName = "Other" },
    };
    
var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=...", "Logs", additionalDataColumns: dataColumns)
    .CreateLogger();
```
The log event properties `User` and `Other` will now be placed in the corresponding column upon logging. The property name must match a column name in your table.

### Auto create table

If you set the *autoCreateSqlTable* option to true, it will create a table for you in the database specified in the connection string. Make sure that the user associated with this connection string has enough rights to make schema changes.

#### Excluding redundant items from the Properties column

By default the additional properties will still be included in the XML data saved to the Properties column (assuming that is not disabled via the storeProperties parameter). That's consistent with the idea behind structured logging, and makes it easier to convert the log data to another (e.g. NoSql) storage platform later if desired.  

However, if the data is to stay in SQL Server, then the additional properties may not need to be saved in both columns and XML.  Use the *excludeAdditionalProperties* parameter in the sink configuration to exclude the redundant properties from the XML.

### Saving the Log Event data

The log event JSON can be stored to the LogEvent column. This can be enabled with the *storeLogEvent* parameter.
