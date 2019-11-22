using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
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
                values += $"{c.ParameterName.Replace("@", "")} = {c.ParameterName},";
            }

            foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
            {
                key = p.Name;
            }

            values = values.Remove(values.Length - 1);
            list.Add(BuildParameter("id", id));

            string query = $"UPDATE {table} SET {values} WHERE {key} = @id";

            bool success = Update(query, list.ToArray());

            if (success && obj.GetType().BaseType == typeof(DBModel))
            {
                (obj as DBModel).GetDBChanges();
            }

            return success;
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
                    values += $"{c.ParameterName.Replace("@", "")} = {c.ParameterName},";
                }

                foreach (PropertyInfo p in obj.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(Key))))
                {
                    key = p.Name;
                }

                values = values.Remove(values.Length - 1);
                list.Add(BuildParameter("id", id));

                string query = $"UPDATE {table} SET {values} WHERE {key} = @id";

                ExecuteNonQuery(query, list.ToArray());
            });
        }
    }
}
