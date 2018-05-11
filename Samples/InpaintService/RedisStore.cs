using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using StackExchange.Redis;
using Zavolokas.Structures;

namespace InpaintService
{
    public class RedisStorage : IStorage
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
        private string _container;

        static RedisStorage()
        {
            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString("Redis");

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }


        //var redis = RedisStore.RedisCache;

        //    if(redis.StringSet("testKey", "testValue"))
        //{
        //    var val = redis.StringGet("testKey");

        //    Console.WriteLine(val);
        //}

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase RedisCache => Connection.GetDatabase();


        public void OpenContainer(string containerName)
        {
            _container = containerName;
        }

        public Task<ZsImage> ReadArgbImageAsync(string imageName)
        {
            var data = Read<byte[]>(imageName);
            using (var imageData = new MemoryStream(data))
            using (var bitmap = new Bitmap(imageData))
            {
                return Task.FromResult(bitmap.ToArgbImage());
            }
        }

        public Task SaveImageLabAsync(ZsImage imageLab, string fileName)
        {
            return Task.Factory.StartNew(() =>
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
                    var data = outputStream.ToArray();
                    Save(data, fileName);
                }
            });
        }

        public void Save<T>(T entity, string fileName)
        {
            var data = JsonConvert.SerializeObject(entity);
            RedisCache.StringSet($"{_container}:{fileName}", data);
        }

        public T Read<T>(string fileName)
        {
            string json = RedisCache.StringGet($"{_container}:{fileName}");
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }
    }
}