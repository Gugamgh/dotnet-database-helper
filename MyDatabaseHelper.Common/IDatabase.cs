using System.Data;

namespace MyDatabaseHelper.Common;

public interface IDatabase<out TConnection, TCommand, TDataReader, TParameter> : IDisposable
{
    TConnection GetConnection();
    void OpenConnection();
    void CloseConnection();

    TParameter CreateParameter(string name, object? value);

    TCommand GetCommand(string commandText, CommandType commandType, params TParameter[] parameters);
    TCommand GetCommand(string commandText, params TParameter[] parameters);

    int ExecuteNonQuery(string commandText, CommandType commandType, params TParameter[] parameters);
    int ExecuteNonQuery(string commandText, params TParameter[] parameters);

    object? ExecuteScalar(string commandText, CommandType commandType, params TParameter[] parameters);
    object? ExecuteScalar(string commandText, params TParameter[] parameters);

    TDataReader ExecuteReader(string commandText, CommandType commandType, CommandBehavior commandBehavior, params TParameter[] parameters);
    TDataReader ExecuteReader(string commandText, CommandType commandType, params TParameter[] parameters);
    TDataReader ExecuteReader(string commandText, CommandBehavior commandBehavior, params TParameter[] parameters);
    TDataReader ExecuteReader(string commandText, params TParameter[] parameters);

    DataTable GetTable(string commandText, CommandType commandType, CommandBehavior commandBehavior, params TParameter[] parameters);
    DataTable GetTable(string commandText, CommandType commandType, params TParameter[] parameters);
    DataTable GetTable(string commandText, CommandBehavior commandBehavior, params TParameter[] parameters);
    DataTable GetTable(string commandText, params TParameter[] parameters);

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}