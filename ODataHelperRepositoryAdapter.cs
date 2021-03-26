using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using ODataHelper.AspNet.Abstractions;

namespace ODataHelper.AspNet
{
    public abstract class ODataHelperRepositoryAdapter<TEntity> : BaseODataHelperRepositoryAdapter<TEntity>
        where TEntity : class
    {
        protected abstract ValueTask<TEntity> GetEntityAsync(object key, IQueryable<TEntity> query);
        protected abstract ValueTask<int> RemoveEntityAsync(TEntity entity);
        protected abstract ValueTask<TEntity> InsertEntityAsync(TEntity entity);
        protected abstract ValueTask<TEntity> UpdateEntityAsync(TEntity entity);

        private static IQueryable ApplyQueryOptions(IQueryable<TEntity> query,
            ODataQueryOptions options, ODataQuerySettings querySettings,
            AllowedQueryOptions ignoreQueryOptions)
        {
            _ = query ?? throw new ArgumentNullException(nameof(query));
            if (options == null) return query;
            var result = querySettings == null
                ? options.ApplyTo(query, ignoreQueryOptions)
                : options.ApplyTo(query, querySettings, ignoreQueryOptions);
            return result;
        }
        private static TEntity ApplyQueryOptions(TEntity entity,
            ODataQueryOptions options, ODataQuerySettings querySettings,
            AllowedQueryOptions ignoreQueryOptions)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            if (options == null) return entity;
            querySettings = querySettings ?? new ODataQuerySettings();
            var result = options.ApplyTo(entity, querySettings, ignoreQueryOptions) as TEntity;
            return result;
        }
        protected virtual IQueryable<TEntity> PrepareQuery(
            ref ODataQueryOptions<TEntity> options, ref ODataQuerySettings querySettings,
            ref AllowedQueryOptions ignoreQueryOptions)
        {
            return AsQueryable();
        }
        public sealed override ValueTask<IQueryable> GetAsync(ODataQueryOptions<TEntity> options, ODataQuerySettings querySettings,
            AllowedQueryOptions ignoreQueryOptions)
        {
            var query = PrepareQuery(ref options, ref querySettings, ref ignoreQueryOptions);
            var result = ApplyQueryOptions(query, options, querySettings, ignoreQueryOptions);
            return new ValueTask<IQueryable>(result);
        }
        public sealed override async ValueTask<TEntity> GetAsync(object key, ODataQueryOptions<TEntity> options,
            ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            var query = PrepareQuery(ref options, ref querySettings, ref ignoreQueryOptions);
            var entity = await GetEntityAsync(key, query).ConfigureAwait(false);
            var result = ApplyQueryOptions(entity, options, querySettings, ignoreQueryOptions);
            return result;
        }
        public sealed override async ValueTask<int> RemoveAsync(TEntity entity)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            var result = await RemoveEntityAsync(entity).ConfigureAwait(false);
            return result;
        }
        public sealed override async ValueTask<TEntity> InsertAsync(TEntity entity)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            var result = await InsertEntityAsync(entity).ConfigureAwait(false);
            return result;
        }
        public sealed override async ValueTask<TEntity> PatchAsync(TEntity entity, Delta<TEntity> delta)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            _ = delta ?? throw new ArgumentNullException(nameof(delta));
            delta.Patch(entity);
            var result = await UpdateEntityAsync(entity).ConfigureAwait(false);
            return result;
        }
        public sealed override async ValueTask<TEntity> PutAsync(TEntity entity)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            var result = await UpdateEntityAsync(entity).ConfigureAwait(false);
            return result;
        }
    }
}
