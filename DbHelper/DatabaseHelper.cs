using Dapper;
using Microsoft.Win32.SafeHandles;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DbHelper.Core
{
    public class DatabaseHelper : IDisposable
    {
        bool disposed = false;
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        DbProviderFactory _factory;

        private readonly string _connectionString;
        private string _provider;
        private int? _timeout;

        private DatabaseHelper(string connectionString, string provider)
        {
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            _connectionString = connectionString;
            _provider = provider;
            _factory = DbProviderFactories.GetFactory(_provider);
        }

        public static DatabaseHelper Create(string connectionString, string provider = "System.Data.SqlClient")
        {
            return new DatabaseHelper(connectionString, provider);
        }

        public void SetTimeout(int timeout)
        {
            _timeout = timeout;
        }
        public void SetProvider(string provider)
        {
            _provider = provider;
            _factory = DbProviderFactories.GetFactory(_provider);
        }

        public int Save(string query, params SqlParameter[] parameters)
        {
            return ExecuteScalar(query, parameters);
        }
        public int Save<T>(T obj)
        {
            List<SqlParameter> list = MountCustomerParameter<T>(obj);

            string table = obj.GetType().Name;
            string values = string.Join(",", list.Select(c => c.ParameterName));
            string columns = values.Replace("@", "");

            string query = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", table, columns, values);

            int id = ExecuteScalar(query, list.ToArray());

            return id;
        }
        public Task<int> SaveAsync<T>(T obj)
        {
            return Task.Run(() =>
            {
                List<SqlParameter> list = MountCustomerParameter<T>(obj);

                string table = obj.GetType().Name;
                string values = string.Join(",", list.Select(c => c.ParameterName));
                string columns = values.Replace("@", "");

                string query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, columns, values);

                return ExecuteScalar(query, list.ToArray());
            });
        }

        public bool Update(string query, params SqlParameter[] parameters)
        {
            return ExecuteAffectedLines(query, parameters) > 0;
        }
        public void Update<K, T>(K id, T obj)
        {
            List<SqlParameter> list = MountCustomerParameter<T>(obj);
            string table = obj.GetType().Name;
            string values = string.Empty;
            string key = string.Empty;

            foreach (SqlParameter c in list)
            {
                values += string.Format("{0} = {1},", c.ParameterName.Replace("@", ""), c.ParameterName);
            }

            foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
            {
                key = p.Name;
            }

            values = values.Remove(values.Length - 1);
            list.Add(BuildParameter("id", id));

            string query = string.Format("UPDATE [{0}] SET {1} WHERE {2} = @id", table, values, key);

            ExecuteNonQuery(query, list.ToArray());
        }
        public void UpdateAsync<K, T>(K id, T obj)
        {
            Task.Run(() =>
            {

                List<SqlParameter> list = MountCustomerParameter<T>(obj);
                string table = obj.GetType().Name;
                string values = string.Empty;
                string key = string.Empty;

                foreach (SqlParameter c in list)
                {
                    values += string.Format("{0} = {1},", c.ParameterName.Replace("@", ""), c.ParameterName);
                }

                foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
                {
                    key = p.Name;
                }

                values = values.Remove(values.Length - 1);
                list.Add(BuildParameter("id", id));

                string query = string.Format("UPDATE {0} SET {1} WHERE {2} = @id", table, values, key);

                ExecuteNonQuery(query, list.ToArray());
            });
        }

        public bool Delete(string query, params SqlParameter[] parameters)
        {
            return ExecuteAffectedLines(query, parameters) > 0;
        }
        public void Delete<T, K>(K id)
        {
            T obj = default;
            obj = Activator.CreateInstance<T>();

            foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
            {
                string query = string.Format("DELETE FROM [{0}] WHERE {1} = @id", obj.GetType().Name, p.Name);
                ExecuteNonQuery(query, BuildParameter("id", id));
            }
        }

        public T Get<T>(string query, params SqlParameter[] parameters)
        {
            return GetList<T>(query, parameters).FirstOrDefault();
        }
        public T Get<T, T1>(string query, string split, params SqlParameter[] parameters)
        {
            return GetList<T, T1>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2>(string query, string split, params SqlParameter[] parameters)
        {
            return GetList<T, T1, T2>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3>(string query, string split, params SqlParameter[] parameters)
        {
            return GetList<T, T1, T2, T3>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4>(string query, string split, params SqlParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4, T5>(string query, string split, params SqlParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4, T5>(query, split, parameters).FirstOrDefault();
        }

        public List<T> GetList<T>(string query, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName, p.Value));

                if (parameters.Length == 0) return conn.Query<T>(query).ToList();
                else return conn.Query<T>(query, dynParams, commandTimeout: _timeout).ToList();
            }
        }
        public List<T> GetList<T, T1>(string query, string split, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                T1 t1 = default;
                t1 = Activator.CreateInstance<T1>();

                var dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query<T, T1, T>(query,
                    map: (a, b) => DynamicMapper<T>(a, b),
                    splitOn: split,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }
        public List<T> GetList<T, T1, T2>(string query, string split, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                var dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query<T, T1, T2, T>(query,
                    map: (a, b, c) => DynamicMapper<T>(a, b, c),
                    splitOn: split,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }
        public List<T> GetList<T, T1, T2, T3>(string query, string split, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                DynamicParameters dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query<T, T1, T2, T3, T>(query,
                    map: (a, b, c, d) => DynamicMapper<T>(a, b, c, d),
                    splitOn: split,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }
        public List<T> GetList<T, T1, T2, T3, T4>(string query, string split, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                DynamicParameters dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query<T, T1, T2, T3, T4, T>(query,
                    map: (a, b, c, d, e) => DynamicMapper<T>(a, b, c, d, e),
                    splitOn: split,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }
        public List<T> GetList<T, T1, T2, T3, T4, T5>(string query, string split, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                DynamicParameters dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query<T, T1, T2, T3, T4, T5, T>(query,
                    map: (a, b, c, d, e, f) => DynamicMapper<T>(a, b, c, d, e, f),
                    splitOn: split,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }

        private T DynamicMapper<T>(params object[] classes)
        {
            int tamanho = classes.Length;

            for (int i = 0; i < tamanho; i++)
            {
                object a = classes[i];

                for (int j = 0; j < tamanho; j++)
                {
                    object b = classes[j];

                    a.GetType().GetProperty(b.GetType().Name)?.SetValue(a, b);
                }
            }

            return (T)classes.FirstOrDefault();
        }

        public async Task<List<T>> GetListAsync<T>(string query, params SqlParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                await conn.OpenAsync();
                IEnumerable<T> ts;
                DynamicParameters dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName, p.Value));

                if (parameters.Length == 0) ts = await conn.QueryAsync<T>(query);
                else ts = await conn.QueryAsync<T>(query, dynParams, commandTimeout: _timeout);

                return ts.ToList();
            }
        }

        public SqlParameter BuildParameter(string nome, object valor)
        {
            SqlParameter parametro = new SqlParameter(GetCorrectParameterName(nome), valor);
            return parametro;
        }
        public SqlParameter BuildParameter(string nome, object valor, string tipo)
        {
            SqlParameter parametro = new SqlParameter(GetCorrectParameterName(nome), valor)
            {
                DbType = (DbType)Enum.Parse(typeof(DbType), tipo, true)
            };
            return parametro;
        }

        public int ExecuteAffectedLines(string query, params SqlParameter[] parameters)
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
        public void ExecuteNonQuery(string query, params SqlParameter[] parameters)
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
        public async Task ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
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

        public int ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            int id = 0;

            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                using (DbCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandTimeout = _timeout ?? 30;
                        command.CommandText = query + ";SELECT CAST(scope_identity() AS int);";
                        command.Parameters.AddRange(parameters);
                        id = (int)command.ExecuteScalar();
                    }
                    catch (DbException ex)
                    {
                        throw new InvalidOperationException(ex.Message + " - " + command.CommandText, ex);
                    }
                }
            }

            return (int)id;
        }

        public DbDataReader ExecuteDataReader(string query, params SqlParameter[] parameters)
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

        public DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
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

        private List<SqlParameter> MountCustomerParameter<T>(T obj)
        {
            List<SqlParameter> sqlparams = new List<SqlParameter>();

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                object[] key = prop.GetCustomAttributes(typeof(Key), true);
                object[] notMapped = prop.GetCustomAttributes(typeof(NotMapped), true);

                if (key.Length == 0 && notMapped.Length == 0 && GetPropValue(obj, prop.Name) != null)
                {
                    if (prop.PropertyType.Name.Contains("Nullable"))
                    {
                        sqlparams.Add(BuildParameter(prop.Name, GetPropValue(obj, prop.Name)));
                    }
                    else
                    {
                        sqlparams.Add(BuildParameter(prop.Name, GetPropValue(obj, prop.Name), prop.PropertyType.Name));
                    }
                }
            }

            return sqlparams;
        }

        private string GetCorrectParameterName(string parameterName)
        {
            if (parameterName[0] != '@')
            {
                parameterName = "@" + parameterName;
            }

            return parameterName;
        }

        private object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            disposed = true;
        }
    }
}