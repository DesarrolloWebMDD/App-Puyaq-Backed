using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Puyaq.CrossCutting.Settings;
using Puyaq.Infrastructure.Interface;
using System.Data;

namespace Puyaq.Infrastructure.Implementacion;

/// <summary>
/// Gestiona la ejecución de Stored Procedures en SQL Server.
/// </summary>
public sealed class ConnectionBase : IConnectionBase
{
    private readonly string _connectionString;

    public ConnectionBase(IOptions<AppSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _connectionString =
            options.Value.ConnectionStrings.DefaultConnection;

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró AppSettings:ConnectionStrings:DefaultConnection.");
        }
    }

    /// <summary>
    /// Ejecuta un Stored Procedure y devuelve un DataTable.
    /// </summary>
    public async Task<DataTable> ExecuteDataTableAsync(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        ValidateStoredProcedure(storedProcedure);

        await using var connection = CreateConnection();

        await using var command = CreateCommand(
            connection,
            storedProcedure,
            parameters,
            commandTimeout);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var dataTable = new DataTable();

        dataTable.Load(reader);

        return dataTable;
    }

    /// <summary>
    /// Ejecuta un Stored Procedure sin devolver registros.
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        ValidateStoredProcedure(storedProcedure);

        await using var connection = CreateConnection();

        await using var command = CreateCommand(
            connection,
            storedProcedure,
            parameters,
            commandTimeout);

        await connection.OpenAsync(cancellationToken);

        return await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    /// <summary>
    /// Ejecuta un Stored Procedure y devuelve un único registro.
    /// </summary>
    public async Task<T?> ExecuteSingleAsync<T>(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        ValidateStoredProcedure(storedProcedure);

        await using var connection = CreateConnection();

        await using var command = CreateCommand(
            connection,
            storedProcedure,
            parameters,
            commandTimeout);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SingleRow,
                cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return default;
        }

        return Map<T>(reader);
    }

    /// <summary>
    /// Ejecuta un Stored Procedure y devuelve una lista de registros.
    /// </summary>
    public async Task<List<T>> ExecuteListAsync<T>(
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters = null,
        int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        ValidateStoredProcedure(storedProcedure);

        await using var connection = CreateConnection();

        await using var command = CreateCommand(
            connection,
            storedProcedure,
            parameters,
            commandTimeout);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<T>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(Map<T>(reader));
        }

        return results;
    }

    private SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string storedProcedure,
        IEnumerable<SqlParameter>? parameters,
        int commandTimeout)
    {
        var command = connection.CreateCommand();

        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = commandTimeout;

        if (parameters is not null)
        {
            command.Parameters.AddRange(
                parameters.ToArray());
        }

        return command;
    }

    private static T Map<T>(SqlDataReader reader)
    {
        var type = typeof(T);

        var columns = Enumerable
            .Range(0, reader.FieldCount)
            .ToDictionary(
                index => reader.GetName(index),
                index => index,
                StringComparer.OrdinalIgnoreCase);

        /*
         * Busca un constructor cuyos parámetros coincidan
         * con las columnas devueltas por el Stored Procedure.
         *
         * Esto permite mapear records posicionales.
         */
        var constructor = type
        .GetConstructors()
        .Where(item =>
            item.GetParameters().Length > 0)
        .OrderByDescending(item =>
            item.GetParameters().Length)
        .FirstOrDefault(item =>
            item.GetParameters().All(parameter =>
                parameter.Name is not null &&
                columns.ContainsKey(parameter.Name)));

        if (constructor is not null)
        {
            var values = constructor
                .GetParameters()
                .Select(parameter =>
                {
                    var index = columns[parameter.Name!];

                    if (reader.IsDBNull(index))
                    {
                        return null;
                    }

                    return ConvertValue(
                        reader.GetValue(index),
                        parameter.ParameterType);
                })
                .ToArray();

            return (T)constructor.Invoke(values);
        }

        /*
         * Si no encuentra constructor compatible,
         * intenta mapear mediante propiedades.
         */
        var instance = Activator.CreateInstance<T>();

        var properties = type
            .GetProperties()
            .Where(property => property.CanWrite);

        foreach (var property in properties)
        {
            if (!columns.TryGetValue(
                    property.Name,
                    out var index))
            {
                continue;
            }

            if (reader.IsDBNull(index))
            {
                continue;
            }

            var value = ConvertValue(
                reader.GetValue(index),
                property.PropertyType);

            property.SetValue(
                instance,
                value);
        }

        return instance;
    }

    private static object? ConvertValue(
        object value,
        Type targetType)
    {
        var underlyingType =
            Nullable.GetUnderlyingType(targetType)
            ?? targetType;

        if (underlyingType.IsAssignableFrom(
                value.GetType()))
        {
            return value;
        }

        if (underlyingType.IsEnum)
        {
            return value is string stringValue
                ? Enum.Parse(
                    underlyingType,
                    stringValue,
                    ignoreCase: true)
                : Enum.ToObject(
                    underlyingType,
                    value);
        }

        if (underlyingType == typeof(Guid))
        {
            return value is Guid guid
                ? guid
                : Guid.Parse(value.ToString()!);
        }

        if (underlyingType == typeof(DateTimeOffset))
        {
            return value is DateTimeOffset dateTimeOffset
                ? dateTimeOffset
                : new DateTimeOffset(
                    Convert.ToDateTime(value));
        }

        return Convert.ChangeType(
            value,
            underlyingType);
    }

    private static void ValidateStoredProcedure(
        string storedProcedure)
    {
        if (string.IsNullOrWhiteSpace(storedProcedure))
        {
            throw new ArgumentException(
                "El nombre del Stored Procedure es obligatorio.",
                nameof(storedProcedure));
        }
    }
}