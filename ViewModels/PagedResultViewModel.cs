namespace DemoApp.Web.ViewModels
{
    public class PagedResultViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
