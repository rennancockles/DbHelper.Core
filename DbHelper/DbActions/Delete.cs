using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
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
    }
}
