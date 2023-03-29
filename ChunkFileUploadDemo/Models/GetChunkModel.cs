namespace ChunkFileUploadDemo.Models
{
    public class GetChunkModel
    {
        public int totalChunks { get; set; }
        public int chunkSize { get; set; }
        public string filePath { get; set; }
    }
}
