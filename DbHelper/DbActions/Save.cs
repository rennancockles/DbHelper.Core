using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
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

            if (id > 0 && obj.GetType().BaseType == typeof(DBModel))
            {
                (obj as DBModel).GetDBChanges();
            }

            return id;
        }

        public int Save<T>(List<T> objs)
        {
            T obj = objs[0];
            List<List<DbParameter>> list = MountCustomerParameter(objs);

            string table = GetCorrectTableName(obj);
            string values = string.Join("),(", list.Select(l => string.Join(",", l.Select(c => c.ParameterName))));
            string columns = string.Join(",", list.First().Select(p => Regex.Replace(p.ParameterName.Replace("@", ""), @"\d+$", "")));

            string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";

            int id = ExecuteScalar(query, list.SelectMany(x => x).ToArray());

            if (id > 0 && obj.GetType().BaseType == typeof(DBModel))
            {
                objs.ForEach(o => (o as DBModel).GetDBChanges());
            }

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

                string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";

                return ExecuteScalar(query, list.ToArray());
            });
        }
    }
}
