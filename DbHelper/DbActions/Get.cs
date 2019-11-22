using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
        public T Get<T>(string query, List<DbParameter> parameters)
        {
            return Get<T>(query, parameters.ToArray());
        }
        public T Get<T, T1>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3, T4>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3, T4>(query, split, parameters.ToArray());
        }
        public T Get<T, T1, T2, T3, T4, T5>(string query, string split, List<DbParameter> parameters)
        {
            return Get<T, T1, T2, T3, T4, T5>(query, split, parameters.ToArray());
        }

        public T Get<T>(string query, params DbParameter[] parameters)
        {
            return GetList<T>(query, parameters).FirstOrDefault();
        }
        public T Get<T, T1>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4>(query, split, parameters).FirstOrDefault();
        }
        public T Get<T, T1, T2, T3, T4, T5>(string query, string split, params DbParameter[] parameters)
        {
            return GetList<T, T1, T2, T3, T4, T5>(query, split, parameters).FirstOrDefault();
        }

        public T Get<T>(string query, object[] classes, List<DbParameter> parameters)
        {
            return GetList<T>(query, classes, parameters.ToArray()).FirstOrDefault();
        }
        public T Get<T>(string query, object[] classes, params DbParameter[] parameters)
        {
            return GetList<T>(query, classes, parameters).FirstOrDefault();
        }
    }
}
