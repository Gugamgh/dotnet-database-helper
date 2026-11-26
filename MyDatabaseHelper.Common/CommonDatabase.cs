using System.Data;
using System.Runtime.CompilerServices;

namespace MyDatabaseHelper.Common;

public abstract class CommonDatabase<TConnection, TCommand, TDataReader, TParameter>
    : IDatabase<TConnection, TCommand, TDataReader, TParameter>
    where TConnection : class, IDbConnection, new()
    where TCommand : class, IDbCommand
    where TDataReader : class, IDataReader
    where TParameter : class, IDbDataParameter, new()
{
    private TConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;
    protected string ConnectionString { get; }

    protected CommonDatabase(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ConnectionString = connectionString;
    }

    protected virtual TConnection CreateNewConnection()
    {
        ThrowIfDisposed();
        TConnection connection = new();
        connection.ConnectionString = ConnectionString;
        return connection;
    }

    public TConnection GetConnection()
    {
        ThrowIfDisposed();
        return _connection ??= CreateNewConnection();
    }

    public void OpenConnection()
    {
        ThrowIfDisposed();
        TConnection connection = GetConnection();
        if (connection.State != ConnectionState.Open) connection.Open();
    }

    public void CloseConnection()
    {
        ThrowIfDisposed();
        if (_connection?.State == ConnectionState.Open) _connection.Close();
    }

    public virtual TParameter CreateParameter(string name, object? value)
    {
        ThrowIfDisposed();
        TParameter parameter = new();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }

    public TCommand GetCommand(string commandText, CommandType commandType, params TParameter[] parameters)
    {
        ThrowIfDisposed();
        TCommand command = (TCommand)GetConnection().CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        foreach (TParameter parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }
        command.Transaction = _transaction;

        return command;
    }

    public TCommand GetCommand(string commandText, params TParameter[] parameters)
        => GetCommand(commandText, CommandType.Text, parameters);

    public int ExecuteNonQuery(string commandText, CommandType commandType, params TParameter[] parameters)
    {
        ThrowIfDisposed();
        using TCommand command = GetCommand(commandText, commandType, parameters);
        OpenConnection();
        return command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string commandText, params TParameter[] parameters)
        => ExecuteNonQuery(commandText, CommandType.Text, parameters);

    public object? ExecuteScalar(string commandText, CommandType commandType, params TParameter[] parameters)
    {
        ThrowIfDisposed();
        using TCommand command = GetCommand(commandText, commandType, parameters);
        OpenConnection();
        return command.ExecuteScalar();
    }

    public object? ExecuteScalar(string commandText, params TParameter[] parameters)
        => ExecuteScalar(commandText, CommandType.Text, parameters);

    public TDataReader ExecuteReader(string commandText, CommandType commandType, CommandBehavior commandBehavior, params TParameter[] parameters)
    {
        ThrowIfDisposed();
        TCommand command = GetCommand(commandText, commandType, parameters);
        OpenConnection();
        TDataReader reader = (TDataReader)command.ExecuteReader(commandBehavior);
        return reader;
    }

    public TDataReader ExecuteReader(string commandText, CommandType commandType, params TParameter[] parameters)
        => ExecuteReader(commandText, commandType, CommandBehavior.Default, parameters);

    public TDataReader ExecuteReader(string commandText, CommandBehavior commandBehavior, params TParameter[] parameters)
        => ExecuteReader(commandText, CommandType.Text, commandBehavior, parameters);

    public TDataReader ExecuteReader(string commandText, params TParameter[] parameters)
        => ExecuteReader(commandText, CommandType.Text, CommandBehavior.Default, parameters);

    public DataTable GetTable(string commandText, CommandType commandType, CommandBehavior commandBehavior, params TParameter[] parameters)
    {
        ThrowIfDisposed();
        DataTable dataTable = new();
        using TDataReader reader = ExecuteReader(commandText, commandType, commandBehavior, parameters);
        dataTable.Load(reader);

        return dataTable;
    }

    public DataTable GetTable(string commandText, CommandType commandType, params TParameter[] parameters)
        => GetTable(commandText, commandType, CommandBehavior.Default, parameters);

    public DataTable GetTable(string commandText, CommandBehavior commandBehavior, params TParameter[] parameters)
        => GetTable(commandText, CommandType.Text, commandBehavior, parameters);

    public DataTable GetTable(string commandText, params TParameter[] parameters)
        => GetTable(commandText, CommandType.Text, CommandBehavior.Default, parameters);

    public void BeginTransaction()
    {
        ThrowIfDisposed();
        if (_transaction != null) throw new InvalidOperationException("A transaction is already in progress.");
        OpenConnection();
        _transaction = _connection!.BeginTransaction();
    }

    public void CommitTransaction()
    {
        ThrowIfDisposed();
        if (_transaction == null) throw new InvalidOperationException("No transaction is in progress.");
        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
    }

    public void RollbackTransaction()
    {
        ThrowIfDisposed();
        if (_transaction == null) throw new InvalidOperationException("No transaction is in progress.");
        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(CommonDatabase<TConnection, TCommand, TDataReader, TParameter>));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }

        _disposed = true;
    }

    ~CommonDatabase()
    {
        Dispose(false);
    }
}