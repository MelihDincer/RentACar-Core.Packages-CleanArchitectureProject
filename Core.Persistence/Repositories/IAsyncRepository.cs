using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IAsyncRepository<TEntity, TEntityId>:IQuery<TEntity>
    where TEntity : Entity<TEntityId>
{
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate, //lambda ile where koşulu kullanımı için diyebiliriz
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, //join desteği için, IQueryable=> verileri query edebilecek. IIncludableQueryable=>
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true, //Ef tracking(izleme) desteğinin enable edilip edilmediği burada belirtildi.
        CancellationToken cancellationToken = default //İptal etme işlemi için gerekli olan değerdir. Default değer atanmıştır. Şuan için önemli değildir
        );

    //IPaginate sayfalama işlemi için
    Task<Paginate<TEntity>> GetListAsync( 
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, //Sıralama işlemini yapabilmek için.
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0, //Kaçıncı sayfa (index)
        int size = 10, //Seçilen sayfada kaç veri listelensin
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true,
        CancellationToken cancellationToken = default
        );

    //Burada hem sayfalama hem de dynamic sorgu yapılabilmektedir.
    Task<Paginate<TEntity>> GetListByDynamicAsync(
        //Dinamik sorgular yapabilmek için.
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, //Sıralama işlemini yapabilmek için.
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0, //Kaçıncı sayfa (index)
        int size = 10, //Seçilen sayfada kaç veri listelensin
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true,
        CancellationToken cancellationToken = default
        );

    //Aradığımız veri var mı yok mu?, örneğin böyle bir TC numarası var mı yokmu?
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null, //predicate geçmezsek data varmı diye bakar, predicate geçersek o koşulda data varmı diye bakacaktır.
        bool withDeleted = false,
        bool enableTracking = true, //enableTracking => verilerin Entity Framework tarafından takip edilip edilmeyeceğini belirler
        CancellationToken cancellationToken = default // Bu parametre, uzun süren asenkron işlemleri iptal etmek için kullanılabilir.
        );

    Task<TEntity> AddAsync(TEntity entity);

    //Çoklu ekleme
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities);

    Task<TEntity> UpdateAsync(TEntity entity);

    //Çoklu güncelleme
    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities);

    //permanent=kalıcı , soft delete işlemi için bu parametre verildi. Yani listelenirken gözükmeyecek lakin db de deleted date tarihi eklenerek kalacaktır.
    Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false);

    //Çoklu silme
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false);

}