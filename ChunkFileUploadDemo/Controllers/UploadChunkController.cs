using ChunkFileUploadDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace ChunkFileUploadDemo.Controllers
{
    public class UploadChunkController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UploadChunkController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor;

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
                string reletivepath = GetRelativeRootPath();
                var savePath = Path.Combine("/", "Uploads", file.FileName);
                var savePath1 = Path.Combine(reletivepath, savePath);
                return Ok(savePath);
            }
            else
            {
                // If this is not the last chunk, return a 206 Partial Content response
                return StatusCode(206);

            }
        }
        public string GetRelativeRootPath()
        {
            //string directoryPath = "/files";
            string relativeRootPath = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host;
            return relativeRootPath;
        }


        [HttpGet]
        public IActionResult FileChunks(string filepath)
        {
            int totalChunks = 0;
            int chunkSize = 1024 * 1024;
            var filePath = Path.Combine(filepath, "SampleVideo_1280x720_30mb.mp4");
            GetChunkModel getChunkModel = new GetChunkModel();
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // Calculate the total number of chunks
                totalChunks = (int)Math.Ceiling((double)fileStream.Length / chunkSize);
                getChunkModel.chunkSize = chunkSize;
                getChunkModel.totalChunks = totalChunks;
                getChunkModel.filePath = filePath;
            }
            return Ok(getChunkModel);
        }

        [HttpGet]
        public IActionResult UploadFileInChunks(string filepath, int totalchunks, int chunksize, int chunkIndexs)
        {

            IFormFile file = null;
            byte[] bytes = null;
            int chunkSize = 1024 * 1024; // 1MB chunk size
            byte[] fileContents = null;
            var filePath = Path.Combine(filepath, "SampleVideo_1280x720_30mb.mp4");
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // Calculate the total number of chunks
                int totalChunks = (int)Math.Ceiling((double)fileStream.Length / chunkSize);

                // Loop through the chunks
                for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
                {
                    // Get the start and end index of the current chunk
                    int startIndex = chunkIndex * chunkSize;
                    int endIndex = (chunkIndex + 1) * chunkSize - 1;

                    // If this is the last chunk, adjust the end index
                    if (chunkIndex == totalChunks - 1)
                    {
                        endIndex = (int)file.Length - 1;
                    }

                    // Read the current chunk

                    fileStream.Seek(startIndex, SeekOrigin.Begin);
                    byte[] buffer = new byte[endIndex - startIndex + 1];
                    fileStream.ReadAsync(buffer, 0, buffer.Length);

                    // Write the current chunk to the output stream

                    using (var stream = new MemoryStream(buffer))
                    {
                        var file1 = new FormFile(stream, 0, buffer.Length, "file", "SampleVideo_1280x720_30mb.mp4")
                        {
                            Headers = new HeaderDictionary(),
                            //ContentType = contentType
                        };

                        // do something with the file, like save it to disk or process its contents
                        file = file1;
                    }


                    chunkIndex = chunkIndex;
                    chunkSize = chunkSize;
                    totalChunks = totalChunks;




                }
            }

            return Ok();
        }


        //[HttpGet]
        //public IActionResult FileChunksreturns(string filepath, int totalchunks, int chunksize, int chunkIndexs)
        //{

        //    IFormFile file = null;
        //    byte[] bytes = null;
        //    int chunkSize = chunksize; // 1MB chunk size
        //    byte[] fileContents = null;
        //    var filedata = "";
        //    var filePath = filepath;
        //    using (var fileStream = new FileStream(filePath, FileMode.Open))
        //    {
        //        // Calculate the total number of chunks
        //        int totalChunks = (int)Math.Ceiling((double)fileStream.Length / chunkSize);

        //        // Loop through the chunks
        //        for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
        //        {
        //            if (chunkIndex <= chunkIndexs)
        //            {
        //                // Get the start and end index of the current chunk
        //                int startIndex = chunkIndex * chunkSize;
        //                int endIndex = (chunkIndex + 1) * chunkSize - 1;

        //                // If this is the last chunk, adjust the end index
        //                if (chunkIndex == totalChunks - 1)
        //                {
        //                    endIndex = (int)file.Length - 1;
        //                }

        //                // Read the current chunk

        //                fileStream.Seek(startIndex, SeekOrigin.Begin);
        //                byte[] buffer = new byte[endIndex - startIndex + 1];
        //                fileStream.ReadAsync(buffer, 0, buffer.Length);

        //                // Write the current chunk to the output stream

        //                //using (var stream = new MemoryStream(buffer))
        //                //{
        //                //    var file1 = new FormFile(stream, 0, buffer.Length, "file", "SampleVideo_1280x720_30mb.mp4")
        //                //    {
        //                //        Headers = new HeaderDictionary(),
        //                //        //ContentType = contentType
        //                //    };

        //                //    // do something with the file, like save it to disk or process its contents
        //                //    file = file1;
        //                //}


        //                var result = new FileContentResult(buffer, "application/octet-stream");
        //                result.EnableRangeProcessing = true;
        //                result.FileDownloadName = Path.GetFileName(filePath);
        //                //return result;

        //            }
        //        }

        //    }
        //    return Ok();
        //}


        [HttpGet]
        public IActionResult FileChunksreturns(string filepath, int totalchunks, int chunksize, int chunkIndexs)
        {
            int chunkSize = chunksize; // 1MB chunk size
            var filePath = filepath;
            var startIndex = chunkIndexs * chunkSize;
            var endIndex = startIndex + chunkSize - 1;
            if (endIndex >= new FileInfo(filePath).Length)
            {
                endIndex = (int)new FileInfo(filePath).Length - 1;
            }
            var bufferLength = endIndex - startIndex + 1;
            var buffer = new byte[bufferLength];

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                fileStream.Seek(startIndex, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, bufferLength);
            }

            var fileStreamResult = new FileStreamResult(new MemoryStream(buffer), "application/octet-stream");
            fileStreamResult.EnableRangeProcessing = true;
            fileStreamResult.FileDownloadName = Path.GetFileName(filePath);
            return fileStreamResult;
        }

    }
}
