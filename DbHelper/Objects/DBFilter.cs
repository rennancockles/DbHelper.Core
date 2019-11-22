using System.Collections.Generic;
using System.Data.Common;

namespace DbHelper.Core
{
    public abstract class DBFilter
    {
        public DBFilter()
        {
            QueryWhere = "";
            DbParameters = new List<DbParameter>();
        }

        public string QueryWhere { get; set; }
        public List<DbParameter> DbParameters { get; set; }

        public abstract void FillQueryWhereAndParams(DatabaseHelper dbHelper);
    }
}
