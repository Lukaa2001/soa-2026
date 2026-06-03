using Explorer.Tours.API.Dtos.Execution;
using Explorer.Tours.API.Public.TourExecution;
using Explorer.Tours.Service.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Tourist.Execution
{
    // Functionality 17: tour execution (start / progress / abandon).
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tourist/execution/tourExecution")]
    public class TourExecutionController : BaseApiController
    {
        private readonly ITourExecutionService _tourExecutionService;
        private readonly PurchaseClient _purchaseClient;

        public TourExecutionController(ITourExecutionService tourExecutionService, PurchaseClient purchaseClient)
        {
            _tourExecutionService = tourExecutionService;
            _purchaseClient = purchaseClient;
        }

        [HttpGet("{id:int}")]
        public ActionResult<TourExecutionDto> Get(int id)
        {
            var result = _tourExecutionService.Get(id);
            return CreateResponse(result);
        }

        // Start a tour -> creates a TourExecution session.
        // Prerequisite (functionality 17): the tour must have been purchased.
        [HttpPost()]
        public async Task<ActionResult<TourExecutionDto>> Create([FromBody] int tourId)
        {
            var userId = ClaimsPrincipalExtensions.UserId(User);
            if (!await _purchaseClient.HasPurchased(userId, tourId))
                return BadRequest("Tura nije kupljena - kupite je pre pokretanja.");

            var result = _tourExecutionService.Create(tourId, ClaimsPrincipalExtensions.PersonId(User));
            return CreateResponse(result);
        }

        [HttpPost("compositetour")]
        public ActionResult<TourExecutionDto> CreateForcompositeTour([FromBody] int tourId)
        {
            var result = _tourExecutionService.CreateForCompositeTour(tourId, ClaimsPrincipalExtensions.PersonId(User));
            return CreateResponse(result);
        }

        // Proximity check (frontend posts current position every ~10s).
        [HttpPatch("{id:int}/{checkpointId:int}")]
        public ActionResult<TourExecutionDto> UpdateProgress(int id, int checkpointId, [FromBody] TourExecutionUpdateDto currentPosition)
        {
            var result = _tourExecutionService.UpdateProgress(id, checkpointId, ClaimsPrincipalExtensions.PersonId(User), currentPosition);
            return CreateResponse(result);
        }

        [HttpPatch("abandon")]
        public ActionResult<TourExecutionDto> Abandon([FromBody] int id)
        {
            var result = _tourExecutionService.Abandon(id, ClaimsPrincipalExtensions.PersonId(User));
            return CreateResponse(result);
        }
    }
}
