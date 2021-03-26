using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using ODataHelper.AspNet.Abstractions;

namespace ODataHelper.AspNet
{
    public abstract class ODataHelperController<TEntity> : BaseODataHelperController<TEntity>
        where TEntity : class
    {
        protected IODataHelperRepositoryAdapter<TEntity> RepositoryAdapter { get; }
        protected ODataQuerySettings QuerySettings { get; } = new ODataQuerySettings
        {
            PageSize = 100,
        };
        protected AllowedQueryOptions IgnoreQueryOptions { get; set; } = AllowedQueryOptions.None;
        protected ODataHelperController(IODataHelperRepositoryAdapter<TEntity> repositoryAdapter)
        {
            RepositoryAdapter = repositoryAdapter;
        }

        public override async ValueTask<IHttpActionResult> Get(ODataQueryOptions<TEntity> options)
        {
            var result = await RepositoryAdapter
                .GetAsync(options, QuerySettings, IgnoreQueryOptions).ConfigureAwait(false);
            return Ok(result);
        }

        public override async ValueTask<IHttpActionResult> Get([FromODataUri] object key, 
            ODataQueryOptions<TEntity> options)
        {
            var result = await RepositoryAdapter
                .GetAsync(key, options, QuerySettings, IgnoreQueryOptions).ConfigureAwait(false);
            return Ok(result);
        }

        public override async ValueTask<IHttpActionResult> Delete([FromODataUri] object key)
        {
            var entityToBeDeleted = await RepositoryAdapter
                .GetAsync(key, null, QuerySettings, IgnoreQueryOptions).ConfigureAwait(false);
            if (await RepositoryAdapter.RemoveAsync(entityToBeDeleted).ConfigureAwait(false) <= 0) return BadRequest();
            await RepositoryAdapter.PersistChangesAsync().ConfigureAwait(false);
            return Ok();
        }

        public override async ValueTask<IHttpActionResult> Post([FromBody]TEntity postEntity)
        {
            var result = await RepositoryAdapter.InsertAsync(postEntity).ConfigureAwait(false);
            await RepositoryAdapter.PersistChangesAsync().ConfigureAwait(false);
            return Created(result);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public override async ValueTask<IHttpActionResult> Patch([FromODataUri] object key, Delta<TEntity> delta)
        {
            var patchEntity = await RepositoryAdapter
                .GetAsync(key, null, QuerySettings, IgnoreQueryOptions).ConfigureAwait(false);
            var result = await RepositoryAdapter.PatchAsync(patchEntity, delta).ConfigureAwait(false);
            await RepositoryAdapter.PersistChangesAsync().ConfigureAwait(false);
            return Updated(result);
        }

        public override async ValueTask<IHttpActionResult> Put([FromODataUri] object key, TEntity putEntity)
        {
            var result = await RepositoryAdapter.PutAsync(putEntity).ConfigureAwait(false);
            await RepositoryAdapter.PersistChangesAsync().ConfigureAwait(false);
            return Updated(result);
        }
    }
}
