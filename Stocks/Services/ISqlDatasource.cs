using System.Data;

namespace Stocks.Services
{
    public interface ISqlDatasource
    {
        IDbConnection CreateConnection();
    }
}
