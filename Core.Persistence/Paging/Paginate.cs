namespace Core.Persistence.Paging;

public class Paginate<T> //Nesneler değişebileceği için T generic yapı kurduk
{
    public Paginate()
    {
        Items = Array.Empty<T>(); //Items başta boş bir array olarak tanımlandı. Sınıf newlendiğinde.
    }

    public int Size { get; set; } //Sayfadaki data sayısı
    public int Index { get; set; } //Kaçıncı sayfa olduğumuz
    public int Count { get; set; } //Kaç veri olduğu
    public int Pages { get; set; } //Toplam kaç sayfamızın olduğu
    public IList<T> Items { get; set; }

    public bool HasPrevious => Index > 0; //Girdiğimiz Index 0'dan büyükse önceki sayfa var demek
    public bool HasNext => Index+1 < Pages ; //Toplam 10 sayfa olduğunu düşünelim. 9.indexte yani 10.sayfada ise son sayfada demek. 9+1 < 10 false döndürecektir. Çünkü 10.sayfadan başka sayfa yok. Kısaca mevcutta olduğumuz indexin bir fazlası kaçıncı sayfada olduğumuzu belirttiği için, toplam sayfa sayısından küçük olması durumunda sonraki sayfanın var olduğu sonucu çıkarılmaktadır.

}
