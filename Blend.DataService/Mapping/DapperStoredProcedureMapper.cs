using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Blend.DataService.Mapping
{
    public sealed class DapperStoredProcedureMapper : IStoredProcedureMapper
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public DapperStoredProcedureMapper(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private IDbConnection CreateConnection()
        {
            return _connectionFactory() ?? throw new InvalidOperationException("The connection factory returned no database connection.");
        }

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class
        {
            using (var connection = CreateConnection())
            {
                return connection.QuerySingleOrDefault<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return connection.Query<T>(procedureName, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public int Execute(string procedureName, object? parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return connection.Execute(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
