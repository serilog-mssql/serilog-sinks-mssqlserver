using System;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal abstract class SqlCommandExecutor : ISqlCommandExecutor
    {
        private readonly ISqlWriter _sqlWriter;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ISqlCommandFactory _sqlCommandFactory;

        public SqlCommandExecutor(
            ISqlWriter sqlWriter,
            ISqlConnectionFactory sqlConnectionFactory,
            ISqlCommandFactory sqlCommandFactory)
        {
            _sqlWriter = sqlWriter ?? throw new ArgumentNullException(nameof(sqlWriter));
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _sqlCommandFactory = sqlCommandFactory ?? throw new ArgumentNullException(nameof(sqlCommandFactory));
        }

        public void Execute()
        {
            try
            {
                using (var conn = _sqlConnectionFactory.Create())
                {
                    var sql = _sqlWriter.GetSql();
                    using (var cmd = _sqlCommandFactory.CreateCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                throw;
            }
        }

        protected abstract void HandleException(Exception ex);
    }
}
