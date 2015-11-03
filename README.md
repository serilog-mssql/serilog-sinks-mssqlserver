# Serilog.Sinks.MSSqlServer

[![Build status](https://ci.appveyor.com/api/projects/status/3btbux1hbgyugind/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-mssqlserver/branch/master)

A Serilog sink that writes events to Microsoft SQL Server. While a NoSql store allows for more flexibility to store the different kinds of properties, it sometimes is easier to use an already existing MS SQL server. This sink will write the logevent data to a table and can optionally also store the properties inside an Xml column so they can be queried.

**Package** - [Serilog.Sinks.MSSqlServer](http://nuget.org/packages/serilog.sinks.mssqlserver)
| **Platforms** - .NET 4.5

You'll need to create a database and add a table like the one you can find in this [Gist](https://gist.github.com/mivano/10429656). 

```csharp
var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=.\SQLEXPRESS;Database=LogEvents;Trusted_Connection=True;", "Logs")
    .CreateLogger();
```

Make sure to set up security in such a way that the sink can write to the log table. If you don't plan on using the properties, then you can disable the storage of them. 

**Using Additional Columns**

```csharp
var dataColumns = new[]
        {
            new DataColumn { DataType = Type.GetType( "System.String" ), ColumnName = "User" },
            new DataColumn { DataType = Type.GetType( "System.String" ), ColumnName = "Other" },
        };
var log = new LoggerConfiguration()
    .WriteTo.MSSqlServer(@"Server=.\SQLEXPRESS;Database=LogEvents;Trusted_Connection=True;", "Logs", additionalDataColumns: dataColumns)
    .CreateLogger();
```
The properties 'User' and 'Other' will now be placed in the corresponding column upon logging. The property name must match a column name in your table.
