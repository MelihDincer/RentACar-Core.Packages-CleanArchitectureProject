namespace Core.Persistence.Dynamic;

public class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; set; }
    public Filter? Filter { get; set; } //İç içe filtre oluşturulduğu için bu şekilde tekil eklendi. Birden çok filtreyi iç içe tanımlayan bir yapı mevcut

    public DynamicQuery()
    {
        
    }
    public DynamicQuery(IEnumerable<Sort>? sort, Filter? filter)
    {
        Filter = filter;
        Sort = sort;
    }
}
