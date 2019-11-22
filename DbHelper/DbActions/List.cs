using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
        public List<T> GetList<T>(string query, List<DbParameter> parameters)
        {
            return GetList<T>(query, parameters.ToArray());
        }
        public List<T> GetList<T, T1>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3, T4>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3, T4>(query, split, parameters.ToArray());
        }
        public List<T> GetList<T, T1, T2, T3, T4, T5>(string query, string split, List<DbParameter> parameters)
        {
            return GetList<T, T1, T2, T3, T4, T5>(query, split, parameters.ToArray());
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
    }
}
