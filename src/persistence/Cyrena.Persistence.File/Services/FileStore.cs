using Microsoft.Extensions.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Persistence.File.Services
{
    internal class FileStore<T> : IStore<T> where T : class, IEntity
    {
        private readonly IOptions<FilePersistenceOptions> _options;
        private readonly string _collectionName;
        private readonly IPersistenceFS _fs;

        public FileStore(IOptions<FilePersistenceOptions> options, IPersistenceFS fs)
        {
            _options = options;
            _fs = fs;
            _collectionName = typeof(T).Name;
        }

        public FileStore(IOptions<FilePersistenceOptions> options, IPersistenceFS fs, string collectionName)
        {
            _options = options;
            _fs = fs;
            _collectionName = collectionName;
        }

        public IQueryable<T> QueryableData { get { return _fs.Read<T>(_collectionName).AsQueryable(); } }

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid().ToString();
            _fs.Write(entity, _collectionName);
            return Task.CompletedTask;
        }

        public Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                if (entity.Id == null)
                    entity.Id = Guid.NewGuid().ToString();
                _fs.Write(entity, _collectionName);
            }
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var collection = _fs.Read<T>(_collectionName).AsQueryable();
            var cl = collection.Where(specification.ToExpression());
            return Task.FromResult(cl.Count());
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _fs.Delete<T>(entity.Id, _collectionName);
            return Task.CompletedTask;
        }

        public Task<int> DeleteManyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var collection = _fs.Read<T>(_collectionName).AsQueryable();
            var cl = collection.Where(specification.ToExpression());
            int count = cl.Count();
            foreach (var entity in collection)
                _fs.Delete<T>(entity.Id, _collectionName);
            return Task.FromResult(count);
        }

        public void Dispose()
        {

        }

        public Task<T?> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var collection = _fs.Read<T>(_collectionName).AsQueryable();
            var cl = collection.FirstOrDefault(specification.ToExpression());
            return Task.FromResult<T?>(cl);
        }

        public Task<IEnumerable<T>> FindManyAsync(ISpecification<T> specification, IOrderBy<T>? orderBy = null, IPaging? paging = null, CancellationToken cancellationToken = default)
        {
            var collection = _fs.Read<T>(_collectionName).AsQueryable();
            var cl = collection.Where(specification.ToExpression());
            if (orderBy != null)
            {
                if (orderBy.SortDirection == SortDirection.Ascending)
                    cl = cl.OrderBy(orderBy.OrderByExpression);
                else
                    cl = cl.OrderByDescending(orderBy.OrderByExpression);
            }
            if (paging != null)
                cl = cl.Skip(paging.Skip).Take(paging.Take);
            return Task.FromResult(cl.AsEnumerable());
        }

        public Task SaveAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid().ToString();
            _fs.Write<T>(entity, _collectionName);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _fs.Write<T>(entity, _collectionName);
            return Task.CompletedTask;
        }
    }
}
