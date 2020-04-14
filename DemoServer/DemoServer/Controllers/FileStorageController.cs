//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DemoServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DemoServer.Controllers
{
    [Route("api/v1/files")]
    [ApiController]
    public class FileStorageController : ControllerBase
    {
        private readonly ILogger _logger;
        private IConfiguration Configuration { get; }

        public FileStorageController(ILogger<FileStorageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        [Produces("application/json", "application/xml")]
        public IActionResult GetFileList()
        {
            var req = Request.Query;

            var filesInDirectory = Directory.GetFileSystemEntries(Configuration["FileStoragePath"], "*", SearchOption.AllDirectories);

            List<FileStorageFile> storageFiles = new List<FileStorageFile>();

            foreach (string file in filesInDirectory)
            {
                var fileInfo = new FileInfo(file);
                storageFiles.Add(
                    new FileStorageFile
                        { Name = fileInfo.Name, Size = fileInfo.Length.ToString() + " Bytes", LastChange = fileInfo.LastWriteTime.ToLongTimeString() });
            }

            var list = new FileStorageFiles
            {
                Files = storageFiles.ToArray()
            };

            if (req.ContainsKey("filenames"))
            {
                var filter = req["filenames"].First().Split(",");
                list.Files = list.Files.Where(p => filter.Contains(p.Name)).ToArray();
            }

            return Ok(list);
        }

        [HttpPost]
        [Route(@"{filename}")]
        [RequestSizeLimit(5242880000)]
        public virtual async Task<IActionResult> SaveFileFromStorageAsync(string filename = "")
        {
            if (filename == "")
            {
                return StatusCode(StatusCodes.Status400BadRequest, @"Missing Filename!");
            }

            try
            {
                FileStream DestinationStream = System.IO.File.Create(Configuration["FileStoragePath"] + filename);
                await Request.Body.CopyToAsync(DestinationStream);
                DestinationStream.Close();
                return Ok("File saved in storage!");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, @"File couldn't be stored!");
            }
        }

        [HttpGet]
        [Route(@"{filename}")]
        public virtual IActionResult GetFileFromStorage(string filename = "")
        {
            if (filename == "")
            {
                return StatusCode(StatusCodes.Status400BadRequest, @"Missing Filename!");
            }

            try
            {
                var data = System.IO.File.OpenRead(Configuration["FileStoragePath"] + filename);
                return new FileStreamResult(data, "application/octet-stream") { EnableRangeProcessing = true };
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, @"Couldn't get file from storage!");
            }
        }
    }
}