namespace Core.Persistence.Dynamic;

public class Sort
{
    public string Field { get; set; } //Hangi field için uygulanacağını belirtmek için.
    public string Dir { get; set; } //Dir=Direction (sıralamanın ascending mi descending mi olduğunu belirtmek için.

    public Sort()
    {
        Field = string.Empty;
        Dir = string.Empty;
    }

    public Sort(string field, string dir)
    {
        Field = field;
        Dir = dir;
    }
}
