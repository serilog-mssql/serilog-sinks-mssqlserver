// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Supplying string literals and not using resources is accepted within this project.", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Too hard to change. Accepted for now.", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.SetProperty.IfNotNull``1(System.String,Serilog.Sinks.MSSqlServer.SetProperty.PropertySetter{``0})")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Platform.SqlTableCreator.CreateTable(System.Data.DataTable)")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Platform.SqlBulkBatchWriter.WriteBatch(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent},System.Data.DataTable)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Output.StandardColumnDataGenerator.ConvertPropertiesToXmlStructure(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,Serilog.Events.LogEventPropertyValue}})~System.String")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Output.PropertiesColumnDataGenerator.TryChangeType(System.Object,System.Type,System.Object@)~System.Boolean")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "type", Target = "~T:Serilog.Sinks.MSSqlServer.ColumnOptions")]
[assembly: SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Performance", "CA1822: Member AllowNull does not access instance data and can be marked as static (Shared in VisualBasic)", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "member", Target = "~P:Serilog.Sinks.MSSqlServer.ColumnOptions.IdColumnOptions.AllowNull")]
