// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Supplying string literals and not using resources is accepted within this project.", Scope = "namespaceanddescendants", Target = "~N:Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Too hard to change. Accepted for now.", Scope = "namespaceanddescendants", Target = "~N:Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "namespaceanddescendants", Target = "~N:Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "type", Target = "~T:Serilog.Sinks.MSSqlServer.ColumnOptions")]
[assembly: SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "namespaceanddescendants", Target = "~N:Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Performance", "CA1822: Mark members as static", Justification = "Using instance members over static for better testablilty.", Scope = "namespaceanddescendants", Target = "~N:Serilog.Sinks.MSSqlServer")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.SetProperty.IfNotNull``1(System.String,Serilog.Sinks.MSSqlServer.SetProperty.PropertySetter{``0})")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Platform.SqlCommandExecutor.Execute()")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Output.StandardColumnDataGenerator.ConvertPropertiesToXmlStructure(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,Serilog.Events.LogEventPropertyValue}})~System.String")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Too hard to change. Accepted for now.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.Output.AdditionalColumnDataGenerator.TryChangeType(System.Object,System.Type,System.Object@)~System.Boolean")]
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "type", Target = "~T:Serilog.Sinks.MSSqlServer.StandardColumnConfigException")]
[assembly: SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Cannot be changed on public classes for backward compatibility reasons.", Scope = "member", Target = "~M:Serilog.Sinks.MSSqlServer.MSSqlServerSink.EmitBatchAsync(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})~System.Threading.Tasks.Task")]
