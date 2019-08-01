using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DbHelper.Core
{
    public abstract class AbstractDAO<T> where T : new()
    {
        protected DatabaseHelper dbHelper;
        public QueryBuilder<T> Query { get; set; }
        public string TableName
        {
            get
            {
                T obj = Activator.CreateInstance<T>();
                return obj.GetType().Name;
            }
        }

        public AbstractDAO(string connectionString, string provider = "System.Data.SqlClient")
        {
            dbHelper = DatabaseHelper.Create(connectionString, provider);
            Query = new QueryBuilder<T>
            {
                orderBy = string.Empty,
                pagination = string.Empty,
                filter = string.Empty,
                split = string.Empty,
                queryComplete = string.Empty,
                fields = string.Format("[{0}].*,", TableName),
                queryInicial = string.Format("SELECT ++ FROM [{0}] (NOLOCK) ", TableName),
                queryCount = string.Format("SELECT COUNT(*) FROM [{0}] (NOLOCK) ", TableName),
            };
        }

        public abstract void Delete<K>(K id);
        public abstract int Save(T obj);
        public abstract void Update<k>(k id, T obj);

        public AbstractDAO<T> OrderBy(string orderBy)
        {
            Query.orderBy = orderBy;
            return this;
        }

        public AbstractDAO<T> Fields(string fields)
        {
            Query.fields = fields;
            return this;
        }

        public AbstractDAO<T> InnerJoin<T1>(string column1, string column2)
        {
            T1 t1 = Activator.CreateInstance<T1>();

            Query.fields += string.Format("[{0}].*,", t1.GetType().Name);
            Query.split += string.Format("{0}Id,", t1.GetType().Name);
            Query.queryInicial += string.Format("INNER JOIN [{0}] (NOLOCK) ON {1} = {2} ", t1.GetType().Name, column1, column2);
            Query.queryCount += string.Format("INNER JOIN [{0}] (NOLOCK) ON {1} = {2} ", t1.GetType().Name, column1, column2);

            return this;
        }

        public AbstractDAO<T> LeftJoin<T1>(string column1, string column2)
        {
            T1 t1 = Activator.CreateInstance<T1>();

            Query.fields += string.Format("'' as {0}Split, [{0}].*,", t1.GetType().Name);
            Query.split += string.Format("{0}Split,", t1.GetType().Name);
            Query.queryInicial += string.Format("LEFT JOIN [{0}] (NOLOCK) ON {1} = {2} ", t1.GetType().Name, column1, column2);
            Query.queryCount += string.Format("INNER JOIN [{0}] (NOLOCK) ON {1} = {2} ", t1.GetType().Name, column1, column2);

            return this;
        }

        public T Get(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T>(Query.queryComplete, Query.SQLParams).FirstOrDefault();
        }

        public T Get<T1>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1>(Query.queryComplete, Query.split, Query.SQLParams).FirstOrDefault();
        }

        public T Get<T1, T2>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2>(Query.queryComplete, Query.split, Query.SQLParams).FirstOrDefault();
        }

        public T Get<T1, T2, T3>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3>(Query.queryComplete, Query.split, Query.SQLParams).FirstOrDefault();
        }

        public T Get<T1, T2, T3, T4>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3, T4>(Query.queryComplete, Query.split, Query.SQLParams).FirstOrDefault();
        }

        public T Get<T1, T2, T3, T4, T5>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3, T4, T5>(Query.queryInicial, Query.split, Query.SQLParams).FirstOrDefault();
        }

        public List<T> List(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T>(Query.queryComplete, Query.SQLParams);
        }

        public List<T> List<T1>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3, T4>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3, T4>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3, T4, T5>(params SqlParameter[] list)
        {
            Query.FormatQuery(list);
            return dbHelper.GetList<T, T1, T2, T3, T4, T5>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T>(Query.queryComplete, Query.SQLParams);
        }

        public List<T> List<T1>(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T, T1>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2>(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T, T1, T2>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3>(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T, T1, T2, T3>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3, T4>(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T, T1, T2, T3, T4>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public List<T> List<T1, T2, T3, T4, T5>(IFilter filter, bool isList = true)
        {
            Query.FormatQuery(filter, true);
            return dbHelper.GetList<T, T1, T2, T3, T4, T5>(Query.queryComplete, Query.split, Query.SQLParams);
        }

        public IFilterable List(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T>(Query.queryComplete, Query.SQLParams);

            return Query.Filterable;
        }

        public IFilterable List<T1>(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T, T1>(Query.queryComplete, Query.split, Query.SQLParams);

            return Query.Filterable;
        }

        public IFilterable List<T1, T2>(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T, T1, T2>(Query.queryComplete, Query.split, Query.SQLParams);

            return Query.Filterable;
        }

        public IFilterable List<T1, T2, T3>(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T, T1, T2, T3>(Query.queryComplete, Query.split, Query.SQLParams);

            return Query.Filterable;
        }

        public IFilterable List<T1, T2, T3, T4>(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T, T1, T2, T3, T4>(Query.queryComplete, Query.split, Query.SQLParams);

            return Query.Filterable;
        }
        public IFilterable List<T1, T2, T3, T4, T5>(IFilter filter)
        {
            Query.FormatQuery(filter);
            Query.Filterable.Total = dbHelper.GetList<int>(Query.queryCount, Query.SQLParams).FirstOrDefault();
            Query.Pagination(filter.Page, filter.PerPage);
            Query.Filterable.Data = dbHelper.GetList<T, T1, T2, T3, T4, T5>(Query.queryComplete, Query.split, Query.SQLParams);

            return Query.Filterable;
        }
    }
}
