using System.Threading.Tasks;
using Zavolokas.Structures;

namespace InpaintService
{
    public interface IStorage
    {
        
        void OpenContainer(string containerName);
        Task<ZsImage> ReadArgbImageAsync(string imageName);
        Task SaveImageLabAsync(ZsImage imageLab, string fileName);
        void SaveJson(string data, string fileName);
        T Read<T>(string fileName);
    }
}