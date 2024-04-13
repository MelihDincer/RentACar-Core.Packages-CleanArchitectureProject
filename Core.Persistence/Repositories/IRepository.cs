using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IRepository<TEntity, TEntityId>:IQuery<TEntity>
    where TEntity : Entity<TEntityId>
{
    TEntity? Get(
        Expression<Func<TEntity, bool>> predicate, //lambda ile where koşulu kullanımı için diyebiliriz
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, //join desteği için, IQueryable=> verileri query edebilecek. IIncludableQueryable=>
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true //Ef tracking(izleme) desteğinin enable edilip edilmediği burada belirtildi.
        );

    //IPaginate sayfalama işlemi için
    Paginate<TEntity> GetList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, //Sıralama işlemini yapabilmek için.
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0, //Kaçıncı sayfa (index)
        int size = 10, //Seçilen sayfada kaç veri listelensin
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true
        );

    //Burada hem sayfalama hem de dynamic sorgu yapılabilmektedir.
    Paginate<TEntity> GetListByDynamic(
        //Dinamik sorgular yapabilmek için.
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, //Sıralama işlemini yapabilmek için.
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0, //Kaçıncı sayfa (index)
        int size = 10, //Seçilen sayfada kaç veri listelensin
        bool withDeleted = false, //false=>Silinenleri getirme.
        bool enableTracking = true
        );

    //Aradığımız veri var mı yok mu?, örneğin böyle bir TC numarası var mı yokmu?
    bool Any(
        Expression<Func<TEntity, bool>>? predicate = null, //predicate geçmezsek data varmı diye bakar, predicate geçersek o koşulda data varmı diye bakacaktır.
        bool withDeleted = false,
        bool enableTracking = true
        );

    TEntity Add(TEntity entity);

    //Çoklu ekleme
    ICollection<TEntity> AddRange(ICollection<TEntity> entities);

    TEntity Update(TEntity entity);

    //Çoklu güncelleme
    ICollection<TEntity> UpdateRange(ICollection<TEntity> entities);

    //permanent=kalıcı , soft delete işlemi için bu parametre verildi. Yani listelenirken gözükmeyecek lakin db de deleted date tarihi eklenerek kalacaktır.
    TEntity Delete(TEntity entity, bool permanent = false);

    //Çoklu silme
    ICollection<TEntity> DeleteRange(ICollection<TEntity> entities, bool permanent = false);
}
