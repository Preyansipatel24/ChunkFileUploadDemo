namespace ChunkFileUploadDemo.Models
{
    public class ChunkModel
    {
        public int[] filebytes { get; set; }
        public decimal Chunk { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
    }
}
