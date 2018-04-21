namespace InpaintService
{
    public static class StorageFactory
    {
        public static IStorage Create()
        {
            return new BlobStorage();
        }
    }
}