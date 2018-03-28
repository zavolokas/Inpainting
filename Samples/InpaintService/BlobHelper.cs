using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Zavolokas.Structures;

namespace InpaintService
{
    public static class BlobHelper
    {
        public static T ReadFromBlob<T>(string blobName, CloudBlobContainer container)
        {
            var blob = container.GetBlockBlobReference(blobName);
            var json = blob.DownloadText();
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public static CloudBlobContainer OpenBlobContainer(string containerName)
        {
            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            return container;
        }

        public static void SaveJsonToBlob(string data, CloudBlobContainer container, string fileName)
        {
            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(data), false))
            {
                blob.UploadFromStream(stream);
            }
        }

        public static async Task SaveImageLabToBlob(ZsImage imageLab, CloudBlobContainer container, string fileName)
        {
            var argbImage = imageLab
                .Clone()
                .FromLabToRgb()
                .FromRgbToArgb(Area2D.Create(0, 0, imageLab.Width, imageLab.Height));

            using (var bitmap = argbImage.FromArgbToBitmap())
            using (var outputStream = new MemoryStream())
            {
                // modify image
                bitmap.Save(outputStream, ImageFormat.Png);

                // save the result back
                outputStream.Position = 0;
                var resultImageBlob = container.GetBlockBlobReference(fileName);
                await resultImageBlob.UploadFromStreamAsync(outputStream);
            }
        }

        public static Task<ZsImage> ConvertBlobToArgbImage(CloudBlob imageBlob)
        {
            using (var imageData = new MemoryStream())
            {
                var downloadTask = imageBlob.DownloadToStreamAsync(imageData);
                downloadTask.Wait();
                using (var bitmap = new Bitmap(imageData))
                {
                    return Task.FromResult(bitmap.ToArgbImage());
                }
            }
        }
    }
}