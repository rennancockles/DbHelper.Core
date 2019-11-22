using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbHelper.Core
{
    public abstract class DBModel
    {
        public DBModel()
        {
            DBChanges = new List<DBChange>();
            FieldsChanged = new List<string>();
        }

        [NotMapped]
        public DBModel DBObject { get; set; }
        [NotMapped]
        public List<DBChange> DBChanges { get; set; }
        [NotMapped]
        public List<string> FieldsChanged { get; set; }

        public void GetDBChanges()
        {
            if (DBObject is null) DBObject = (DBModel)Activator.CreateInstance(GetType());
            
            foreach (PropertyInfo prop in DBObject.GetType().GetProperties())
            {
                object[] notMapped = prop.GetCustomAttributes(typeof(NotMapped), true);

                if (notMapped.Length == 0 &&
                    (GetPropValue(DBObject, prop.Name)?.ToString() ?? "") != (GetPropValue(this, prop.Name)?.ToString() ?? ""))
                {
                    DBChanges.Add(new DBChange
                    {
                        Field = prop.Name,
                        FromValue = GetPropValue(DBObject, prop.Name).ToString(),
                        ToValue = GetPropValue(this, prop.Name).ToString()
                    });

                    FieldsChanged.Add(prop.Name);
                }
            }
        }

        private object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null) ?? "";
        }
    }
}
