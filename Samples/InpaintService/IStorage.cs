using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Zavolokas.Structures;

namespace InpaintService
{
    public interface IStorage
    {
        Task<ZsImage> ConvertBlobToArgbImage(CloudBlob imageBlob);
        CloudBlobContainer OpenBlobContainer(string containerName);
        T ReadFromBlob<T>(string blobName, CloudBlobContainer container);
        Task SaveImageLabToBlob(ZsImage imageLab, CloudBlobContainer container, string fileName);
        void SaveJsonToBlob(string data, CloudBlobContainer container, string fileName);
    }
}