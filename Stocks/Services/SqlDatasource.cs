using Microsoft.Data.SqlClient;
using System.Data;

namespace Stocks.Services
{
    public class SqlDatasource : ISqlDatasource
    {
        private readonly string _connectionString;

        public SqlDatasource(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

}
