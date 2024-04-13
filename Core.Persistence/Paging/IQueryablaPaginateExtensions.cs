using Microsoft.EntityFrameworkCore;

namespace Core.Persistence.Paging;

public static class IQueryablaPaginateExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(this IQueryable<T> source, int index,int size,
        CancellationToken cancellationToken = default) //Asenkron çalıştığımız için bunu verdik        
    {
        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false); //await configurasyonu yapılmasın olarak ayarladık.
        List<T> items = await source.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false); //index*size kadar datayı atla ve 

        Paginate<T> list = new()
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
        return list;
    }

    public static Paginate<T> ToPaginate<T>(this IQueryable<T> source,int index,int size)
    {
        int count = source.Count(); //await configurasyonu yapılmasın olarak ayarladık.
        List<T> items = source.Skip(index * size).Take(size).ToList(); //index*size kadar datayı atla ve 

        Paginate<T> list = new()
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
        return list;
    }
}
