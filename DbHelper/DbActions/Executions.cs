using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
        public int ExecuteScalar(string query, params DbParameter[] parameters)
        {
            int id = 0;
            string lastId = _isSqlServer ? "CAST(scope_identity() AS int)" : "LAST_INSERT_ID()";

            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandTimeout = _timeout ?? 30;
                        command.CommandText = $"{query};SELECT {lastId};";
                        command.Parameters.AddRange(parameters);
                        id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    catch (DbException ex)
                    {
                        throw new InvalidOperationException(ex.Message + " - " + command.CommandText, ex);
                    }
                }
            }

            return (int)id;
        }

        public int ExecuteAffectedLines(string query, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = query;
                    command.CommandTimeout = _timeout ?? 30;
                    if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public DbDataReader ExecuteDataReader(string query, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandTimeout = _timeout ?? 30;
                        command.CommandText = query;
                        command.Parameters.AddRange(parameters);
                        return command.ExecuteReader();
                    }
                    catch (DbException ex)
                    {
                        throw new InvalidOperationException($"{ex.Message} - {command.CommandText}", ex);
                    }
                }
            }
        }

        public DataTable ExecuteDataTable(string query, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandTimeout = _timeout ?? 30;
                        command.CommandText = query;
                        command.Parameters.AddRange(parameters);
                        DbDataReader dataReader = command.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dataReader);
                        return dt;
                    }
                    catch (DbException ex)
                    {
                        throw new InvalidOperationException($"{ex.Message} - {command.CommandText}", ex);
                    }
                }
            }
        }

        public void ExecuteNonQuery(string query, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = query;
                    command.CommandTimeout = _timeout ?? 30;
                    if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task ExecuteNonQueryAsync(string query, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters);
                    command.CommandTimeout = _timeout ?? 30;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
