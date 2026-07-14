using Microsoft.Data.SqlClient;
using System.Data;

namespace Puyaq.Infrastructure.Interface;

public interface IConnectionBase
{
    Task<DataTable> ExecuteDataTableAsync(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteNonQueryAsync(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default);

    Task<T?> ExecuteSingleAsync<T>(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default);

    Task<List<T>> ExecuteListAsync<T>(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default);
}