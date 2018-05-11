using System.Threading.Tasks;
using Zavolokas.Structures;

namespace InpaintService
{
    public class ReadOnlyStorage: IStorage
    {
        private readonly IStorage _storage;

        public ReadOnlyStorage()
        {
            _storage = new BlobStorage();
        }

        public void OpenContainer(string containerName)
        {
            _storage.OpenContainer(containerName);
        }

        public Task<ZsImage> ReadArgbImageAsync(string imageName)
        {
            return _storage.ReadArgbImageAsync(imageName);
        }

        public Task SaveImageLabAsync(ZsImage imageLab, string fileName)
        {
            return Task.CompletedTask;
        }

        public void Save<T>(T entity, string fileName)
        {
        }

        public T Read<T>(string fileName)
        {
            return _storage.Read<T>(fileName);
        }
    }
}