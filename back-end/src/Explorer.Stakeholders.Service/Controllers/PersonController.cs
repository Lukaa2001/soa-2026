using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Explorer.API.Controllers;

[Route("api/person")]
public class PersonController : BaseApiController
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    // Functionality 4: a user can view their own profile.
    [HttpGet("{id:int}")]
    public ActionResult<UpdatePersonDTO> Get(int id)
    {
        var result = _personService.GetByUserId(id);
        return CreateResponse(result);
    }

    // Functionality 14: position simulator stores the tourist's current location.
    [Authorize(Policy = "touristAndAuthorPolicy")]
    [HttpPut("updateLocation")]
    public ActionResult<UpdatePersonDTO> UpdateLocation([FromBody] UpdatePersonDTO person)
    {
        var result = _personService.UpdatePerson(person, User.PersonId());
        return CreateResponse(result);
    }

    // Functionality 5: a user can edit their profile information (image is optional).
    [HttpPut, DisableRequestSizeLimit]
    public ActionResult<UpdatePersonDTO> Update([FromForm] UpdatePersonDTO person, IFormFile? image)
    {
        try
        {
            if (image != null && image.Length > 0)
            {
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                EnsureRepositoryFolderExists(pathToSave);

                var fileName = ContentDispositionHeaderValue.Parse(image.ContentDisposition).FileName!.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                person.Image = fileName;
            }
            // When no new file is uploaded, person.Image already carries the
            // existing image filename sent by the client, so it is preserved.
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }

        var result = _personService.UpdatePerson(person, User.PersonId());
        return CreateResponse(result);
    }

    private void EnsureRepositoryFolderExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
