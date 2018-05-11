namespace InpaintService
{
    public static class StorageFactory
    {
        public static IStorage CreateBlob()
        {
            //return new BlobStorage();
            return new ReadOnlyStorage();
        }

        public static IStorage Create()
        {
            //return new RedisStorage();
            return new BlobStorage();
        }
    }
}