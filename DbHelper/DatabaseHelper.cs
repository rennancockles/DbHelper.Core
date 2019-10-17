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
        private readonly bool _isSqlServer;
        private string _provider;
        private int? _timeout;

        private DatabaseHelper(string connectionString, string provider)
        {
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);


            _connectionString = connectionString;
            _provider = provider;
            _factory = DbProviderFactories.GetFactory(_provider);
            _isSqlServer = (_factory == SqlClientFactory.Instance) ? true : false;
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

        public int Save(string query, List<DbParameter> parameters)
        {
            return Save(query, parameters.ToArray());
        }
        public int Save(string query, params DbParameter[] parameters)
        {
            return ExecuteScalar(query, parameters);
        }
        public int Save<T>(T obj)
        {
            List<DbParameter> list = MountCustomerParameter<T>(obj);

            string table = GetCorrectTableName(obj);
            string values = string.Join(",", list.Select(c => c.ParameterName));
            string columns = values.Replace("@", "");

            string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";

            int id = ExecuteScalar(query, list.ToArray());

            return id;
        }
        public Task<int> SaveAsync<T>(T obj)
        {
            return Task.Run(() =>
            {
                List<DbParameter> list = MountCustomerParameter<T>(obj);

                string table = GetCorrectTableName(obj);
                string values = string.Join(",", list.Select(c => c.ParameterName));
                string columns = values.Replace("@", "");

                string query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, columns, values);

                return ExecuteScalar(query, list.ToArray());
            });
        }

        public bool Update(string query, List<DbParameter> parameters)
        {
            return Update(query, parameters.ToArray());
        }
        public bool Update(string query, params DbParameter[] parameters)
        {
            return ExecuteAffectedLines(query, parameters) > 0;
        }
        public bool Update<K, T>(K id, T obj)
        {
            List<DbParameter> list = MountCustomerParameter<T>(obj);
            string table = GetCorrectTableName(obj);
            string values = string.Empty;
            string key = string.Empty;

            foreach (DbParameter c in list)
            {
                values += string.Format("{0} = {1},", c.ParameterName.Replace("@", ""), c.ParameterName);
            }

            foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
            {
                key = p.Name;
            }

            values = values.Remove(values.Length - 1);
            list.Add(BuildParameter("id", id));

            string query = $"UPDATE {table} SET {values} WHERE {key} = @id";

            return Update(query, list.ToArray());
        }
        public void UpdateAsync<K, T>(K id, T obj)
        {
            Task.Run(() =>
            {

                List<DbParameter> list = MountCustomerParameter<T>(obj);
                string table = GetCorrectTableName(obj);
                string values = string.Empty;
                string key = string.Empty;

                foreach (DbParameter c in list)
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

        public bool Delete(string query, List<DbParameter> parameters)
        {
            return Delete(query, parameters.ToArray());
        }
        public bool Delete(string query, params DbParameter[] parameters)
        {
            return ExecuteAffectedLines(query, parameters) > 0;
        }
        public bool Delete<T, K>(K id)
        {
            T obj = default;
            obj = Activator.CreateInstance<T>();

            foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
            {
                string query = $"DELETE FROM {GetCorrectTableName(obj)} WHERE {p.Name} = @id";
                return Delete(query, BuildParameter("id", id));
            }

            return false;
        }

        public T Get<T>(string query, List<DbParameter> parameters)
        {
            return Get<T>(query, parameters.ToArray());
        }
        public T Get<T>(string query, params DbParameter[] parameters)
        {
            return GetList<T>(query, parameters).FirstOrDefault();
        }
        public T Get<T, T1>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1>(query, split, parameters.ToArray());
        }
        public T Get<T, T1>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3, T4>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3, T4>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4, T5>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3, T4, T5>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3, T4, T5>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4, T5>(query, split, parameters).FirstOrDefault();
        }

        public List<T> GetList<T>(string query, List<DbParameter> parameters)
        {
            return GetList<T>(query, parameters.ToArray());
        }
        public List<T> GetList<T>(string query, params DbParameter[] parameters)
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
        public List<T> GetList<T, T1>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1>(string query, string split, params DbParameter[] parameters)
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
        public List<T> GetList<T, T1, T2>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2>(string query, string split, params DbParameter[] parameters)
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
        public List<T> GetList<T, T1, T2, T3>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3>(string query, string split, params DbParameter[] parameters)
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
        public List<T> GetList<T, T1, T2, T3, T4>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3, T4>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3, T4>(string query, string split, params DbParameter[] parameters)
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
        public List<T> GetList<T, T1, T2, T3, T4, T5>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3, T4, T5>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3, T4, T5>(string query, string split, params DbParameter[] parameters)
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

        public List<T> GetList<T>(string query, object[] classes, List<DbParameter> parameters)
        {
            return GetList<T>(query, classes, parameters.ToArray());
        }
        public List<T> GetList<T>(string query, object[] classes, params DbParameter[] parameters)
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                conn.Open();

                object[] objs = new object[classes.Length + 1];

                List<Type> typeList = classes.Select(cls => cls.GetType()).ToList();
                typeList.Insert(0, typeof(T));

                string splitOn = GetSplitOn(typeList);

                DynamicParameters dynParams = new DynamicParameters(new { });
                parameters.ToList().ForEach(p => dynParams.Add(p.ParameterName.Split('.')[0], p.Value));

                return conn.Query(query,
                    typeList.ToArray(),
                    obj =>
                    {
                        objs[0] = (T)Convert.ChangeType(obj[0], typeof(T));
                        for (int i = 1; i < classes.Length + 1; i++)
                            objs[i] = Convert.ChangeType(obj[i], typeList[i]);

                        return DynamicMapper<T>(objs);
                    },
                    splitOn: splitOn,
                    param: dynParams,
                    commandTimeout: _timeout).ToList();
            }
        }

        private string GetSplitOn(List<Type> types)
        {
            return string.Join(",", types.Select(t => GetKeyProp(t)).ToArray());
        }

        private string GetKeyProp(Type tipo)
        {
            return tipo.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(Key)))?.Name ?? "";
        }

        private T DynamicMapper<T>(params object[] classes)
        {
            int tamanho = classes.Length;

            for (int i = 0; i < tamanho; i++)
            {
                object a = classes[i];
                if (a is null) continue;

                for (int j = 0; j < tamanho; j++)
                {
                    object b = classes[j];
                    if (b is null) continue;

                    PropertyInfo propInfo = a
                        .GetType()
                        .GetProperties()
                        .FirstOrDefault(p => p.PropertyType.Name == b.GetType().Name && p.GetValue(a) == null);

                    propInfo?.SetValue(a, b);
                }
            }

            return (T)classes.FirstOrDefault();
        }

        public async Task<List<T>> GetListAsync<T>(string query, params DbParameter[] parameters)
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

        public DbParameter BuildParameter(string nome, object valor)
        {
            DbParameter parametro = _factory.CreateParameter();
            parametro.ParameterName = GetCorrectParameterName(nome);
            parametro.Value = valor;

            return parametro;
        }
        public DbParameter BuildParameter(string nome, object valor, string tipo)
        {
            DbParameter parametro = _factory.CreateParameter();
            parametro.ParameterName = GetCorrectParameterName(nome);
            parametro.Value = valor;
            parametro.DbType = (DbType)Enum.Parse(typeof(DbType), tipo, true);
            
            return parametro;
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

        private List<DbParameter> MountCustomerParameter<T>(T obj)
        {
            List<DbParameter> sqlparams = new List<DbParameter>();

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
        private string GetCorrectTableName(object obj)
        {
            TableName tableName = (TableName)obj.GetType().GetCustomAttributes(typeof(TableName), false).FirstOrDefault();

            if (tableName != null && !string.IsNullOrEmpty(tableName.Name))
            {
                return tableName.Name;
            }
            else
            {
                return obj.GetType().Name.ToLower();
            }

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