using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DbHelper.Core
{
    public class QueryBuilder<T>
    {
        internal string orderBy;
        internal string pagination;
        internal string queryCount;
        internal string queryComplete;
        internal string filter;
        internal string fields;
        internal string split;
        internal string queryInicial;

        public string Create { get; set; }
        public Filterable<T> Filterable { get; set; }
        public SqlParameter[] SQLParams => sqlParams.ToArray();

        private readonly List<SqlParameter> sqlParams;

        public QueryBuilder()
        {
            sqlParams = new List<SqlParameter>();
            Filterable = new Filterable<T>();
        }

        public void AddFilter(IFilter filter)
        {
            Type type = filter.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());
            this.filter = "WHERE 1 = 1 ";

            foreach (PropertyInfo prop in props)
            {
                object[] notMapped = prop.GetCustomAttributes(typeof(NotMapped), true);
                object[] column = prop.GetCustomAttributes(typeof(Column), true);

                string value = (column.Length > 0) ? (column[0] as Column).Name : prop.Name;
                string op = (column.Length > 0) ? (column[0] as Column).Operator : "=";

                object propValue = prop.GetValue(filter, null);

                if (!string.IsNullOrEmpty(propValue?.ToString()) && notMapped.Length == 0)
                {
                    SqlParameter parametro;
                    if (prop.PropertyType.Name == "String")
                    {
                        if (op.ToLower().Equals("like"))
                        {
                            parametro = new SqlParameter(prop.Name, string.Format("%{0}%", propValue.ToString()));
                            sqlParams.Add(parametro);
                            this.filter += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                            Create += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                        }
                        else if (op.ToLower().Equals("in"))
                        {
                            this.filter += string.Format(" AND {0} {1} ({2}) ", value, op, propValue.ToString());
                            Create += string.Format(" AND {0} {1} ({2}) ", value, op, propValue.ToString());
                        }
                        else
                        {
                            parametro = new SqlParameter(prop.Name, propValue.ToString());
                            sqlParams.Add(parametro);
                            this.filter += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                            Create += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                        }
                    }
                    else if (prop.PropertyType.Name.Contains("Int"))
                    {
                        if (int.Parse(propValue.ToString()) != 0)
                        {
                            parametro = new SqlParameter(prop.Name, int.Parse(propValue.ToString()));
                            sqlParams.Add(parametro);
                            this.filter += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                            Create += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                        }
                    }
                    else if (prop.PropertyType.Name == "DateBetween")
                    {
                        DateTime? dtStart = (propValue as DateBetween)?.StartDate;
                        DateTime? dtEnd = (propValue as DateBetween)?.EndDate;

                        if (dtStart != null && dtEnd != null)
                        {
                            parametro = new SqlParameter("start", dtStart?.ToString("yyyy-MM-dd"))
                            {
                                DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime", true)
                            };
                            sqlParams.Add(parametro);

                            parametro = new SqlParameter("end", dtEnd?.ToString("yyyy-MM-dd"))
                            {
                                DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime", true)
                            };
                            sqlParams.Add(parametro);

                            this.filter += string.Format(" AND {0} {1} @{2} AND @{3} ", value, op, "start", "end");
                            Create += string.Format(" AND {0} {1} @{2} AND @{3} ", value, op, "start", "end");
                        }
                    }
                    else if (prop.PropertyType.Name == "DateTime")
                    {
                        if (DateTime.MinValue != DateTime.Parse(propValue.ToString()))
                        {
                            {
                                DateTime dt = DateTime.Parse(propValue.ToString());
                                parametro = new SqlParameter(prop.Name, dt.ToString("yyyy-MM-dd"))
                                {
                                    DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime", true)
                                };
                                sqlParams.Add(parametro);
                                this.filter += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                                Create += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                            }
                        }
                    }
                    else if (prop.PropertyType.Name == "Boolean")
                    {
                        parametro = new SqlParameter(prop.Name, propValue.ToString());
                        sqlParams.Add(parametro);
                        this.filter += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                        Create += string.Format(" AND {0} {1} @{2} ", value, op, prop.Name);
                    }
                }
            }
        }

        public void AddFilter(SqlParameter[] list)
        {
            filter = "WHERE 1 = 1 ";
            Create += "WHERE 1 = 1 ";

            foreach (SqlParameter c in list)
            {
                string fieldName = Regex.Replace(c.ParameterName, "[.@]", "");
                c.ParameterName = $"@{fieldName}";

                filter += $"AND {fieldName} = {c.ParameterName} ";
                Create += $"AND {fieldName} = {c.ParameterName} ";

                sqlParams.Add(c);
            }
        }
        public void OrderBy(IFilter filter)
        {
            Type type = filter.GetType();
            _ = new List<PropertyInfo>(type.GetProperties());

            if (string.IsNullOrEmpty(filter.OrderBy))
            {
                orderBy = "ORDER BY 1 ";
            }
            else
            {
                orderBy = string.Format("ORDER BY {0} ", filter.OrderBy);
            }

            orderBy += filter.SortAscending ? "ASC " : "DESC ";
            Create += orderBy;
        }

        public void Pagination(int page, int per_page)
        {
            decimal lastpage = Math.Ceiling((Filterable.Total / per_page));

            Filterable.LastPage = lastpage > 0 ? (int)Math.Ceiling(lastpage) : 1;

            if (page <= lastpage)
            {
                Filterable.From = 1;
                Filterable.To = per_page;
                if (page > 1)
                {
                    int offset = (page - 1) * per_page;

                    /// TODO => Rever Logica
                    queryComplete += string.Format(" OFFSET {0} ROWS", offset.ToString());
                    pagination += string.Format(" OFFSET {0} ROWS", offset.ToString());
                    Create += string.Format(" OFFSET {0} ROWS", offset.ToString());

                    Filterable.To = per_page * page;
                    Filterable.From = (Filterable.To - per_page) + 1;
                }
                else
                {
                    queryComplete += " OFFSET 0 ROWS ";
                    pagination += " OFFSET 0 ROWS ";
                    Create += " OFFSET 0 ROWS ";
                }

                queryComplete += string.Format(" FETCH NEXT {0} ROWS ONLY", per_page.ToString());
                pagination += string.Format(" FETCH NEXT {0} ROWS ONLY", per_page.ToString());
                Create += string.Format(" FETCH NEXT {0} ROWS ONLY", per_page.ToString());

                if (Filterable.To > Filterable.Total)
                {
                    Filterable.To = (int)Filterable.Total;
                }

                Filterable.PerPage = per_page;
                Filterable.CurrentPage = page;
            }
        }

        internal void FormatQuery(SqlParameter[] list)
        {
            AddFilter(list);

            if (fields.Length > 0) fields = fields.Substring(0, fields.Length - 1);

            if (split.Length > 0) split = split.Substring(0, split.Length - 1);

            queryInicial = queryInicial.Replace("++", fields);

            queryComplete = $"{queryInicial} {filter} {orderBy}";
        }

        internal void FormatQuery(IFilter filter)
        {
            AddFilter(filter);

            if (fields.Length > 0) fields = fields.Substring(0, fields.Length - 1);

            if (split.Length > 0) split = split.Substring(0, split.Length - 1);

            queryInicial = queryInicial.Replace("++", fields);

            queryCount = $"{queryCount} {this.filter}";

            if (string.IsNullOrEmpty(orderBy)) OrderBy(filter);

            queryComplete = $"{queryInicial} {this.filter} {orderBy} {pagination}";
        }

        internal void FormatQuery(IFilter filter, bool isList = true)
        {
            AddFilter(filter);

            if (fields.Length > 0) fields = fields.Substring(0, fields.Length - 1);

            if (split.Length > 0) split = split.Substring(0, split.Length - 1);

            queryInicial = queryInicial.Replace("++", fields);

            queryComplete = $"{queryInicial} {this.filter} {orderBy}";
        }
    }
}
