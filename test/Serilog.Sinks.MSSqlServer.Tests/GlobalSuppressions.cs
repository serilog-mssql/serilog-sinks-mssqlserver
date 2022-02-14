// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Supplying string literals and not using resources is accepted within this project.", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer.Tests")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Member names must match SQL server master DB objects.", Scope = "type", Target = "~T:Serilog.Sinks.MSSqlServer.Tests.TestUtils.sp_pkey")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Member names must match SQL server master DB objects.", Scope = "type", Target = "~T:Serilog.Sinks.MSSqlServer.Tests.TestUtils.SysIndex_CCI")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "In net452 there is no Contains() overload that accepts a StringComparison specifier", Scope = "namespaceanddescendants", Target = "Serilog.Sinks.MSSqlServer.Tests")]
