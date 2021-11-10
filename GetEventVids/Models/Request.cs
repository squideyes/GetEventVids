namespace GetEventVids;

public class Request
{
    public int ItemsPerPage { get; set; } = 1000;
    public string SearchText { get; set; } = "*";
    public int SearchPage { get; set; } = 1;
    public string SortOption { get; set; } = "None";
    public SearchFacets SearchFacets { get; set; } = new SearchFacets();
}

public class DateFacet
{
    public DateTime StartDateTime { get; set; } = new DateTime(2020, 9, 22);
    public DateTime EndDateTime { get; set; } = new DateTime(2020, 9, 24);
}

public class SearchFacets
{
    public List<string> Facets { get; set; } = new List<string>();
    public List<string> PersonalizationFacets { get; set; } = new List<string>();
    public DateFacet DateFacet { get; set; } = new DateFacet();
}
