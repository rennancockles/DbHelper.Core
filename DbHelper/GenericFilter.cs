namespace DbHelper.Core
{
    public abstract class GenericFilter : IFilter
    {
        [NotMapped]
        public int Page { get; set; } = 1;
        [NotMapped]
        public int PerPage { get; set; } = 25;
        [NotMapped]
        public bool SortAscending { get; set; } = true;
        [NotMapped]
        public string OrderBy { get; set; }
    }
}
