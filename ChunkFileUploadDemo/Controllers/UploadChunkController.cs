using Microsoft.AspNetCore.Mvc;

namespace ChunkFileUploadDemo.Controllers
{
    public class UploadChunkController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostEnvironment;
        public UploadChunkController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file, int chunkIndex, int totalChunks, int chunkSize)
        {
            byte[] fileContents = null; // gets uploaded file contents
            var directoryPath = Path.Combine(_hostEnvironment.WebRootPath, "Uploads");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            var filePath = Path.Combine(directoryPath, file.FileName);
          
            if (chunkIndex == 0)
            {
                // If this is the first chunk, delete any existing file with the same name
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            else
            {
                fileContents = System.IO.File.ReadAllBytes(filePath); // get file content read 
            }
            using (var fileStream = new FileStream(filePath, FileMode.Append)) // file append for appending  file content
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);  // get file data in memory stream  
                    var buffer = memoryStream.ToArray();
                    byte[] finalbuffer = null;

                    if (chunkIndex > 0)
                    {
                        finalbuffer = new byte[fileContents.Length];
                        System.Array.Copy(fileContents, finalbuffer, fileContents.Length);
                        finalbuffer = finalbuffer.Concat(buffer).ToArray();
                    }
                    else
                    {
                        finalbuffer = new byte[buffer.Length];
                        System.Array.Copy(buffer, finalbuffer, buffer.Length);
                    }

                    var start = chunkIndex * chunkSize;
                    var length = finalbuffer.Length;
                    if (chunkIndex > 0)
                        length += start;

                    if (length < start)
                    {
                        throw new ArgumentOutOfRangeException(nameof(start), "The start index is greater than the length of the buffer.");
                    }

                    if (length - start < length)
                    {
                        length = finalbuffer.Length - start;
                    }

                    // Write the chunk to the file stream
                    fileStream.Write(finalbuffer, start, length);
                }
            }

            if (chunkIndex == totalChunks - 1)
            {
                // If this is the last chunk, the file is now complete
                return Ok(filePath);
            }
            else
            {
                // If this is not the last chunk, return a 206 Partial Content response
                return StatusCode(206);
                
            }
        }
    }
}
