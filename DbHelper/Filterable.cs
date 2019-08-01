using System.Collections.Generic;

namespace DbHelper.Core
{
    public class Filterable<T> : IFilterable
    {
        public int LastPage { get; set; }
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public decimal Total { get; set; }
        public List<T> Data { get; set; }
    }
}
