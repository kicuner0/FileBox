#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using FileBox.Models;
using System.IO.Compression;

namespace FileBox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileItemsController : ControllerBase
    {
        private readonly FileContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        public FileItemsController(FileContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileItemDTO>>> GetFileItems()
        {
            return await _context.FileItems.Select(x => ItemToDTO(x)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HttpResponseMessage>> GetFileItem(Guid id)
        {
            var fileItem = await _context.FileItems.FindAsync(id);

            if (fileItem == null)
            {
                return NotFound();
            }

            string tempFilePath = "/Files/temp" + fileItem.Id + fileItem.Type;

            using (var fileToDecompress = new FileStream(_appEnvironment.WebRootPath + fileItem.Path, FileMode.Open))
            {
                using (FileStream decompressedFileStream = System.IO.File.Create(_appEnvironment.WebRootPath + tempFilePath))
                {
                    using (GZipStream decompressionStream = new GZipStream(fileToDecompress,
                       CompressionMode.Decompress))
                    {
                        await decompressionStream.CopyToAsync(decompressedFileStream);
                    }   
                }
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(_appEnvironment.WebRootPath + tempFilePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(_appEnvironment.WebRootPath + tempFilePath);

            System.IO.File.Delete(_appEnvironment.WebRootPath + tempFilePath);

            return File(bytes, contentType, fileItem.Name);
        }

        [HttpPost]
        public async Task PostFileItem(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                var fileItem = new FileItem
                {
                    Name = Path.GetFileNameWithoutExtension(uploadedFile.FileName),
                    Type = Path.GetExtension(uploadedFile.FileName),
                    Size = uploadedFile.Length
                };

                _context.FileItems.Add(fileItem);
                await _context.SaveChangesAsync();

                _context.Entry(fileItem).State = EntityState.Modified;

                string tempFilePath = "/Files/temp" + fileItem.Id + fileItem.Type;
                string createFilePath = "/Files/" + fileItem.Id + ".gz";

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + tempFilePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
        
                }

                using (var fileToCompress = new FileStream(_appEnvironment.WebRootPath + tempFilePath, FileMode.Open))
                {
                    using (FileStream compressedFileStream = System.IO.File.Create(_appEnvironment.WebRootPath+createFilePath))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                           CompressionMode.Compress))
                        {
                            fileToCompress.CopyTo(compressionStream);
                        }
                    }
                }

                System.IO.File.Delete(_appEnvironment.WebRootPath + tempFilePath);

                fileItem.Path = createFilePath;
                
                await _context.SaveChangesAsync();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileItem(Guid id)
        {
            var fileItem = await _context.FileItems.FindAsync(id);
            if (fileItem == null)
            {
                return NotFound();
            }
            System.IO.File.Delete(_appEnvironment.WebRootPath + fileItem.Path);
            _context.FileItems.Remove(fileItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static FileItemDTO ItemToDTO(FileItem fileItem) => 
            new FileItemDTO
            {
                Id = fileItem.Id,
                Name = fileItem.Name,
                Type = fileItem.Type,
                Size = fileItem.Size
            };

        /*private async Task<FileStream> CompressAsync(FileStream sourceFile, string compressedFile)
        {
            // поток для чтения исходного файла
            using FileStream tempStream = new FileStream(sourceFile, FileMode.OpenOrCreate);
            // поток для записи сжатого файла

            // поток архивации
            using GZipStream compressionStream = new GZipStream(sourceFile, CompressionMode.Compress);
            return await sourceStream.CopyToAsync(compressionStream); // копируем байты из одного потока в другой
        }*/
    }
}
