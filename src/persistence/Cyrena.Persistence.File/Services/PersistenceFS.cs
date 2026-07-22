using Microsoft.Extensions.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using System.Text.Json;

namespace Cyrena.Services
{
    internal class PersistenceFS : IPersistenceFS
    {
        private readonly FilePersistenceOptions _options;
        private readonly Dictionary<string, IList<IEntity>> _collections;
        private readonly object _lock = new object();
        public PersistenceFS(IOptions<FilePersistenceOptions> options)
        {
            _options = options.Value;
            _collections = new Dictionary<string, IList<IEntity>>();
        }

        public PersistenceFS(FilePersistenceOptions options)
        {
            _options = options;
            _collections = new Dictionary<string, IList<IEntity>>();
        }

        public IList<T> Read<T>(string collectionName)
            where T : class, IEntity
        {
            lock (_lock)
            {
                if (_collections.ContainsKey(collectionName))
                {
                    IList<IEntity> entities = _collections[collectionName];
                    var m = new List<T>();
                    foreach (var entity in entities)
                        if (entity is T t)
                            m.Add(t);
                    return m;
                }

                var collection = _options.LoadList<T>(collectionName);
                var mem = new List<IEntity>(collection);
                _collections.Add(collectionName, mem);
                return collection;
            }
        }

        public void Write<T>(T entity, string collectionName)
            where T : class, IEntity
        {
            lock (_lock)
            {
                _collections.TryGetValue(collectionName, out var collection);
                if (collection == null)
                {
                    var ext = _options.LoadList<T>(collectionName);
                    collection = new List<IEntity>(ext);
                    _collections.Add(collectionName, collection);
                }
                var item = collection.FirstOrDefault(x => x.Id == entity.Id);
                if (item == null)
                    collection!.Add(entity);
                else
                {
                    var index = collection.IndexOf(item);
                    if (index >= 0) collection[index] = entity;
                }
                //var json = JsonSerializer.Serialize(entity);
                var json = System.Text.Json.JsonSerializer.Serialize(entity, new JsonSerializerOptions() { WriteIndented = true });
                var path = Path.Combine(_options.BaseDirectory, collectionName, $"{entity.Id}.{_options.FileExtension}");
                File.WriteAllText(path, json);
            }
        }

        public void Delete<T>(string id, string collectionName)
            where T : class, IEntity
        {
            lock (_lock)
            {
                _collections.TryGetValue(collectionName, out var collection);
                if (collection == null)
                {
                    var ext = _options.LoadList<T>(collectionName);
                    collection = new List<IEntity>(ext);
                    _collections.Add(collectionName, collection);
                }
                var item = collection.FirstOrDefault(x => x.Id == id);
                if (item != null)
                    collection.Remove(item);
                var path = Path.Combine(_options.BaseDirectory, collectionName, $"{id}.{_options.FileExtension}");
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
