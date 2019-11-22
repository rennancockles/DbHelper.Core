namespace DbHelper.Core
{
    public interface IFilterable
    {
        int LastPage { get; set; }
        int CurrentPage { get; set; }
        int PerPage { get; set; }
        int From { get; set; }
        int To { get; set; }
        decimal Total { get; set; }
    }
}
