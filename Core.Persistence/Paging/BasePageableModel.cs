namespace Core.Persistence.Paging;

public abstract class BasePageableModel
{
    public int Size { get; set; } //Sayfadaki data sayısı
    public int Index { get; set; } //Kaçıncı sayfa olduğumuz
    public int Count { get; set; } //Kaç veri olduğu
    public int Pages { get; set; } //Toplam kaç sayfamızın olduğu
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
