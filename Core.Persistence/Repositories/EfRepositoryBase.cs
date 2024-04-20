using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Persistence.Repositories
{
    public class EfRepositoryBase<TEntity, TEntityId, TContext> : IAsyncRepository<TEntity, TEntityId>
        where TEntity : Entity<TEntityId>
        where TContext : DbContext
    {
        protected readonly TContext Context;

        public EfRepositoryBase(TContext context)
        {
            Context = context;
        }

        public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            //Sorgumuz
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking) //enableTracking açık değilse
                queryable = queryable.AsNoTracking(); //kapat
            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters(); //Bu kontrol, withDeleted parametresi true ise soft delete filtresini geçersiz kılar ve silinmiş kayıtları da sorguya dahil eder.
            if (predicate != null)
                queryable = queryable.Where(predicate);
            if (orderBy != null)
                return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);
            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query().ToDynamic(dynamic);
            if (!enableTracking)
                queryable = queryable.AsNoTracking();
            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters();
            if (predicate != null)
                queryable = queryable.Where(predicate);
            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking)
                queryable = queryable.AsNoTracking();
            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters();
            return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        }


        public IQueryable<TEntity> Query() => Context.Set<TEntity>();


        public async Task<TEntity> AddAsync(TEntity entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await Context.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
        {
            foreach (TEntity entity in entities)
                entity.CreatedDate = DateTime.UtcNow;
            await Context.AddRangeAsync(entities);
            await Context.SaveChangesAsync();
            return entities;
        }


        //Belirttiğimiz predicate(filtre) değerine göre elimize veri var mı yokmu onu döndüren metot.
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking)    //enableTracking false ise
                queryable = queryable.AsNoTracking(); //AsNoTracking => veriyi takip edilmeyen bir hale getirir.
            if (withDeleted) //silinmiş kayıtlar da sorguya dahil edildi ise
                queryable = queryable.IgnoreQueryFilters(); //IgnoreQueryFilters=> sorgu filtrelerini yoksay
            if (predicate != null) //predicate null değil ise
                queryable = queryable.Where(predicate); //sorguyu o predicate değeri ile filtrele.
            return await queryable.AnyAsync(cancellationToken); //Bu satır, sorgulanabilir veri üzerinde AnyAsync metodunun asenkron olarak çağrılmasını sağlar. Bu metot, koleksiyonda en azından bir öğe olup olmadığını kontrol eder ve sonucu await anahtar kelimesi kullanılarak asenkron olarak bekler. Sonuç olarak true (en az bir öğe varsa) veya false (hiç öğe yoksa) değeri döndürülür.
        }

        public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
        {
            await SetEntityAsDeletedAsync(entity, permanent); //silme işlemini bir metot yapsın. Burası bizim nesnemizin silineceğine mi yoksa güncelleneceğine mi (delete tarihi) karar verecek.
            await Context.SaveChangesAsync();
            return entity;
        }

        //Çoklu Silme İşlemi
        public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
        {
            await SetEntityAsDeletedAsync(entities, permanent);
            await Context.SaveChangesAsync();
            return entities;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            Context.Update(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
        {
            foreach (TEntity entity in entities)
                entity.UpdatedDate = DateTime.UtcNow;
            Context.UpdateRange(entities);
            await Context.SaveChangesAsync();
            return entities;
        }


        protected async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
        {
            if (!permanent) //kalıcı değilse(soft delete ise)
            {
                CheckHasEntityHaveOneToOneRelation(entity); //Bire bir ilişkisi var mı?
                await setEntityAsSoftDeletedAsync(entity);
            }
            else
            {
                Context.Remove(entity);
            }
        }

        //Örneğin; E-ticaret sitesinde bir ürünü sildiğimizde, satışlar tablosunda ona ait bütün satışların deleted hale gelmesi gerekiyor. SoftDelete güzel bir yapı,lakin ilişkisel yapılarda sorun yaşatmakta.
        //Bu yüzden burada bütün ilişkisel noktalar bulunup, ürünü sildiğimizde (soft delete yaptığımızda) ürüne ait tüm satışların da deleted duruma çekilmesi gerekmekte.
        //Burada hem reflection hem de entity framework'ün bize sağladığı imkanlardan yararlanılmaktadır.
        //GetRelationLoaderQuery, burada bütün ilişkileri bulmaya yarayan bir metottur.
        private async Task setEntityAsSoftDeletedAsync(IEntityTimestamps entity)
        {
            if (entity.DeletedDate.HasValue) //Entity'nin DeletedDate değeri var ise
                return;
            entity.DeletedDate = DateTime.UtcNow;

            var navigations = Context
                .Entry(entity)
                .Metadata.GetNavigations()
                .Where(x => x is { IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade })
                .ToList();
            foreach (INavigation? navigation in navigations)
            {
                if (navigation.TargetEntityType.IsOwned())
                    continue;
                if (navigation.PropertyInfo == null)
                    continue;

                object? navValue = navigation.PropertyInfo.GetValue(entity);
                if (navigation.IsCollection)
                {
                    if (navValue == null)
                    {
                        IQueryable query = Context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                        navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType()).ToListAsync();
                        if (navValue == null)
                            continue;
                    }
                    foreach (IEntityTimestamps navValueItem in (IEnumerable)navValue)
                        await setEntityAsSoftDeletedAsync(navValueItem);
                }
                else
                {
                    if (navValue == null)
                    {
                        IQueryable query = Context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                        navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType())
                            .FirstOrDefaultAsync();
                        if (navValue == null)
                            continue;
                    }
                    await setEntityAsSoftDeletedAsync((IEntityTimestamps)navValue);
                }
            }
            Context.Update(entity);
        }


        //Bire bir ilişkisi var mı yokmu bunu tespit eden metot.
        protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
        {
            //İlgili contexte, ilgili entitynin Metadatasının foreign keylerini al. 
            bool hasEntityHaveOneToOneRelation =
                Context
                    .Entry(entity)
                    .Metadata.GetForeignKeys()
                    .All(
                        x =>
                            x.DependentToPrincipal?.IsCollection == true
                            || x.PrincipalToDependent?.IsCollection == true
                            || x.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType == entity.GetType()
                    ) == false;
            if (hasEntityHaveOneToOneRelation) //bire bir ilişkisi varsa bununla ilgili hatayı fırlat.
                throw new InvalidOperationException(
                    "Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key."
                );
        }


        //Bizim bütün ilişkilerimizi bulmaya yarayacak metot
        protected IQueryable<object> GetRelationLoaderQuery(IQueryable query, Type navigationPropertyType)
        {
            Type queryProviderType = query.Provider.GetType();
            MethodInfo createQueryMethod =
                queryProviderType
                    .GetMethods()
                    .First(m => m is { Name: nameof(query.Provider.CreateQuery), IsGenericMethod: true })
                    ?.MakeGenericMethod(navigationPropertyType)
                ?? throw new InvalidOperationException("CreateQuery<TElement> method is not found in IQueryProvider.");
            var queryProviderQuery =
                (IQueryable<object>)createQueryMethod.Invoke(query.Provider, parameters: new object[] { query.Expression })!;
            return queryProviderQuery.Where(x => !((IEntityTimestamps)x).DeletedDate.HasValue);
        }

        protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
        {
            foreach (TEntity entity in entities)
                await SetEntityAsDeletedAsync(entity, permanent);
        }
    }
}
