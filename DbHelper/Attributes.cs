using System;

namespace DbHelper.Core
{
    public class Key : Attribute
    {
        public string Name { get; set; }
    }

    public class NotMapped : Attribute
    {
        public string Name { get; set; }
    }

    public class Column : Attribute
    {
        public string Name;
        public string Operator;

        public Column(string name)
        {
            Name = name;
            Operator = "=";
        }

        public Column(string name, string op)
        {
            Name = name;
            Operator = op;
        }

        public class Object : Attribute
        {
            public string Name;
        }
    }
}