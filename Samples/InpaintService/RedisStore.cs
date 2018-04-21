using System;
using Microsoft.Azure.WebJobs.Host;
using StackExchange.Redis;

namespace InpaintService
{
    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
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
    }
}