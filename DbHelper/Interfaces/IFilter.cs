namespace DbHelper.Core
{
    public interface IFilter
    {
        int Page { get; set; }
        int PerPage { get; set; }
        bool SortAscending { get; set; }
        string OrderBy { get; set; }
    }
}
