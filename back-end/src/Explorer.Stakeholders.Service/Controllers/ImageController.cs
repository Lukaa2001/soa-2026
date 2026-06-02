using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers;

public class TextWrapper
{
    public string Text { get; set; } = string.Empty;
}

[Route("api/image")]
public class ImageController : BaseApiController
{
    private const string _imagesFolderPath = "Images";

    [HttpPost]
    public ActionResult<string> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                string filePath = Path.Combine(_imagesFolderPath, uniqueFileName);

                EnsureDirectoryExists();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                Result<TextWrapper> resultWrapper = Result.Ok(new TextWrapper { Text = uniqueFileName });
                return CreateResponse(resultWrapper);
            }

            return BadRequest("No file or empty file provided.");
        }
        catch (Exception ex)
        {
            return BadRequest("Error: " + ex.Message);
        }
    }

    [HttpGet("{imageURL}")]
    public IActionResult GetImage(string imageURL)
    {
        string imagePath = Path.Combine(_imagesFolderPath, imageURL);

        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound();
        }

        string contentType = Path.GetExtension(imagePath).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        var stream = System.IO.File.OpenRead(imagePath);
        return File(stream, contentType);
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_imagesFolderPath))
        {
            Directory.CreateDirectory(_imagesFolderPath);
        }
    }
}
