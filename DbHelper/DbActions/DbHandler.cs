using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DbHelper.Core
{
    public abstract partial class DbHandler
    {
        DbProviderFactory _factory;
        private readonly string _connectionString;
        private readonly bool _isSqlServer;
        private int? _timeout;
        private string _provider;

        public DbHandler(string connectionString, string provider)
        {
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            _connectionString = connectionString;
            _provider = provider;
            _factory = DbProviderFactories.GetFactory(_provider);
            _isSqlServer = (_factory == SqlClientFactory.Instance) ? true : false;
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

        private List<DbParameter> MountCustomerParameter<T>(T obj)
        {
            List<DbParameter> sqlparams = new List<DbParameter>();

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                bool isNotKey = prop.GetCustomAttributes(typeof(Key), true).Length == 0;
                bool isMapped = prop.GetCustomAttributes(typeof(NotMapped), true).Length == 0;

                if (isNotKey && isMapped)
                {
                    string propTypeName = GetPropTypeName(prop);
                    sqlparams.Add(BuildParameter(prop.Name, prop.GetValue(obj), propTypeName));
                }
            }

            return sqlparams;
        }
        private List<List<DbParameter>> MountCustomerParameter<T>(List<T> objs)
        {
            List<List<DbParameter>> result = new List<List<DbParameter>>();
            List<DbParameter> sqlparams;
            int i = 0;

            foreach (T obj in objs)
            {
                sqlparams = new List<DbParameter>();

                PropertyInfo[] props = obj
                    .GetType()
                    .GetProperties()
                    .Where(prop => prop.GetCustomAttributes().All(attr => !new[] { typeof(Key), typeof(NotMapped) }.Contains(attr.GetType())))
                    .ToArray();

                foreach (PropertyInfo prop in props)
                {
                    string paramName = $"{prop.Name}{i}";
                    object paramValue = prop.GetValue(obj);
                    string propTypeName = GetPropTypeName(prop);

                    sqlparams.Add(BuildParameter(paramName, paramValue, propTypeName));
                }

                result.Add(sqlparams);
                i++;
            }

            return result;
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
                        .FirstOrDefault(p => p.PropertyType.Name == b.GetType().Name && p.CanRead && p.GetValue(a) == null);

                    propInfo?.SetValue(a, b);
                }
            }

            return (T)classes.FirstOrDefault();
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

        private string GetSplitOn(List<Type> types)
        {
            return string.Join(",", types.Select(t => GetKeyProp(t)).ToArray());
        }

        private string GetKeyProp(Type tipo)
        {
            return tipo.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(Key)))?.Name ?? "";
        }

        private string GetPropTypeName(PropertyInfo prop)
        {
            if (prop.PropertyType.BaseType.Name == "Enum") return "Int32";
            else if (prop.PropertyType.Name.Contains("Nullable")) return prop.PropertyType.GenericTypeArguments.Select(arg => arg.Name).FirstOrDefault();
            else return prop.PropertyType.Name;
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
    }
}
