using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChunkFileUploadDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileChunkController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public FileChunkController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
       
      

    }
}
