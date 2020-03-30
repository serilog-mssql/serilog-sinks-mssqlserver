5.2.0
 * Enhancement #232: Allow to override formatter for rendering LogEvent column.
 * Fixed #187 (again - still an exception when using logevent column with TimeStamp column type DateTimeOffset).
 * Added sample programs

5.1.4
 * Fixed #187 Support datetimeoffset as a column type for default column TimeStamp.
 * Fixed #229 Slight issue with documentation.

5.1.3
 * Support binary data type, support specify data length in column config, support specify allow null column
 * Also build on unit-test commits
 * Added issue templase
 * Hybrid config implementation
 * Bugfixes

5.1.2
 * Support for Audit sink added (#118/#110).

4.0.0
 * Serilog 2.0
 * [Documentation fix](https://github.com/serilog/serilog-sinks-mssqlserver/pull/32)

2.0.33
 * Option added to exclude redundant properties from serialized JSON in column LogEvent. (https://github.com/serilog/serilog-sinks-mssqlserver/pull/27)

2.0.32
 * Safe conversion of data types. Also included selflog for bulk operation errors. (https://github.com/serilog/serilog-sinks-mssqlserver/pull/4)

2.0.31
 * Added the ability to configure additional columns via XML configuration (https://github.com/serilog/serilog-sinks-mssqlserver/pull/6)

2.0.30
 * You can optionally save the log event inside the database too. Also added ability to exclude the properties if they are saved already inside additional columns. (https://github.com/serilog/serilog-sinks-mssqlserver/pull/7)

2.0.28
 * Added explicit column mappings (https://github.com/serilog/serilog-sinks-mssqlserver/pull/10) 

2.0.27
 * Option added to automatically create a database table (by Kiran Varsani (https://github.com/varsanikp))

2.0.13
 * Ability to add additional properties as columns in the database

2.0.1
 * Option to [write times in UTC](https://github.com/serilog/serilog-sinks-mssqlserver/pull/1)

1.5
 * Moved from serilog/serilog
