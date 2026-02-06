using Newtonsoft.Json;
using Cyrena.Models;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class FilePersistenceExtensions
    {
        public static IEnumerable<T> LoadEnumerable<T>(this FilePersistenceOptions options, string collectionName)
            where T : class, IEntity
        {
            var path = Path.Combine(options.BaseDirectory, collectionName);
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path, $"*.{options.FileExtension}");
            var models = new List<T>();
            foreach( var file in files)
            {
                string json = File.ReadAllText(file);
                var model = JsonConvert.DeserializeObject<T>(json);
                if(model != null)
                    models.Add(model);
            }
            return models;
        }

        public static IList<T> LoadList<T>(this FilePersistenceOptions options, string collectionName)
            where T : class, IEntity
        {
            var path = Path.Combine(options.BaseDirectory, collectionName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path, $"*.{options.FileExtension}");
            var models = new List<T>();
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var model = JsonConvert.DeserializeObject<T>(json);
                if (model != null)
                    models.Add(model);
            }
            return models;
        }
    }
}
