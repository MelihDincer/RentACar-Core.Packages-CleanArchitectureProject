namespace Core.Persistence.Dynamic;

public class Filter
{
    public string Field { get; set; } //Yakıt tipi
    public string? Value { get; set; } //Field'ın değeri (null olabilir)
    public string Operator { get; set; } //Veri tipine göre, vites tipi üzerinde içinde geçene göre ya da eşittir e göre operatörlerimiz olabilir.
    public string? Logic { get; set; } //and, or logicleri olabilir
    public IEnumerable<Filter> Filters { get; set; } //Birden fazla filtre olabilir

    public Filter()
    {
        Field = string.Empty;
        Operator = string.Empty;
    }
    public Filter(string field, string @operator)
    {
        Field = field;
        Operator = @operator;
    }
}
